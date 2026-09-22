from __future__ import annotations

import json
import threading
from collections import defaultdict
from pathlib import Path

from .models import GestureEvent


class ManualGestureStats:
    """Operator-labelled camera acceptance counts; it never invents ground truth."""

    def __init__(self) -> None:
        self._counts: dict[str, dict[str, int]] = defaultdict(lambda: {"hit": 0, "miss": 0, "false_trigger": 0})
        self._expected: str | None = None
        self._lock = threading.RLock()

    def begin_expected(self, gesture: str) -> None:
        with self._lock:
            self.close_expected()
            self._expected = gesture

    def record(self, event: GestureEvent) -> None:
        with self._lock:
            if event.type != "gesture_recognized" or not event.gesture:
                return
            if self._expected == event.gesture:
                self._counts[event.gesture]["hit"] += 1
                self._expected = None
            else:
                self._counts[event.gesture]["false_trigger"] += 1

    def close_expected(self) -> None:
        with self._lock:
            if self._expected is not None:
                self._counts[self._expected]["miss"] += 1
                self._expected = None

    def summary(self) -> dict[str, dict[str, int]]:
        with self._lock:
            return {gesture: dict(counts) for gesture, counts in sorted(self._counts.items())}

    def write(self, path: str | Path) -> None:
        Path(path).write_text(json.dumps(self.summary(), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
