from __future__ import annotations

import asyncio
import json
import math
import threading

from ghost_gesture.audit import StructuredEventLogger
from ghost_gesture.detector import GestureDetector, GestureStateMachine, Observation
from ghost_gesture.metrics import ManualGestureStats
from ghost_gesture.models import GestureEvent, HandFrame, Landmark
from ghost_gesture.protocol import decode_message, encode_message
from ghost_gesture.server import GestureTcpServer


def test_jitter_breaks_hold_and_cooldown_deduplicates() -> None:
    machine = GestureStateMachine(candidate_seconds=0.10, cooldown_seconds=0.50, neutral_seconds=0.05, point_seconds=0.10)
    point = Observation("Point", 0.9)
    first_candidate = machine.update(point, 0.00)[0]
    assert first_candidate.type == "gesture_candidate"
    machine.update(None, 0.09)  # one missing frame must reset the hold.
    second_candidate = machine.update(point, 0.10)[0]
    assert second_candidate.type == "gesture_candidate"
    recognized = machine.update(point, 0.21)[0]
    assert recognized.type == "gesture_recognized"
    assert recognized.trace_id == second_candidate.trace_id
    assert recognized.trace_id != first_candidate.trace_id  # jitter began a new candidate.
    assert machine.update(point, 0.22)[0].state == "COOLDOWN"
    machine.update(None, 0.73)
    assert machine.update(point, 0.79)[0].type == "gesture_candidate"


def test_repeated_input_stress_has_one_confirmation_per_neutral_reset() -> None:
    machine = GestureStateMachine(candidate_seconds=0.04, cooldown_seconds=0.08, neutral_seconds=0.02, point_seconds=0.04)
    point = Observation("Point", 0.9)
    recognized: list[GestureEvent] = []
    now = 0.0
    for _ in range(25):
        for _ in range(15):  # repeated stable input and cooldown frames
            recognized.extend(event for event in machine.update(point, now) if event.type == "gesture_recognized")
            now += 0.01
        machine.update(None, now)
        now += 0.03  # neutral reset before the next repeated trial
    assert len(recognized) == 25
    assert len({event.trace_id for event in recognized}) == 25


def test_protocol_and_structured_audit(tmp_path) -> None:
    event = GestureEvent("gesture_recognized", 12.5, "gesture-12500-0001", "Point", 0.91)
    assert decode_message(encode_message(event.to_dict())) == event.to_dict()
    path = tmp_path / "audit.jsonl"
    audit = StructuredEventLogger(path)
    audit.record(event, 1.2345, 1)
    row = json.loads(path.read_text(encoding="utf-8"))
    assert row == {"event": "gesture_recognized", "timestamp": 12.5, "gesture": "Point", "confidence": 0.91, "trace_id": "gesture-12500-0001", "send_ms": 1.234, "client_count": 1}


def test_manual_stats_records_hit_miss_and_false_trigger() -> None:
    stats = ManualGestureStats()
    stats.begin_expected("OpenPalm")
    stats.record(GestureEvent("gesture_recognized", 1, gesture="Point"))
    stats.close_expected()
    stats.begin_expected("Point")
    stats.record(GestureEvent("gesture_recognized", 2, gesture="Point"))
    assert stats.summary() == {"OpenPalm": {"hit": 0, "miss": 1, "false_trigger": 0}, "Point": {"hit": 1, "miss": 0, "false_trigger": 1}}


def test_manual_stats_serializes_console_and_recognition_threads() -> None:
    stats = ManualGestureStats()
    stats.begin_expected("Point")
    worker_started = threading.Event()

    def recognize() -> None:
        worker_started.set()
        stats.record(GestureEvent("gesture_recognized", 1, gesture="Point"))

    with stats._lock:
        worker = threading.Thread(target=recognize)
        worker.start()
        assert worker_started.wait(timeout=1)
        assert worker.is_alive()

    worker.join(timeout=1)
    assert not worker.is_alive()
    assert stats.summary()["Point"] == {"hit": 1, "miss": 0, "false_trigger": 0}


def test_tcp_broadcast_uses_length_prefixed_protocol() -> None:
    async def scenario() -> None:
        server = GestureTcpServer(port=0)
        await server.start()
        reader, writer = await asyncio.open_connection("127.0.0.1", server.port)
        for _ in range(20):
            if server.client_count:
                break
            await asyncio.sleep(0.01)
        event = GestureEvent("gesture_candidate", 1, gesture="Point", confidence=0.8)
        await server.broadcast(event)
        size = int.from_bytes(await reader.readexactly(4), "big")
        assert json.loads((await reader.readexactly(size)).decode("utf-8")) == event.to_dict()
        writer.close()
        await writer.wait_closed()
        await server.close()

    asyncio.run(scenario())


def _pointing_frame(index_x: float, index_y: float, timestamp: float, thumb_x: float = 2.0, thumb_y: float = 2.0) -> HandFrame:
    points = [Landmark(0.0, 0.0) for _ in range(21)]
    points[5], points[17] = Landmark(-0.5, 0.0), Landmark(0.5, 0.0)
    points[4] = Landmark(thumb_x, thumb_y)
    points[6], points[8] = Landmark(0.0, 0.30), Landmark(index_x, index_y)
    for pip, tip in ((10, 12), (14, 16), (18, 20)):
        points[pip], points[tip] = Landmark(0.0, 0.30), Landmark(0.0, 0.08)
    return HandFrame(tuple(points), timestamp=timestamp)


def test_detector_accepts_relaxed_pinch_distance() -> None:
    detector = GestureDetector()
    observation = detector.observe(_pointing_frame(0.0, 1.0, 0.0, thumb_x=0.43, thumb_y=1.0))
    assert observation.gesture == "Confirm"


def test_detector_recognizes_horizontal_sword_before_static_point() -> None:
    detector = GestureDetector()
    observations = [
        detector.observe(_pointing_frame(-0.55 + step * 0.10, 1.0, step * 0.05))
        for step in range(13)
    ]
    assert observations[-1].gesture == "SwordQi"


def test_detector_recognizes_full_circle() -> None:
    detector = GestureDetector()
    observations = []
    for step in range(22):
        angle = step * (2 * math.pi / 21)
        observations.append(detector.observe(_pointing_frame(0.55 * math.cos(angle), 1.0 + 0.55 * math.sin(angle), step * 0.05)))
    assert observations[-1].gesture == "FireTalisman"


def test_default_point_waits_for_motion_intent_window() -> None:
    machine = GestureStateMachine()
    point = Observation("Point", 0.9)
    assert machine.update(point, 0.0)[0].type == "gesture_candidate"
    assert machine.update(point, 0.20)[0].type == "gesture_candidate"
    assert machine.update(point, 0.39)[0].type == "gesture_recognized"
