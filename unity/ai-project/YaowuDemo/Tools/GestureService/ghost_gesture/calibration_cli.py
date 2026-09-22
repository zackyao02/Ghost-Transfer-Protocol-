from __future__ import annotations

import argparse
from functools import lru_cache
from pathlib import Path
import time

import numpy as np
from PIL import Image, ImageDraw, ImageFont

from .calibration import GestureCalibration, TrajectoryFeatures


@lru_cache(maxsize=4)
def _font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    font_path = Path("C:/Windows/Fonts/msyh.ttc")
    return ImageFont.truetype(str(font_path), size) if font_path.exists() else ImageFont.load_default()


def _draw(cv2, frame, title: str, detail: str, remaining: float):
    image = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    draw = ImageDraw.Draw(image)
    draw.rectangle((0, 0, image.width, 96), fill=(20, 20, 20))
    draw.text((16, 12), title, font=_font(28), fill=(255, 230, 0))
    draw.text((16, 47), detail, font=_font(20), fill=(255, 255, 255))
    draw.text((16, 73), f"剩余 {remaining:.1f} 秒（按 Q 取消）", font=_font(16), fill=(180, 220, 180))
    return cv2.cvtColor(np.asarray(image), cv2.COLOR_RGB2BGR)


def _capture_trial(cv2, cap, hands, name: str, instruction: str, trial: int, total: int, neutral_seconds: float, active_seconds: float):
    for phase, duration in (("准备", neutral_seconds), ("执行手势", active_seconds)):
        started = time.monotonic()
        samples: list[tuple[float, float, float, float]] = []
        pinches: list[float] = []
        palms: list[float] = []
        while time.monotonic() - started < duration:
            ok, frame = cap.read()
            if not ok:
                raise RuntimeError("camera frame read failed")
            if phase == "执行手势":
                result = hands.process(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
                if result.multi_hand_landmarks:
                    points = result.multi_hand_landmarks[0].landmark
                    palm = max(((points[5].x - points[17].x) ** 2 + (points[5].y - points[17].y) ** 2) ** 0.5, 1e-4)
                    pinch = ((points[4].x - points[8].x) ** 2 + (points[4].y - points[8].y) ** 2) ** 0.5 / palm
                    palms.append(palm)
                    pinches.append(pinch)
                    samples.append((time.time(), points[8].x, points[8].y, palm))
            display = _draw(cv2, frame, f"{name}：第 {trial}/{total} 次 · {phase}", instruction if phase == "执行手势" else "请放下手，等待下一次", duration - (time.monotonic() - started))
            cv2.imshow("幽灵传输协议 · 手势校准", display)
            if cv2.waitKey(1) & 0xFF == ord("q"):
                raise KeyboardInterrupt
        if phase == "执行手势":
            return palms, pinches, samples
    return [], [], []


def main() -> None:
    parser = argparse.ArgumentParser(description="幽灵传输协议本地个人手势校准")
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
    cv2.namedWindow("幽灵传输协议 · 手势校准", cv2.WINDOW_NORMAL)
    cv2.resizeWindow("幽灵传输协议 · 手势校准", 960, 720)
    palms: list[float] = []
    pinches: list[float] = []
    swords: list[TrajectoryFeatures] = []
    circles: list[TrajectoryFeatures] = []
    phases = (
        ("手掌尺寸", "张开手掌并保持", "palm"),
        ("捏合确认", "拇指与食指轻触并保持", "pinch"),
        ("横划剑诀", "食指先停留，再快速水平横划", "sword"),
        ("画圈净化符", "食指先停留，再连续画完整一圈", "circle"),
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
        print("校准已取消，不会写入个人配置。", flush=True)
        return
    finally:
        hands.close()
        cap.release()
        cv2.destroyAllWindows()

    profile = GestureCalibration.from_samples(palms, pinches, swords, circles)
    profile.save(args.output)
    print(f"个人校准已保存到 {args.output}", flush=True)
    print(profile, flush=True)


if __name__ == "__main__":
    main()
