from __future__ import annotations

import argparse
import asyncio
import logging
import time

from .camera import OptionalMediaPipeCamera
from .detector import GestureDetector, GestureStateMachine
from .models import GestureEvent
from .server import GestureTcpServer


async def run(args: argparse.Namespace) -> None:
    server = GestureTcpServer(args.host, args.port)
    detector, machine = GestureDetector(), GestureStateMachine()
    await server.start()
    await server.broadcast(GestureEvent("camera_status", time.time(), state="STARTING"))
    logging.info("gesture service listening on %s:%s", args.host, server.port)
    try:
        if args.synthetic:
            while True:
                await server.broadcast(GestureEvent("performance", time.time(), details={"fps": 0, "mode": "synthetic"}))
                await asyncio.sleep(1)
        else:
            for frame in OptionalMediaPipeCamera(args.camera, args.fps).frames():
                observation = detector.observe(frame) if frame is not None else None
                for event in machine.update(observation):
                    await server.broadcast(event)
    except (KeyboardInterrupt, asyncio.CancelledError):
        pass
    finally:
        await server.broadcast(GestureEvent("camera_status", time.time(), state="STOPPED"))
        await server.close()


def main() -> None:
    parser = argparse.ArgumentParser(description="Ghost Transfer Protocol gesture JSON service")
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=8765)
    parser.add_argument("--camera", type=int, default=0)
    parser.add_argument("--fps", type=int, default=30, choices=range(15, 31))
    parser.add_argument("--synthetic", action="store_true", help="run without camera or MediaPipe")
    parser.add_argument("--verbose", action="store_true")
    args = parser.parse_args()
    logging.basicConfig(level=logging.DEBUG if args.verbose else logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
    asyncio.run(run(args))


if __name__ == "__main__":
    main()
