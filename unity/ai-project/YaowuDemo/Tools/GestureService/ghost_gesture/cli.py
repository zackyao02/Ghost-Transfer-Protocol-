from __future__ import annotations

import argparse
import asyncio
import logging
from pathlib import Path
import threading
import time

from .audit import StructuredEventLogger
from .camera import OptionalMediaPipeCamera
from .calibration import GestureCalibration
from .detector import GestureDetector, GestureStateMachine
from .models import GestureEvent
from .metrics import ManualGestureStats
from .server import GestureTcpServer


async def run(args: argparse.Namespace) -> None:
    stats = ManualGestureStats() if args.manual_stats else None
    server = GestureTcpServer(args.host, args.port, StructuredEventLogger(args.audit_log))
    calibration_path = Path(args.calibration_file)
    calibration = GestureCalibration.load(calibration_path) if calibration_path.exists() else GestureCalibration.default()
    detector = GestureDetector(calibration)
    machine = GestureStateMachine(point_seconds=calibration.point_hold_seconds)
    await server.start()
    await server.broadcast(GestureEvent("camera_status", time.time(), state="STARTING"))
    logging.info("gesture service listening on %s:%s", args.host, server.port)
    logging.info("gesture calibration: %s", calibration_path if calibration_path.exists() else "defaults")
    if stats is not None:
        _start_manual_stats_console(stats, args.manual_stats)
    try:
        if args.synthetic:
            while True:
                await server.broadcast(GestureEvent("performance", time.time(), details={"fps": 0, "mode": "synthetic"}))
                await asyncio.sleep(1)
        else:
            for frame in OptionalMediaPipeCamera(args.camera, args.fps).frames():
                if frame is None:
                    detector.reset()
                    observation = None
                else:
                    observation = detector.observe(frame)
                for event in machine.update(observation):
                    await server.broadcast(event)
                    stats.record(event) if stats is not None else None
    except (KeyboardInterrupt, asyncio.CancelledError):
        pass
    finally:
        if stats is not None:
            stats.close_expected()
            stats.write(args.manual_stats)
            logging.info("manual gesture stats written to %s", args.manual_stats)
        await server.broadcast(GestureEvent("camera_status", time.time(), state="STOPPED"))
        await server.close()


def main() -> None:
    parser = argparse.ArgumentParser(description="Ghost Transfer Protocol gesture JSON service")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=8765)
    parser.add_argument("--camera", type=int, default=0)
    parser.add_argument("--fps", type=int, default=30, choices=range(15, 31))
    parser.add_argument("--calibration-file", default="gesture-calibration.json", help="load personal gesture thresholds when the file exists")
    parser.add_argument("--synthetic", action="store_true", help="run without camera or MediaPipe")
    parser.add_argument("--audit-log", help="write candidate/confirmation JSONL delivery logs")
    parser.add_argument("--manual-stats", metavar="PATH", help="operator-labelled camera hit/miss/false-trigger JSON report")
    parser.add_argument("--verbose", action="store_true")
    args = parser.parse_args()
    logging.basicConfig(level=logging.DEBUG if args.verbose else logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    asyncio.run(run(args))


def _start_manual_stats_console(stats: ManualGestureStats, output_path: str) -> None:
    def command_loop() -> None:
        logging.info("manual stats: type a gesture name to begin a trial; type 'miss' to close it; Ctrl+C stops and writes %s", output_path)
        while True:
            try:
                command = input().strip()
            except (EOFError, KeyboardInterrupt):
                return
            if command == "miss":
                stats.close_expected()
            elif command:
                stats.begin_expected(command)

    threading.Thread(target=command_loop, daemon=True, name="manual-gesture-stats").start()


if __name__ == "__main__":
    main()
