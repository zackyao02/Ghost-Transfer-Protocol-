from __future__ import annotations

from dataclasses import asdict, dataclass
import json
import math
from pathlib import Path
from statistics import median
from typing import Iterable


def _clamp(value: float, low: float, high: float) -> float:
    return max(low, min(value, high))


@dataclass(frozen=True)
class TrajectoryFeatures:
    span_x_ratio: float
    span_y_ratio: float
    axis_ratio: float
    path_ratio: float
    rotation_degrees: float

    @classmethod
    def from_samples(cls, samples: Iterable[tuple[float, float, float, float]]) -> "TrajectoryFeatures | None":
        rows = list(samples)
        if len(rows) < 6:
            return None
        palms = [row[3] for row in rows if row[3] > 0]
        if not palms:
            return None
        palm = median(palms)
        xs, ys = [row[1] for row in rows], [row[2] for row in rows]
        span_x, span_y = max(xs) - min(xs), max(ys) - min(ys)
        path = sum(math.hypot(b[1] - a[1], b[2] - a[2]) for a, b in zip(rows, rows[1:]))
        center_x, center_y = sum(xs) / len(xs), sum(ys) / len(ys)
        angles = [math.atan2(y - center_y, x - center_x) for x, y in zip(xs, ys)]
        rotation = sum((b - a + math.pi) % (2 * math.pi) - math.pi for a, b in zip(angles, angles[1:]))
        return cls(
            span_x_ratio=span_x / palm,
            span_y_ratio=span_y / palm,
            axis_ratio=max(span_x, span_y) / max(min(span_x, span_y), 1e-4),
            path_ratio=path / palm,
            rotation_degrees=abs(math.degrees(rotation)),
        )


@dataclass(frozen=True)
class GestureCalibration:
    """Derived personal thresholds; no frames or landmark histories are persisted."""

    schema_version: int = 1
    palm_width: float = 0.20
    pinch_threshold: float = 0.50
    sword_min_span_ratio: float = 0.60
    sword_axis_ratio: float = 1.70
    circle_min_rotation_degrees: float = 150.0
    circle_min_span_ratio: float = 0.50
    circle_min_path_ratio: float = 1.40
    point_hold_seconds: float = 0.65
    sequence_arm_seconds: float = 0.18

    @classmethod
    def default(cls) -> "GestureCalibration":
        return cls()

    @classmethod
    def load(cls, path: str | Path) -> "GestureCalibration":
        payload = json.loads(Path(path).read_text(encoding="utf-8"))
        allowed = {field: payload[field] for field in cls.__dataclass_fields__ if field in payload}
        return cls(**allowed)

    def save(self, path: str | Path) -> None:
        Path(path).write_text(json.dumps(asdict(self), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    @classmethod
    def from_samples(
        cls,
        palm_widths: Iterable[float],
        pinch_ratios: Iterable[float],
        sword_samples: Iterable[TrajectoryFeatures],
        circle_samples: Iterable[TrajectoryFeatures],
    ) -> "GestureCalibration":
        base = cls.default()
        palms = list(palm_widths)
        pinches = list(pinch_ratios)
        swords = list(sword_samples)
        circles = list(circle_samples)
        return cls(
            palm_width=median(palms) if palms else base.palm_width,
            pinch_threshold=_clamp(median(pinches) + 0.08, 0.34, 0.70) if pinches else base.pinch_threshold,
            sword_min_span_ratio=_clamp(median(row.span_x_ratio for row in swords) * 0.60, 0.35, 1.20) if swords else base.sword_min_span_ratio,
            sword_axis_ratio=_clamp(median(row.axis_ratio for row in swords) * 0.55, 1.20, 3.50) if swords else base.sword_axis_ratio,
            circle_min_rotation_degrees=_clamp(median(row.rotation_degrees for row in circles) * 0.60, 110.0, 300.0) if circles else base.circle_min_rotation_degrees,
            circle_min_span_ratio=_clamp(median(min(row.span_x_ratio, row.span_y_ratio) for row in circles) * 0.55, 0.30, 1.10) if circles else base.circle_min_span_ratio,
            circle_min_path_ratio=_clamp(median(row.path_ratio for row in circles) * 0.55, 0.80, 5.00) if circles else base.circle_min_path_ratio,
        )
