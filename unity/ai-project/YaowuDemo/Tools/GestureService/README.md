# Python Gesture Service

分工 C 的本地视觉手势服务：摄像头/MediaPipe 只负责识别，Unity 通过 `127.0.0.1:8765` 接收长度头 + UTF-8 JSON。服务不发送 JPEG，不把摄像头线程放进 Unity。

## 运行

无摄像头合成模式（用于联调）：

```bash
cd unity/ai-project/YaowuDemo/Tools/GestureService
python -m ghost_gesture.cli --synthetic
```

真实摄像头模式：

```bash
python -m pip install -e ".[vision,dev]"
python -m ghost_gesture.cli --fps 30
```

记录 Gate 2 的候选/确认送达日志（JSONL，包含时间戳、置信度、`trace_id`、发送耗时）并进行人工摄像头验收：

```bash
python -m ghost_gesture.cli --fps 30 --audit-log gesture-audit.jsonl --manual-stats gesture-camera-stats.json
```

在上述模式中，每次开始一个人工试次时，在控制台输入手势名（如 `OpenPalm`、`Point`、`SwordQi` 或 `FireTalisman`）。识别到同名确认事件即计为命中；其他确认事件计为对应手势的误触；输入 `miss`、开始下一试次或 Ctrl+C 会把未命中的当前试次计为漏检。该统计依赖操作者标注，服务不会伪造真实摄像头结果。

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

在本目录执行（不依赖摄像头或 MediaPipe）：

```bash
python -m pytest -q
```

压力测试会重复候选输入并插入抖动/中立帧，覆盖候选去重、确认、冷却、中立复位、TCP 长度头协议、`trace_id` 审计字段与人工统计逻辑。
