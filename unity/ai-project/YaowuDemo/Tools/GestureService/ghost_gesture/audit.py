from __future__ import annotations

import json
import logging
from pathlib import Path
from typing import Any

from .models import GestureEvent

LOGGER = logging.getLogger(__name__)


class StructuredEventLogger:
    """Writes candidate/confirmation delivery facts as one JSON object per line."""

    def __init__(self, path: str | Path | None = None) -> None:
        self._path = Path(path) if path else None

    def record(self, event: GestureEvent, send_ms: float, client_count: int) -> None:
        if event.type not in {"gesture_candidate", "gesture_recognized"}:
            return
        record: dict[str, Any] = {
            "event": event.type,
            "timestamp": event.timestamp,
            "gesture": event.gesture,
            "confidence": event.confidence,
            "trace_id": event.trace_id,
            "send_ms": round(send_ms, 3),
            "client_count": client_count,
        }
        line = json.dumps(record, ensure_ascii=False, separators=(",", ":"))
        LOGGER.info("gesture_audit=%s", line)
        if self._path is not None:
            with self._path.open("a", encoding="utf-8") as output:
                output.write(line + "\n")
