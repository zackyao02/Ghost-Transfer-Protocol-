using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class MediaPipeHandInput : MonoBehaviour
{
    [Serializable]
    private sealed class GestureMessage
    {
        public string type;
        public string gesture;
        public string trace_id;
        public string state;
        public float confidence;
        public float progress;
        public double timestamp;
    }

    public event Action<SkillType, string> SkillRequested;
    public event Action<ProtocolInputAction, string> ProtocolActionRequested;
    public event Action<bool> AvailabilityChanged;

    [SerializeField] private string host = "127.0.0.1";
    [SerializeField] private int port = 8765;
    [SerializeField] private float timeoutSeconds = 1f;

    private readonly ConcurrentQueue<GestureMessage> pendingMessages = new ConcurrentQueue<GestureMessage>();
    private readonly System.Collections.Generic.HashSet<string> seenTraceIds = new System.Collections.Generic.HashSet<string>();
    private CancellationTokenSource cancellation;
    private float lastMessageTime = -999f;
    private bool available;

    public bool IsAvailable { get { return available; } }

    private void Start()
    {
        cancellation = new CancellationTokenSource();
        _ = ReceiveLoop(cancellation.Token);
        SetAvailable(false);
        SimpleEventBus.RaiseRecognitionStatusChanged("连接本地手势服务中，键盘导演模式可用");
    }

    private void Update()
    {
        GestureMessage message;
        while (pendingMessages.TryDequeue(out message))
        {
            HandleMessage(message);
        }

        if (available && Time.unscaledTime - lastMessageTime > timeoutSeconds)
        {
            SetAvailable(false);
            SimpleEventBus.RaiseRecognitionStatusChanged("手势服务断开，已在 1 秒内回退键盘导演模式");
            SimpleEventBus.RaiseInputModeChanged("键盘 / 鼠标导演模式");
        }
    }

    private async Task ReceiveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(host, port);
                    using (NetworkStream stream = client.GetStream())
                    {
                        while (!token.IsCancellationRequested)
                        {
                            byte[] header = await ReadExact(stream, 4, token);
                            int length = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];
                            if (length <= 0 || length > 1024 * 1024)
                            {
                                throw new InvalidDataException("Invalid gesture packet length.");
                            }

                            byte[] body = await ReadExact(stream, length, token);
                            GestureMessage message = JsonUtility.FromJson<GestureMessage>(Encoding.UTF8.GetString(body));
                            if (message != null)
                            {
                                pendingMessages.Enqueue(message);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                await Task.Delay(250, token);
            }
        }
    }

    private static async Task<byte[]> ReadExact(NetworkStream stream, int length, CancellationToken token)
    {
        byte[] buffer = new byte[length];
        int offset = 0;
        while (offset < length)
        {
            token.ThrowIfCancellationRequested();
            int read = await stream.ReadAsync(buffer, offset, length - offset);
            if (read == 0)
            {
                throw new IOException("Gesture service closed the connection.");
            }

            offset += read;
        }

        return buffer;
    }

    private void HandleMessage(GestureMessage message)
    {
        lastMessageTime = Time.unscaledTime;
        SetAvailable(true);

        if (message.type == "gesture_candidate")
        {
            SimpleEventBus.RaiseRecognitionStatusChanged("手势候选: " + message.gesture + " " + Mathf.RoundToInt(message.progress * 100f) + "%");
            return;
        }

        if (message.type == "camera_status")
        {
            SimpleEventBus.RaiseRecognitionStatusChanged("手势服务: " + message.state);
            return;
        }

        if (message.type != "gesture_recognized" || string.IsNullOrEmpty(message.gesture))
        {
            return;
        }

        if (!string.IsNullOrEmpty(message.trace_id) && !seenTraceIds.Add(message.trace_id))
        {
            return;
        }

        ProtocolInputAction protocolAction;
        string protocolLabel;
        if (TryMapProtocolAction(message.gesture, out protocolAction, out protocolLabel))
        {
            SimpleEventBus.RaiseRecognitionStatusChanged("已识别: " + protocolLabel);
            ProtocolActionRequested?.Invoke(protocolAction, protocolLabel);
            return;
        }

        SkillType skill;
        string label;
        if (!TryMapGesture(message.gesture, out skill, out label))
        {
            return;
        }

        SimpleEventBus.RaiseRecognitionStatusChanged("已识别: " + label);
        SkillRequested?.Invoke(skill, label);
    }

    private static bool TryMapProtocolAction(string gesture, out ProtocolInputAction action, out string label)
    {
        switch (gesture)
        {
            case "Point": action = ProtocolInputAction.Point; label = "指向选择"; return true;
            case "Confirm": action = ProtocolInputAction.Confirm; label = "确认"; return true;
            default: action = ProtocolInputAction.Point; label = string.Empty; return false;
        }
    }

    private static bool TryMapGesture(string gesture, out SkillType skill, out string label)
    {
        switch (gesture)
        {
            case "SwordQi": skill = SkillType.SwordQi; label = "手势横划剑气"; return true;
            case "FireTalisman": skill = SkillType.FireTalisman; label = "手势画圈火符"; return true;
            case "OpenPalm": skill = SkillType.DeitySummon; label = "张掌请神"; return true;
            default: skill = SkillType.SwordQi; label = string.Empty; return false;
        }
    }

    private void SetAvailable(bool value)
    {
        if (available == value)
        {
            return;
        }

        available = value;
        AvailabilityChanged?.Invoke(available);
        if (available)
        {
            SimpleEventBus.RaiseInputModeChanged("摄像头手势 + 键盘兜底");
        }
    }

    private void OnDestroy()
    {
        if (cancellation != null)
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}
