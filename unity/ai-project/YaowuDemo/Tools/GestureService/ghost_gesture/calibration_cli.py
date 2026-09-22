from __future__ import annotations

import argparse
import time

from .calibration import GestureCalibration, TrajectoryFeatures


def _draw(cv2, frame, title: str, detail: str, remaining: float) -> None:
    cv2.rectangle(frame, (0, 0), (frame.shape[1], 84), (20, 20, 20), -1)
    cv2.putText(frame, title, (16, 32), cv2.FONT_HERSHEY_SIMPLEX, 0.75, (0, 255, 255), 2)
    cv2.putText(frame, detail, (16, 58), cv2.FONT_HERSHEY_SIMPLEX, 0.50, (255, 255, 255), 1)
    cv2.putText(frame, f"{remaining:.1f}s  (Q: cancel)", (16, 78), cv2.FONT_HERSHEY_SIMPLEX, 0.42, (180, 220, 180), 1)


def _capture_trial(cv2, cap, hands, name: str, instruction: str, trial: int, total: int, neutral_seconds: float, active_seconds: float):
    for phase, duration in (("Prepare", neutral_seconds), ("Do gesture", active_seconds)):
        started = time.monotonic()
        samples: list[tuple[float, float, float, float]] = []
        pinches: list[float] = []
        palms: list[float] = []
        while time.monotonic() - started < duration:
            ok, frame = cap.read()
            if not ok:
                raise RuntimeError("camera frame read failed")
            if phase == "Do gesture":
                result = hands.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
                if result.multi_hand_landmarks:
                    points = result.multi_hand_landmarks[0].landmark
                    palm = max(((points[5].x - points[17].x) ** 2 + (points[5].y - points[17].y) ** 2) ** 0.5, 1e-4)
                    pinch = ((points[4].x - points[8].x) ** 2 + (points[4].y - points[8].y) ** 2) ** 0.5 / palm
                    palms.append(palm)
                    pinches.append(pinch)
                    samples.append((time.time(), points[8].x, points[8].y, palm))
            _draw(cv2, frame, f"{name} {trial}/{total}: {phase}", instruction if phase == "Do gesture" else "Lower your hand", duration - (time.monotonic() - started))
            cv2.imshow("Ghost Gesture Calibration", frame)
            if cv2.waitKey(1) & 0xFF == ord("q"):
                raise KeyboardInterrupt
        if phase == "Do gesture":
            return palms, pinches, samples
    return [], [], []


def main() -> None:
    parser = argparse.ArgumentParser(description="Local personal calibration for Ghost Gesture Protocol")
    parser.add_argument("--output", default="gesture-calibration.json")
    parser.add_argument("--camera", type=int, default=0)
    parser.add_argument("--trials", type=int, default=5)
    args = parser.parse_args()

    try:
        import cv2
        import mediapipe as mp
    except ImportError as exc:
        raise RuntimeError("install gesture_service[vision] to run calibration") from exc

    cap = cv2.VideoCapture(args.camera, cv2.CAP_DSHOW)
    if not cap.isOpened():
        raise RuntimeError(f"camera {args.camera} could not be opened")
    hands = mp.solutions.hands.Hands(max_num_hands=1, model_complexity=0, min_detection_confidence=0.5, min_tracking_confidence=0.5)
    cv2.namedWindow("Ghost Gesture Calibration", cv2.WINDOW_NORMAL)
    cv2.resizeWindow("Ghost Gesture Calibration", 960, 720)
    palms: list[float] = []
    pinches: list[float] = []
    swords: list[TrajectoryFeatures] = []
    circles: list[TrajectoryFeatures] = []
    phases = (
        ("Palm", "Open palm and hold", "palm"),
        ("Confirm", "Pinch thumb and index finger", "pinch"),
        ("SwordQi", "Point briefly, then swipe horizontally", "sword"),
        ("FireTalisman", "Point briefly, then draw one circle", "circle"),
    )
    try:
        for name, instruction, kind in phases:
            for trial in range(1, args.trials + 1):
                print(f"{name} {trial}/{args.trials}: {instruction}", flush=True)
                widths, ratios, trajectory = _capture_trial(cv2, cap, hands, name, instruction, trial, args.trials, 0.8, 1.8)
                palms.extend(widths)
                if kind == "pinch" and ratios:
                    pinches.append(min(ratios))
                features = TrajectoryFeatures.from_samples(trajectory)
                if kind == "sword" and features is not None:
                    swords.append(features)
                if kind == "circle" and features is not None:
                    circles.append(features)
    except KeyboardInterrupt:
        print("Calibration cancelled; no profile was written.", flush=True)
        return
    finally:
        hands.close()
        cap.release()
        cv2.destroyAllWindows()

    profile = GestureCalibration.from_samples(palms, pinches, swords, circles)
    profile.save(args.output)
    print(f"Saved calibration to {args.output}", flush=True)
    print(profile, flush=True)


if __name__ == "__main__":
    main()
