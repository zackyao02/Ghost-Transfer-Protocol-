from __future__ import annotations

from collections import deque
from dataclasses import dataclass
import math
import time

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
    def __init__(self, history_size: int = 36) -> None:
        self._history: deque[tuple[float, float, float]] = deque(maxlen=history_size)

    def reset(self) -> None:
        self._history.clear()

    def observe(self, frame: HandFrame) -> Observation:
        points = frame.landmarks
        palm = palm_width(points)
        self._history.append((frame.timestamp, points[8].x, points[8].y))
        pinch = distance(points[4], points[8]) / palm
        if pinch < 0.34:
            return Observation("Confirm", min(1.0, 0.65 + (0.34 - pinch) / 0.20))
        extended = [finger_extended(points, tip, pip) for tip, pip in zip(TIP_IDS[1:], PIP_IDS[1:])]
        if all(extended):
            return Observation("OpenPalm", 0.92)
        if extended[0] and not any(extended[1:]):
            dynamic = self._dynamic(palm)
            return dynamic if dynamic.gesture else Observation("Point", 0.88)
        return Observation(None, 0.0)

    def _dynamic(self, palm: float) -> Observation:
        if len(self._history) < 8:
            return Observation(None, 0.0)
        history = list(self._history)
        duration = history[-1][0] - history[0][0]
        if duration <= 0 or duration > 1.8:
            return Observation(None, 0.0)
        xs = [p[1] for p in history]
        ys = [p[2] for p in history]
        dx, dy = xs[-1] - xs[0], ys[-1] - ys[0]
        path = sum(math.hypot(b[1] - a[1], b[2] - a[2]) for a, b in zip(history, history[1:]))
        if abs(dx) > 1.7 * palm and abs(dx) > 2.2 * abs(dy) and path < abs(dx) * 1.8:
            return Observation("SwordQi", min(1.0, abs(dx) / (2.6 * palm)))
        center_x, center_y = sum(xs) / len(xs), sum(ys) / len(ys)
        angles = [math.atan2(y - center_y, x - center_x) for x, y in zip(xs, ys)]
        rotation = sum((b - a + math.pi) % (2 * math.pi) - math.pi for a, b in zip(angles, angles[1:]))
        radius = sum(math.hypot(x - center_x, y - center_y) for x, y in zip(xs, ys)) / len(xs)
        if abs(rotation) >= math.radians(270) and radius >= palm * 0.45:
            progress = min(1.0, abs(rotation) / (2 * math.pi))
            return Observation("FireTalisman", 0.82 + 0.18 * progress, progress)
        return Observation(None, 0.0)


class GestureStateMachine:
    def __init__(self, hold_seconds: float = 0.65, candidate_seconds: float = 0.12, cooldown_seconds: float = 0.75, neutral_seconds: float = 0.20) -> None:
        self.hold_seconds, self.candidate_seconds = hold_seconds, candidate_seconds
        self.cooldown_seconds, self.neutral_seconds = cooldown_seconds, neutral_seconds
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
        required = self.hold_seconds if observation.gesture == "OpenPalm" else self.candidate_seconds
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
