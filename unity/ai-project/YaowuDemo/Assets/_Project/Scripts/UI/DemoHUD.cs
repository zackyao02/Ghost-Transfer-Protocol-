using UnityEngine;

public sealed class DemoHUD : MonoBehaviour
{
    private static readonly string[] Stages = { "校准", "破煞", "祭坛", "山门", "镇妖", "请神", "抉择", "余响" };
    private static readonly string[] ChoiceIds = { "seal", "symbiosis", "transfer" };
    private static readonly string[] ChoiceNames = { "封印", "共生", "转移" };
    private static readonly string[] ChoiceNotes = { "封存妖雾，守住旧约", "与灵体共担因果", "以己身承接残响" };

    private SkillManager skillManager;
    private string inputMode = "键盘 / 鼠标";
    private string recognition = "准备中";
    private string prompt = "横划释放剑气";
    private string selectedChoice = "seal";
    private bool finalChoiceActive;
    private bool showBoss;
    private bool completed;
    private float bossCurrent;
    private float bossMax = 1f;
    private GUIStyle title;
    private GUIStyle body;
    private GUIStyle small;
    private GUIStyle center;

    public void Initialize(SkillManager manager) { skillManager = manager; }

    private void OnEnable()
    {
        SimpleEventBus.InputModeChanged += HandleInputMode;
        SimpleEventBus.RecognitionStatusChanged += HandleRecognition;
        SimpleEventBus.BossSpawned += HandleBossSpawned;
        SimpleEventBus.BossHealthChanged += HandleBossHealth;
        SimpleEventBus.DemoCompleted += HandleCompleted;
        SimpleEventBus.FinalChoiceSelectionChanged += HandleChoice;
    }

    private void OnDisable()
    {
        SimpleEventBus.InputModeChanged -= HandleInputMode;
        SimpleEventBus.RecognitionStatusChanged -= HandleRecognition;
        SimpleEventBus.BossSpawned -= HandleBossSpawned;
        SimpleEventBus.BossHealthChanged -= HandleBossHealth;
        SimpleEventBus.DemoCompleted -= HandleCompleted;
        SimpleEventBus.FinalChoiceSelectionChanged -= HandleChoice;
    }

    public void SetPrompt(string text) { prompt = text; }
    public void ClearResult() { completed = false; showBoss = false; finalChoiceActive = false; }
    private void HandleInputMode(string mode) { inputMode = mode; }
    private void HandleRecognition(string status) { recognition = status; }
    private void HandleBossSpawned(EnemyHealth boss)
    {
        showBoss = boss != null;
        if (boss != null) { bossCurrent = boss.CurrentHealth; bossMax = boss.MaxHealth; }
    }
    private void HandleBossHealth(float current, float max) { bossCurrent = Mathf.Max(0f, current); bossMax = Mathf.Max(1f, max); }
    private void HandleCompleted() { completed = true; showBoss = false; finalChoiceActive = false; }
    private void HandleChoice(string choice, string hint) { selectedChoice = choice; finalChoiceActive = true; }

    private void OnGUI()
    {
        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.7f, 1.2f);
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        float width = Screen.width / scale;
        float height = Screen.height / scale;

        DrawHeader(new Rect(28f, 24f, 390f, 62f));
        DrawStages(new Rect(28f, 98f, Mathf.Min(760f, width - 56f), 50f));
        DrawStatus(new Rect(28f, height - 164f, Mathf.Min(520f, width - 56f), 136f));
        DrawSkills(new Rect(width - 320f, height - 158f, 292f, 130f));
        if (showBoss) DrawBoss(new Rect(width * 0.5f - 220f, 24f, 440f, 40f));
        if (finalChoiceActive && !completed) DrawChoices(width, height);
        if (completed) DrawResult(width, height);

