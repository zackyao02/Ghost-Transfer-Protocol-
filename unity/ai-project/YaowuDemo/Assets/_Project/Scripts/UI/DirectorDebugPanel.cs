using UnityEngine;

public sealed class DirectorDebugPanel : MonoBehaviour
{
    private KeyboardSkillInput keyboard;
    private MediaPipeHandInput mediaPipe;
    private WorldStateStore world;
    private bool visible;
    private GUIStyle textStyle;
    private GUIStyle headerStyle;

    public void Initialize(KeyboardSkillInput input, MediaPipeHandInput handInput, WorldStateStore state)
    {
        keyboard = input;
        mediaPipe = handInput;
        world = state;
        keyboard.DebugToggleRequested += Toggle;
        keyboard.PresetLoadRequested += LoadPreset;
    }

    private void OnDestroy()
    {
        if (keyboard != null)
        {
            keyboard.DebugToggleRequested -= Toggle;
            keyboard.PresetLoadRequested -= LoadPreset;
        }
    }

    private void Toggle()
    {
        visible = !visible;
        SimpleEventBus.RaiseRecognitionStatusChanged(visible ? "调试面板已开启" : "调试面板已关闭");
    }

    private void LoadPreset(string preset)
    {
        world.LoadPreset(preset);
        string label = preset == "kind" ? "善意 Ghost 评审存档" : "敌意 Ghost 评审存档";
        SimpleEventBus.RaiseRecognitionStatusChanged("已载入 " + label);
    }

    private void OnGUI()
    {
        if (!visible || world == null)
        {
            return;
        }

        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.7f, 1.2f);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        float width = Screen.width / scale;
        PersistentWorldState state = world.Current;
        Rect panel = new Rect(width - 348f, 92f, 320f, 176f);
        GUI.color = new Color(0.025f, 0.04f, 0.06f, 0.95f); GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = new Color(0.95f, 0.28f, 0.35f); GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 2f), Texture2D.whiteTexture);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 11f, 290f, 20f), "DIRECTOR DEBUG // F1", headerStyle);
        GUI.color = new Color(0.75f, 0.86f, 0.9f);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 40f, 290f, 18f), "Gesture TCP  " + (mediaPipe != null && mediaPipe.IsAvailable ? "ONLINE" : "KEYBOARD FALLBACK"), textStyle);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 64f, 290f, 18f), "Cycle " + state.cycle + "   Corruption " + state.corruption, textStyle);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 88f, 290f, 18f), "Ghost trust " + state.ghost.trust + "   Shell " + state.ghost.shell, textStyle);
        GUI.Label(new Rect(panel.x + 14f, panel.y + 112f, 290f, 18f), "Last choice  " + (string.IsNullOrEmpty(state.lastChoice) ? "none" : state.lastChoice), textStyle);
        GUI.color = new Color(1f, 0.68f, 0.35f); GUI.Label(new Rect(panel.x + 14f, panel.y + 144f, 290f, 18f), "F2 善意预设   F3 敌意预设", textStyle);
        GUI.matrix = oldMatrix;
        GUI.color = Color.white;
    }

    private void EnsureStyles()
    {
        if (textStyle != null) return;
        Font chinese = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC" }, 14);
        Font font = chinese != null ? chinese : GUI.skin.font;
        textStyle = new GUIStyle(GUI.skin.label) { font = font, fontSize = 12, alignment = TextAnchor.MiddleLeft };
        headerStyle = new GUIStyle(textStyle) { fontSize = 15, fontStyle = FontStyle.Bold };
    }
}
