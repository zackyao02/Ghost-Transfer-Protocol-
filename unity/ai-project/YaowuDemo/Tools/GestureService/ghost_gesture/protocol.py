from __future__ import annotations

import json
import struct
from typing import Any

MAX_MESSAGE_BYTES = 1_048_576


def encode_message(payload: dict[str, Any]) -> bytes:
    body = json.dumps(payload, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    if len(body) > MAX_MESSAGE_BYTES:
        raise ValueError("message exceeds maximum size")
    return struct.pack(">I", len(body)) + body


def decode_message(packet: bytes) -> dict[str, Any]:
    if len(packet) < 4:
        raise ValueError("packet is missing the length prefix")
    (size,) = struct.unpack(">I", packet[:4])
    if size > MAX_MESSAGE_BYTES or len(packet) != size + 4:
        raise ValueError("packet length is invalid")
    value = json.loads(packet[4:].decode("utf-8"))
    if not isinstance(value, dict):
        raise ValueError("message body must be a JSON object")
    return value
