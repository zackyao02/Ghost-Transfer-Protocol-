from __future__ import annotations

from collections import deque
from dataclasses import dataclass
import math
import time

from .calibration import GestureCalibration
from .models import GestureEvent, HandFrame, Landmark
from .trace import TraceIdFactory

TIP_IDS = (4, 8, 12, 16, 20)
PIP_IDS = (3, 6, 10, 14, 18)


def distance(a: Landmark, b: Landmark) -> float:
    return math.hypot(a.x - b.x, a.y - b.y)


def palm_width(points: tuple[Landmark, ...]) -> float:
    return max(distance(points[5], points[17]), 1e-4)


def finger_extended(points: tuple[Landmark, ...], tip: int, pip: int) -> bool:
    wrist = points[0]
    return distance(points[tip], wrist) > distance(points[pip], wrist) * 1.12


@dataclass(frozen=True)
class Observation:
    gesture: str | None
    confidence: float
    progress: float = 0.0


class GestureDetector:
    def __init__(self, calibration: GestureCalibration | None = None, history_size: int = 72, dynamic_window_seconds: float = 1.45) -> None:
        self._history: deque[tuple[float, float, float]] = deque(maxlen=history_size)
        self.calibration = calibration or GestureCalibration.default()
        self._dynamic_window_seconds = dynamic_window_seconds
        self._latched_dynamic: Observation | None = None
        self._dynamic_latch_until = 0.0
        self._point_still_since: float | None = None
        self._sequence_started_at: float | None = None
        self._sequence_armed_until = 0.0

    def reset(self) -> None:
        self._history.clear()
        self._latched_dynamic = None
        self._dynamic_latch_until = 0.0
        self._point_still_since = None
        self._sequence_started_at = None
        self._sequence_armed_until = 0.0

    def observe(self, frame: HandFrame) -> Observation:
        points = frame.landmarks
        palm = palm_width(points)
        self._history.append((frame.timestamp, points[8].x, points[8].y))
        self._trim_history(frame.timestamp)
        if self._latched_dynamic is not None and frame.timestamp < self._dynamic_latch_until:
            return self._latched_dynamic
        self._latched_dynamic = None
        extended = [finger_extended(points, tip, pip) for tip, pip in zip(TIP_IDS[1:], PIP_IDS[1:])]
        pinch = distance(points[4], points[8]) / palm
        # Evaluate OpenPalm before pinch. An open hand viewed at an angle can
        # make the thumb and index look close despite all fingers being open.
        if all(extended):
            self._clear_sequence()
            return Observation("OpenPalm", 0.92)
        if extended[0] and not any(extended[1:]):
            if self._motion_in_progress(palm):
                dynamic = self._dynamic(palm) if frame.timestamp <= self._sequence_armed_until else Observation(None, 0.0)
                if dynamic.gesture:
                    self._latched_dynamic = dynamic
                    self._dynamic_latch_until = frame.timestamp + 0.20
                    return dynamic
                self._point_still_since = None
                return Observation(None, 0.0)
            if pinch < self.calibration.pinch_threshold:
                self._clear_sequence()
                return Observation("Confirm", min(1.0, 0.60 + (self.calibration.pinch_threshold - pinch) / 0.28))
            if self._point_still_since is None:
                self._point_still_since = frame.timestamp
            if (self._sequence_started_at is None or frame.timestamp > self._sequence_armed_until) and frame.timestamp - self._point_still_since >= self.calibration.sequence_arm_seconds:
                self._sequence_started_at = frame.timestamp
                self._sequence_armed_until = frame.timestamp + self._dynamic_window_seconds
            return Observation("Point", 0.88)
        self._clear_sequence()
        if pinch < self.calibration.pinch_threshold:
            return Observation("Confirm", min(1.0, 0.60 + (self.calibration.pinch_threshold - pinch) / 0.28))
        return Observation(None, 0.0)

    def _dynamic(self, palm: float) -> Observation:
        history = self._node_trace(palm)
        if len(history) < 6:
            return Observation(None, 0.0)
        duration = history[-1][0] - history[0][0]
        if duration < 0.12:
            return Observation(None, 0.0)
        xs = [p[1] for p in history]
        ys = [p[2] for p in history]
        span_x, span_y = max(xs) - min(xs), max(ys) - min(ys)
        net_x = xs[-1] - xs[0]
        path = sum(math.hypot(b[1] - a[1], b[2] - a[2]) for a, b in zip(history, history[1:]))

        # Node-trajectory loop: use only the moving index-fingertip samples
        # after the staged Point arm. This keeps stationary Point frames from
        # distorting the loop centre and direction calculation.
        center_x, center_y = sum(xs) / len(xs), sum(ys) / len(ys)
        angles = [math.atan2(y - center_y, x - center_x) for x, y in zip(xs, ys)]
        rotation = sum((b - a + math.pi) % (2 * math.pi) - math.pi for a, b in zip(angles, angles[1:]))
        radius = sum(math.hypot(x - center_x, y - center_y) for x, y in zip(xs, ys)) / len(xs)
        directions = self._direction_sequence(history, palm)
        if (
            len(history) >= 12
            and span_x >= palm * (self.calibration.circle_min_span_ratio * 0.80)
            and span_y >= palm * (self.calibration.circle_min_span_ratio * 0.80)
            and radius >= palm * (self.calibration.circle_min_span_ratio * 0.35)
            and path >= palm * (self.calibration.circle_min_path_ratio * 0.70)
            and len(directions) >= 3
        ):
            progress = min(1.0, max(abs(rotation) / math.radians(180), len(directions) / 4))
            return Observation("FireTalisman", 0.82 + 0.18 * progress, progress)
        if (
            span_x >= palm * self.calibration.sword_min_span_ratio
            and span_x >= span_y * self.calibration.sword_axis_ratio
            and abs(net_x) >= palm * (self.calibration.sword_min_span_ratio * 0.80)
            and path <= span_x * 1.80
        ):
            return Observation("SwordQi", min(1.0, span_x / (1.6 * palm)))
        return Observation(None, 0.0)

    def _trim_history(self, now: float) -> None:
        cutoff = now - self._dynamic_window_seconds
        while self._history and self._history[0][0] < cutoff:
            self._history.popleft()

    def _motion_in_progress(self, palm: float) -> bool:
        history = self._node_trace(palm)
        if len(history) < 2:
            return False
        xs = [p[1] for p in history]
        ys = [p[2] for p in history]
        path = sum(math.hypot(b[1] - a[1], b[2] - a[2]) for a, b in zip(history, history[1:]))
        return max(max(xs) - min(xs), max(ys) - min(ys)) >= palm * 0.32 or path >= palm * 0.45

    def _node_trace(self, palm: float) -> list[tuple[float, float, float]]:
        history = list(self._history)
        if self._sequence_started_at is not None:
            history = [point for point in history if point[0] >= self._sequence_started_at]
        if len(history) < 2:
            return history
        origin = history[0]
        for index, point in enumerate(history[1:], start=1):
            if math.hypot(point[1] - origin[1], point[2] - origin[2]) >= palm * 0.12:
                return history[max(0, index - 1):]
        return history[:1]

    def _direction_sequence(self, history: list[tuple[float, float, float]], palm: float) -> list[str]:
        if len(history) < 2:
            return []
        labels: list[str] = []
        anchor = history[0]
        for point in history[1:]:
            dx, dy = point[1] - anchor[1], point[2] - anchor[2]
            if math.hypot(dx, dy) < palm * 0.20:
                continue
            label = ("右" if dx > 0 else "左") if abs(dx) >= abs(dy) else ("下" if dy > 0 else "上")
            if not labels or label != labels[-1]:
                labels.append(label)
            anchor = point
        return labels

    def _clear_sequence(self) -> None:
        self._point_still_since = None
        self._sequence_started_at = None
        self._sequence_armed_until = 0.0


