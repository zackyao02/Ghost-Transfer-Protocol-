from __future__ import annotations

import time


class TraceIdFactory:
    def __init__(self) -> None:
        self._sequence = 0

    def next(self, now: float | None = None) -> str:
        self._sequence += 1
        millis = int((time.time() if now is None else now) * 1000)
        return f"gesture-{millis}-{self._sequence:04d}"
