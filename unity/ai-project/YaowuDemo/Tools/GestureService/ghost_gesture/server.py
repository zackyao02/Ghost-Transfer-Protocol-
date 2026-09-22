from __future__ import annotations

import asyncio
import contextlib
import logging
import time

from .audit import StructuredEventLogger
from .models import GestureEvent
from .protocol import encode_message

LOGGER = logging.getLogger(__name__)


class GestureTcpServer:
    def __init__(self, host: str = "127.0.0.1", port: int = 8765, audit_logger: StructuredEventLogger | None = None) -> None:
        self.host = host
        self.port = port
        self._clients: set[asyncio.StreamWriter] = set()
        self._server: asyncio.Server | None = None
        self._audit_logger = audit_logger

    @property
    def client_count(self) -> int:
        return len(self._clients)

    async def start(self) -> None:
        self._server = await asyncio.start_server(self._accept_client, self.host, self.port)
        sockets = self._server.sockets or []
        if sockets:
            self.port = int(sockets[0].getsockname()[1])

    async def _accept_client(self, reader: asyncio.StreamReader, writer: asyncio.StreamWriter) -> None:
        self._clients.add(writer)
        peer = writer.get_extra_info("peername")
        LOGGER.info("Unity client connected: %s", peer)
        try:
            while await reader.read(1024):
                pass
        except (ConnectionError, asyncio.CancelledError):
            pass
        finally:
            self._clients.discard(writer)
            writer.close()
            with contextlib.suppress(ConnectionError):
                await writer.wait_closed()

    async def broadcast(self, event: GestureEvent) -> None:
        started = time.monotonic()
        packet = encode_message(event.to_dict())
        for writer in tuple(self._clients):
            try:
                writer.write(packet)
                await writer.drain()
            except (ConnectionError, OSError):
                self._clients.discard(writer)
                writer.close()
        if self._audit_logger is not None:
            self._audit_logger.record(event, (time.monotonic() - started) * 1000, self.client_count)

    async def close(self) -> None:
        for writer in tuple(self._clients):
            writer.close()
            with contextlib.suppress(ConnectionError):
                await writer.wait_closed()
        self._clients.clear()
        if self._server is not None:
            self._server.close()
            await self._server.wait_closed()
            self._server = None
