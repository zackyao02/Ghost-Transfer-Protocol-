using UnityEngine;

public sealed class DemoHUD : MonoBehaviour
{
    private SkillManager skillManager;
    private string inputMode = "键盘 / 鼠标";
    private string recognitionStatus = "准备中";
    private string prompt = "横划释放剑气";
    private string resultText = string.Empty;
    private float bossCurrent;
    private float bossMax;
    private bool showBossBar;
    private string finalChoiceHint = string.Empty;

    public void Initialize(SkillManager manager)
    {
        skillManager = manager;
    }

    private void OnEnable()
    {
        SimpleEventBus.InputModeChanged += HandleInputModeChanged;
        SimpleEventBus.RecognitionStatusChanged += HandleRecognitionStatusChanged;
        SimpleEventBus.BossSpawned += HandleBossSpawned;
        SimpleEventBus.BossHealthChanged += HandleBossHealthChanged;
        SimpleEventBus.DemoCompleted += HandleDemoCompleted;
        SimpleEventBus.FinalChoiceSelectionChanged += HandleFinalChoiceSelectionChanged;
    }

    private void OnDisable()
    {
        SimpleEventBus.InputModeChanged -= HandleInputModeChanged;
        SimpleEventBus.RecognitionStatusChanged -= HandleRecognitionStatusChanged;
        SimpleEventBus.BossSpawned -= HandleBossSpawned;
        SimpleEventBus.BossHealthChanged -= HandleBossHealthChanged;
        SimpleEventBus.DemoCompleted -= HandleDemoCompleted;
        SimpleEventBus.FinalChoiceSelectionChanged -= HandleFinalChoiceSelectionChanged;
    }

    public void SetPrompt(string text)
    {
        prompt = text;
    }

    public void ClearResult()
    {
        resultText = string.Empty;
        showBossBar = false;
        finalChoiceHint = string.Empty;
    }

    private void HandleInputModeChanged(string mode)
    {
        inputMode = mode;
    }

    private void HandleRecognitionStatusChanged(string status)
    {
        recognitionStatus = status;
    }

    private void HandleBossSpawned(EnemyHealth boss)
    {
        showBossBar = boss != null;
        if (boss != null)
        {
            bossCurrent = boss.CurrentHealth;
            bossMax = boss.MaxHealth;
        }
    }

    private void HandleBossHealthChanged(float current, float max)
    {
        bossCurrent = Mathf.Max(0f, current);
        bossMax = Mathf.Max(1f, max);
    }

    private void HandleDemoCompleted()
    {
        resultText = "请神成功，妖雾尽散";
        showBossBar = false;
    }

    private void HandleFinalChoiceSelectionChanged(string choice, string hint)
    {
        finalChoiceHint = hint;
    }

    private void OnGUI()
    {
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.Box(new Rect(16f, 16f, 450f, 148f), string.Empty);
        GUI.color = Color.white;
        GUI.Label(new Rect(28f, 28f, 420f, 24f), "请神 Demo | 输入模式: " + inputMode);
        GUI.Label(new Rect(28f, 52f, 420f, 24f), "识别状态: " + recognitionStatus);
        GUI.Label(new Rect(28f, 76f, 420f, 24f), "当前提示: " + prompt);
        GUI.Label(new Rect(28f, 100f, 420f, 24f), "操作: 1 剑气  2 火符  Q 请神  Tab 选择  Enter 确认  R 重置");

        if (skillManager != null)
        {
            GUI.Label(new Rect(28f, 124f, 420f, 24f),
                "冷却: 剑气 " + skillManager.GetRemainingCooldown(SkillType.SwordQi).ToString("0.0") +
                " | 火符 " + skillManager.GetRemainingCooldown(SkillType.FireTalisman).ToString("0.0") +
                " | 请神 " + skillManager.GetRemainingCooldown(SkillType.DeitySummon).ToString("0.0"));
        }

        if (showBossBar)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.Box(new Rect(Screen.width * 0.25f, 24f, Screen.width * 0.5f, 32f), string.Empty);
            GUI.color = new Color(0.85f, 0.15f, 0.1f, 1f);
            float ratio = bossCurrent / bossMax;
            GUI.Box(new Rect(Screen.width * 0.25f + 4f, 28f, (Screen.width * 0.5f - 8f) * ratio, 24f), string.Empty);
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width * 0.25f + 8f, 28f, 200f, 24f), "山魈血量 " + bossCurrent.ToString("0") + " / " + bossMax.ToString("0"));
        }

        if (!string.IsNullOrEmpty(resultText))
        {
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.Box(new Rect(Screen.width * 0.5f - 180f, Screen.height * 0.5f - 40f, 360f, 80f), string.Empty);
            GUI.color = new Color(1f, 0.9f, 0.3f, 1f);
            GUI.Label(new Rect(Screen.width * 0.5f - 120f, Screen.height * 0.5f - 8f, 300f, 24f), resultText);
            GUI.color = Color.white;
        }

        if (!string.IsNullOrEmpty(finalChoiceHint) && string.IsNullOrEmpty(resultText))
        {
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.Box(new Rect(Screen.width * 0.5f - 220f, Screen.height - 90f, 440f, 48f), string.Empty);
            GUI.color = new Color(1f, 0.9f, 0.35f, 1f);
            GUI.Label(new Rect(Screen.width * 0.5f - 204f, Screen.height - 76f, 408f, 22f), finalChoiceHint);
            GUI.color = Color.white;
        }
    }
}
