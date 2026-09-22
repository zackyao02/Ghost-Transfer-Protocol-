using UnityEngine;

public sealed class DirectorDebugPanel : MonoBehaviour
{
    private KeyboardSkillInput keyboard;
    private MediaPipeHandInput mediaPipe;
    private WorldStateStore world;
    private bool visible;

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

        PersistentWorldState state = world.Current;
        GUI.color = new Color(0f, 0f, 0f, 0.78f);
        GUI.Box(new Rect(Screen.width - 360f, 16f, 344f, 160f), string.Empty);
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 344f, 28f, 320f, 22f), "WORLD DIRECTOR DEBUG");
        GUI.Label(new Rect(Screen.width - 344f, 52f, 320f, 22f), "Gesture TCP: " + (mediaPipe != null && mediaPipe.IsAvailable ? "ONLINE" : "KEYBOARD FALLBACK"));
        GUI.Label(new Rect(Screen.width - 344f, 76f, 320f, 22f), "Cycle: " + state.cycle + " | Corruption: " + state.corruption);
        GUI.Label(new Rect(Screen.width - 344f, 100f, 320f, 22f), "Ghost trust: " + state.ghost.trust + " | Shell: " + state.ghost.shell);
        GUI.Label(new Rect(Screen.width - 344f, 124f, 320f, 22f), "Last choice: " + (string.IsNullOrEmpty(state.lastChoice) ? "none" : state.lastChoice));
        GUI.Label(new Rect(Screen.width - 344f, 148f, 320f, 22f), "F2 善意存档 | F3 敌意存档");
    }
}
