# Python Gesture Service

分工 C 的本地视觉手势服务：摄像头/MediaPipe 只负责识别，Unity 通过 `127.0.0.1:8765` 接收长度头 + UTF-8 JSON。服务不发送 JPEG，不把摄像头线程放进 Unity。

## 运行

无摄像头合成模式（用于联调）：

```bash
cd gesture_service
python -m ghost_gesture.cli --synthetic
```

真实摄像头模式：

```bash
python -m pip install -e ".[vision,dev]"
python -m ghost_gesture.cli --fps 30
```

`--fps` 限制在 15～30；服务启动失败、摄像头断开或客户端断开都不会要求 Unity 退出。Unity 侧检测不到事件超过 1 秒后进入键盘导演模式。

## 协议

每个消息是：`4-byte unsigned big-endian body length + UTF-8 JSON body`。事件包括：

- `camera_status`: `STARTING` / `STOPPED`
- `hand_state`: `NO_HAND` / `READY` / `CANDIDATE` / `COOLDOWN`
- `gesture_candidate`: 预反馈，带 `gesture`、`confidence`、`progress`
- `gesture_recognized`: 确认事件，带唯一 `trace_id`
- `performance`: FPS 与模式
- `error`: 可扩展错误事件

动作映射：`OpenPalm`（校准/神降）、`Point`（选择）、`Confirm`（捏合）、`SwordQi`（横划）、`FireTalisman`（画圈）。`DeitySummon` 由 Unity 在 OpenPalm 当前流程上下文中映射。

## 测试

在仓库根目录执行：

```bash
python tests/run_tests.py
```

安装了 pytest 时也可执行 `python -m pytest -q`。测试不要求安装 MediaPipe 或连接摄像头，覆盖协议、状态机、trace_id、冷却和合成模式。
