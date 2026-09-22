from __future__ import annotations

from dataclasses import asdict, dataclass, field
import time
from typing import Any


@dataclass(frozen=True)
class Landmark:
    x: float
    y: float
    z: float = 0.0


@dataclass(frozen=True)
class HandFrame:
    landmarks: tuple[Landmark, ...]
    timestamp: float = field(default_factory=time.time)
    handedness: str = "Unknown"
    confidence: float = 1.0

    def __post_init__(self) -> None:
        if len(self.landmarks) != 21:
            raise ValueError("a hand frame must contain exactly 21 landmarks")


@dataclass(frozen=True)
class GestureEvent:
    type: str
    timestamp: float
    trace_id: str | None = None
    gesture: str | None = None
    confidence: float | None = None
    state: str | None = None
    progress: float | None = None
    details: dict[str, Any] = field(default_factory=dict)

    def to_dict(self) -> dict[str, Any]:
        payload = {k: v for k, v in asdict(self).items() if v is not None and k != "details"}
        payload.update(self.details)
        return payload