class GestureStateMachine:
    def __init__(self, hold_seconds: float = 0.65, candidate_seconds: float = 0.12, cooldown_seconds: float = 0.75, neutral_seconds: float = 0.20, point_seconds: float = 0.38, dynamic_seconds: float = 0.08) -> None:
        self.hold_seconds, self.candidate_seconds = hold_seconds, candidate_seconds
        self.cooldown_seconds, self.neutral_seconds = cooldown_seconds, neutral_seconds
        self.point_seconds, self.dynamic_seconds = point_seconds, dynamic_seconds
        self.state = "NO_HAND"
        self._candidate: str | None = None
        self._candidate_trace: str | None = None
        self._candidate_since = 0.0
        self._cooldown_until = 0.0
        self._neutral_since: float | None = None
        self._armed = True
        self._trace = TraceIdFactory()

    def update(self, observation: Observation | None, now: float | None = None) -> list[GestureEvent]:
        now = time.time() if now is None else now
        if observation is None:
            self.state, self._candidate, self._candidate_trace = "NO_HAND", None, None
            self._neutral_since = self._neutral_since or now
            if now - self._neutral_since >= self.neutral_seconds:
                self._armed = True
            return [GestureEvent("hand_state", now, state=self.state, progress=0.0)]
        if now < self._cooldown_until:
            self.state = "COOLDOWN"
            return [GestureEvent("hand_state", now, state=self.state, progress=0.0)]
        if observation.gesture is None:
            self._neutral_since = self._neutral_since or now
            # A non-gesture frame breaks a hold; otherwise brief jitter can
            # incorrectly confirm a gesture after it has disappeared.
            self._candidate, self._candidate_trace = None, None
            if now - self._neutral_since >= self.neutral_seconds:
                self.state, self._armed = "READY", True
            return [GestureEvent("hand_state", now, state=self.state, progress=0.0)]
        if not self._armed:
            if self._neutral_since is not None and now - self._neutral_since >= self.neutral_seconds:
                self._armed = True
            else:
                self.state = "NEUTRAL_RESET"
                return [GestureEvent("hand_state", now, state=self.state, progress=0.0)]
        self._neutral_since = None
        if observation.gesture != self._candidate:
            self._candidate, self._candidate_since = observation.gesture, now
            self._candidate_trace = self._trace.next(now)
            self.state = "CANDIDATE"
        if observation.gesture == "OpenPalm":
            required = self.hold_seconds
        elif observation.gesture == "Point":
            required = self.point_seconds
        elif observation.gesture in {"SwordQi", "FireTalisman"}:
            required = self.dynamic_seconds
        else:
            required = self.candidate_seconds
        elapsed = now - self._candidate_since
        progress = max(observation.progress, min(1.0, elapsed / required))
        if elapsed < required:
            return [GestureEvent("gesture_candidate", now, trace_id=self._candidate_trace, gesture=observation.gesture, confidence=observation.confidence, state="CANDIDATE", progress=progress)]
        self.state, self._cooldown_until, self._armed = "CONFIRMED", now + self.cooldown_seconds, False
        return [GestureEvent("gesture_recognized", now, trace_id=self._candidate_trace, gesture=observation.gesture, confidence=observation.confidence, state="CONFIRMED", progress=1.0)]


def frame_from_xy(points: list[tuple[float, float]], timestamp: float) -> HandFrame:
    if len(points) != 21:
        raise ValueError("expected 21 x/y points")
    return HandFrame(tuple(Landmark(x, y) for x, y in points), timestamp=timestamp)
