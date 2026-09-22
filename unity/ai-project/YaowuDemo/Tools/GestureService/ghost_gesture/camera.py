from __future__ import annotations

import time
from collections.abc import Iterator

from .models import HandFrame, Landmark


class OptionalMediaPipeCamera:
    """Optional camera adapter. Core service remains testable without these packages."""

    def __init__(self, camera_index: int = 0, max_fps: int = 30) -> None:
        self.camera_index = camera_index
        self.max_fps = max(15, min(max_fps, 30))

    def frames(self) -> Iterator[HandFrame]:
        try:
            import cv2
            import mediapipe as mp
        except ImportError as exc:
            raise RuntimeError("install gesture_service[vision] to use camera mode") from exc

        cap = cv2.VideoCapture(self.camera_index)
        if not cap.isOpened():
            cap.release()
            raise RuntimeError(f"camera {self.camera_index} could not be opened")
        hands = mp.solutions.hands.Hands(max_num_hands=1, model_complexity=0, min_detection_confidence=0.5, min_tracking_confidence=0.5)
        interval = 1.0 / self.max_fps
        try:
            while True:
                started = time.monotonic()
                ok, image = cap.read()
                if not ok:
                    raise RuntimeError("camera frame read failed")
                result = hands.process(cv2.cvtColor(image, cv2.COLOR_BGR2RGB))
                if result.multi_hand_landmarks:
                    points = tuple(Landmark(p.x, p.y, p.z) for p in result.multi_hand_landmarks[0].landmark)
                    yield HandFrame(points, confidence=0.9)
                else:
                    yield None  # type: ignore[misc]
                time.sleep(max(0.0, interval - (time.monotonic() - started)))
        finally:
            hands.close()
            cap.release()