        GUI.matrix = oldMatrix;
        GUI.color = Color.white;
    }

    private void DrawHeader(Rect rect)
    {
        Panel(rect, new Color(0.02f, 0.07f, 0.1f, 0.9f), new Color(0.18f, 0.85f, 0.95f));
        GUI.color = new Color(0.4f, 0.96f, 1f); GUI.Label(new Rect(rect.x + 14f, rect.y + 8f, 350f, 26f), "请 神 协 议", title);
        GUI.color = new Color(0.63f, 0.78f, 0.84f); GUI.Label(new Rect(rect.x + 15f, rect.y + 35f, 350f, 18f), "RITUAL PROTOCOL // CYBER EAST", small);
    }

    private void DrawStages(Rect rect)
    {
        Panel(rect, new Color(0.02f, 0.055f, 0.085f, 0.84f), new Color(0.1f, 0.38f, 0.5f));
        int active = GetStage();
        float unit = (rect.width - 20f) / Stages.Length;
        for (int index = 0; index < Stages.Length; index++)
        {
            Color color = index < active ? new Color(0.24f, 0.92f, 0.84f) : index == active ? new Color(1f, 0.76f, 0.24f) : new Color(0.2f, 0.32f, 0.38f);
            GUI.color = color; GUI.DrawTexture(new Rect(rect.x + 10f + index * unit, rect.y + 11f, unit - 5f, 4f), Texture2D.whiteTexture);
            GUI.Label(new Rect(rect.x + 10f + index * unit, rect.y + 22f, unit - 5f, 19f), (index + 1).ToString("00") + " " + Stages[index], center);
        }
    }

    private void DrawStatus(Rect rect)
    {
        Panel(rect, new Color(0.02f, 0.055f, 0.085f, 0.9f), new Color(0.1f, 0.42f, 0.54f));
        GUI.color = new Color(0.35f, 0.94f, 0.98f); GUI.Label(new Rect(rect.x + 14f, rect.y + 10f, 160f, 18f), "仪 式 链 路", small);
        GUI.color = Color.white; GUI.Label(new Rect(rect.x + 14f, rect.y + 34f, rect.width - 28f, 25f), "目标  " + prompt, body);
        GUI.color = new Color(0.66f, 0.82f, 0.88f); GUI.Label(new Rect(rect.x + 14f, rect.y + 64f, rect.width - 28f, 18f), "输入  " + inputMode, small);
        GUI.color = recognition.Contains("已识别") || recognition.Contains("ONLINE") ? new Color(0.4f, 1f, 0.75f) : new Color(1f, 0.76f, 0.32f);
        GUI.Label(new Rect(rect.x + 14f, rect.y + 87f, rect.width - 28f, 18f), "识别  " + recognition, small);
        GUI.color = new Color(0.48f, 0.65f, 0.72f); GUI.Label(new Rect(rect.x + 14f, rect.y + 112f, rect.width - 28f, 18f), "1 剑气   2 火符   Q 请神   Tab 切换   Enter 确认", small);
    }

    private void DrawSkills(Rect rect)
    {
        Panel(rect, new Color(0.02f, 0.055f, 0.085f, 0.9f), new Color(0.1f, 0.42f, 0.54f));
        GUI.color = new Color(0.35f, 0.94f, 0.98f); GUI.Label(new Rect(rect.x + 14f, rect.y + 9f, 180f, 18f), "术 式 冷 却", small);
        DrawSkill(rect.x + 14f, rect.y + 35f, "01", "剑气", SkillType.SwordQi, new Color(0.35f, 0.9f, 1f));
        DrawSkill(rect.x + 14f, rect.y + 64f, "02", "火符", SkillType.FireTalisman, new Color(1f, 0.45f, 0.18f));
        DrawSkill(rect.x + 14f, rect.y + 93f, "Q", "请神", SkillType.DeitySummon, new Color(1f, 0.78f, 0.24f));
    }

    private void DrawSkill(float x, float y, string key, string name, SkillType type, Color accent)
    {
        float cooldown = skillManager == null ? 0f : skillManager.GetRemainingCooldown(type);
        GUI.color = accent; GUI.Label(new Rect(x, y, 34f, 18f), key, small);
        GUI.color = Color.white; GUI.Label(new Rect(x + 36f, y, 48f, 18f), name, small);
        GUI.color = new Color(0.08f, 0.16f, 0.21f); GUI.DrawTexture(new Rect(x + 88f, y + 5f, 124f, 7f), Texture2D.whiteTexture);
        GUI.color = cooldown > 0f ? new Color(accent.r, accent.g, accent.b, 0.55f) : accent;
        GUI.DrawTexture(new Rect(x + 88f, y + 5f, 124f * (cooldown > 0f ? Mathf.Clamp01(1f - cooldown / 6f) : 1f), 7f), Texture2D.whiteTexture);
        GUI.color = cooldown > 0f ? new Color(1f, 0.76f, 0.35f) : new Color(0.4f, 1f, 0.75f);
        GUI.Label(new Rect(x + 220f, y, 52f, 18f), cooldown > 0f ? cooldown.ToString("0.0") + "s" : "READY", small);
    }

    private void DrawBoss(Rect rect)
    {
        Panel(rect, new Color(0.08f, 0.02f, 0.05f, 0.92f), new Color(0.82f, 0.15f, 0.3f));
        GUI.color = new Color(1f, 0.7f, 0.74f); GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, 106f, 19f), "山魈  " + bossCurrent.ToString("0"), small);
        GUI.color = new Color(0.33f, 0.05f, 0.1f); GUI.DrawTexture(new Rect(rect.x + 116f, rect.y + 15f, 310f, 10f), Texture2D.whiteTexture);
        GUI.color = new Color(0.95f, 0.16f, 0.3f); GUI.DrawTexture(new Rect(rect.x + 116f, rect.y + 15f, 310f * Mathf.Clamp01(bossCurrent / bossMax), 10f), Texture2D.whiteTexture);
    }

    private void DrawChoices(float width, float height)
    {
        float cardWidth = Mathf.Min(260f, (width - 92f) / 3f);
        float all = cardWidth * 3f + 32f;
        float left = width * 0.5f - all * 0.5f;
        float top = height * 0.5f - 74f;
        GUI.color = new Color(0.025f, 0.07f, 0.09f, 0.93f); GUI.DrawTexture(new Rect(left - 18f, top - 45f, all + 36f, 205f), Texture2D.whiteTexture);
        GUI.color = new Color(1f, 0.78f, 0.24f); GUI.Label(new Rect(left, top - 38f, all, 20f), "最终抉择 // Point 或 Tab 切换，Confirm 或 Enter 铭刻", center);
        for (int index = 0; index < ChoiceNames.Length; index++)
        {
            bool selected = selectedChoice == ChoiceIds[index];
            Rect card = new Rect(left + index * (cardWidth + 16f), top, cardWidth, 142f);
            Color accent = selected ? new Color(1f, 0.76f, 0.24f) : new Color(0.12f, 0.46f, 0.58f);
            Panel(card, selected ? new Color(0.13f, 0.11f, 0.07f, 0.96f) : new Color(0.025f, 0.075f, 0.1f, 0.9f), accent);
            GUI.color = accent; GUI.Label(new Rect(card.x + 13f, card.y + 13f, card.width - 26f, 26f), (index + 1).ToString("00") + "  " + ChoiceNames[index], title);
            GUI.color = new Color(0.78f, 0.88f, 0.91f); GUI.Label(new Rect(card.x + 13f, card.y + 52f, card.width - 26f, 38f), ChoiceNotes[index], body);
            GUI.color = accent; GUI.Label(new Rect(card.x + 13f, card.y + 110f, card.width - 26f, 18f), selected ? "[ 当前已选 ]" : "可选路径", small);
        }
    }

    private void DrawResult(float width, float height)
    {
        PersistentWorldState state = FindObjectOfType<WorldStateStore>() != null ? FindObjectOfType<WorldStateStore>().Current : null;
        Rect panel = new Rect(width * 0.5f - 260f, height * 0.5f - 110f, 520f, 220f);
        Panel(panel, new Color(0.025f, 0.065f, 0.085f, 0.96f), new Color(1f, 0.74f, 0.24f));
        GUI.color = new Color(1f, 0.78f, 0.28f); GUI.Label(new Rect(panel.x + 20f, panel.y + 22f, panel.width - 40f, 32f), EndingTitle(), center);
        GUI.color = new Color(0.72f, 0.9f, 0.94f); GUI.Label(new Rect(panel.x + 20f, panel.y + 62f, panel.width - 40f, 20f), "结局已铭刻：" + ChoiceName() + "  //  按 R 重启仪式", center);
        GUI.color = Color.white;
        string summary = state == null ? "世界状态正在同步" : "轮回 " + state.cycle + "    污染 " + state.corruption + "    灵体信任 " + state.ghost.trust + "\n义体 " + state.ghost.shell + "    伤痕 " + state.scars.Count + "    记忆 " + state.ghost.memories.Count;
        GUI.Label(new Rect(panel.x + 42f, panel.y + 105f, panel.width - 84f, 52f), summary, body);
        GUI.color = new Color(0.38f, 0.73f, 0.78f); GUI.Label(new Rect(panel.x + 20f, panel.y + 183f, panel.width - 40f, 18f), "WORLD STATE / PERSISTENT ECHO", center);
    }

    private void Panel(Rect rect, Color fill, Color border)
    {
        GUI.color = fill; GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = border;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 1f, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), Texture2D.whiteTexture);
    }

    private int GetStage()
    {
        if (completed) return 7;
        if (finalChoiceActive) return 6;
        if (prompt.Contains("请神仪式")) return 5;
        if (prompt.Contains("山魈") || prompt.Contains("普通技能")) return 4;
        if (prompt.Contains("山门")) return 3;
        if (prompt.Contains("祭坛")) return 2;
        return prompt.Contains("教程") || prompt.Contains("石精") ? 1 : 0;
    }

    private string ChoiceName() { return selectedChoice == "symbiosis" ? "共生" : selectedChoice == "transfer" ? "转移" : "封印"; }
    private string EndingTitle() { return selectedChoice == "symbiosis" ? "《共生回响》" : selectedChoice == "transfer" ? "《代价转移》" : "《封印余烬》"; }

    private void EnsureStyles()
    {
        if (title != null) return;
        Font chinese = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC" }, 16);
        Font font = chinese != null ? chinese : GUI.skin.font;
        title = new GUIStyle(GUI.skin.label) { font = font, fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        body = new GUIStyle(GUI.skin.label) { font = font, fontSize = 15, wordWrap = true, alignment = TextAnchor.MiddleLeft };
        small = new GUIStyle(GUI.skin.label) { font = font, fontSize = 12, alignment = TextAnchor.MiddleLeft };
        center = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
    }
}
