using UnityEngine;

public sealed class DemoFlowController : MonoBehaviour
{
    private enum DemoStage { Calibration, TutorialCombat, MemoryAltar, BossIntro, BossCombat, DeitySummon, FinalChoice, Result }
    private static readonly string[] FinalChoices = { "seal", "symbiosis", "transfer" };
    private static DemoFlowController instance;

    // Optional hand-off anchors for scene/prefab teams; primitives remain the runtime fallback.
    [SerializeField] private Transform memoryAltarAnchor;
    [SerializeField] private Transform finalChoiceAnchor;
    [SerializeField] private GameObject finalChoiceMarkerPrefab;

    private DemoStage stage;
    private EnemySpawner enemySpawner;
    private DemoHUD hud;
    private SkillManager skillManager;
    private Transform playerTransform;
    private WorldStateStore worldState;
    private int enemiesRemaining;
    private EnemyHealth currentBoss;
    private int selectedChoiceIndex;
    private readonly GameObject[] choiceMarkers = new GameObject[3];

    public static void RequestReset() { if (instance != null) instance.ResetDemo(); }

    public void Initialize(EnemySpawner spawner, DemoHUD demoHud, SkillManager manager, Transform player)
    {
        instance = this;
        enemySpawner = spawner;
        hud = demoHud;
        skillManager = manager;
        playerTransform = player;
    }

    public void SetWorldState(WorldStateStore state) { worldState = state; }

    private void OnEnable()
    {
        SimpleEventBus.EnemyKilled += HandleEnemyKilled;
        SimpleEventBus.SkillCast += HandleSkillCast;
        SimpleEventBus.DeitySummonStarted += HandleDeitySummonStarted;
        SimpleEventBus.ProtocolInputReceived += HandleProtocolInput;
    }

    private void OnDisable()
    {
        SimpleEventBus.EnemyKilled -= HandleEnemyKilled;
        SimpleEventBus.SkillCast -= HandleSkillCast;
        SimpleEventBus.DeitySummonStarted -= HandleDeitySummonStarted;
        SimpleEventBus.ProtocolInputReceived -= HandleProtocolInput;
    }

    public void StartDemo() { ResetDemo(); }

    private void HandleSkillCast(SkillType skill)
    {
        if (stage == DemoStage.Calibration) StartTutorialCombat();
        else if (stage == DemoStage.MemoryAltar) EnterBossIntro();
        else if (stage == DemoStage.BossIntro) StartBossCombat();
    }

    private void HandleDeitySummonStarted()
    {
        if (stage == DemoStage.BossCombat)
        {
            stage = DemoStage.DeitySummon;
            hud.SetPrompt("请神仪式已响应，净化山魈后进入最终抉择");
            SimpleEventBus.RaiseRecognitionStatusChanged("DeitySummon: 等待妖祟消散");
        }
        else if (stage == DemoStage.DeitySummon && currentBoss == null) EnterFinalChoice();
    }

    private void HandleProtocolInput(ProtocolInputAction action)
    {
        if (stage == DemoStage.Calibration && action == ProtocolInputAction.Confirm) { StartTutorialCombat(); return; }
        if (stage == DemoStage.MemoryAltar && action == ProtocolInputAction.Confirm) { EnterBossIntro(); return; }
        if (stage == DemoStage.BossIntro && action == ProtocolInputAction.Confirm) { StartBossCombat(); return; }
        if (stage != DemoStage.FinalChoice) return;

        if (action == ProtocolInputAction.Point)
        {
            selectedChoiceIndex = (selectedChoiceIndex + 1) % FinalChoices.Length;
            UpdateFinalChoiceFeedback();
        }
        else if (action == ProtocolInputAction.Confirm) CompleteFinalChoice();
    }

    private void HandleEnemyKilled(EnemyHealth enemy)
    {
        if (enemy == null) return;
        if (enemy.IsBoss)
        {
            currentBoss = null;
            if (stage == DemoStage.BossCombat)
            {
                stage = DemoStage.DeitySummon;
                hud.SetPrompt("山魈已伏，按 Q / 张掌完成请神仪式");
                SimpleEventBus.RaiseRecognitionStatusChanged("DeitySummon: 可用 Q 或张掌继续");
            }
            else if (stage == DemoStage.DeitySummon) EnterFinalChoice();
            return;
        }

        enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        if (stage == DemoStage.TutorialCombat && enemiesRemaining == 0) EnterMemoryAltar();
    }

    private void ResetDemo()
    {
        for (int index = EnemyHealth.ActiveEnemies.Count - 1; index >= 0; index--)
        {
            EnemyHealth enemy = EnemyHealth.ActiveEnemies[index];
            if (enemy != null) Destroy(enemy.gameObject);
        }

        ClearChoiceMarkers();
        stage = DemoStage.Calibration;
        currentBoss = null;
        enemiesRemaining = 0;
        selectedChoiceIndex = 0;
        if (skillManager != null)
        {
            skillManager.ResetRuntimeState();
            skillManager.SetDeitySummonEnabled(false);
        }
        if (hud != null) { hud.ClearResult(); hud.SetPrompt("校准: Point 指向，Confirm 确认；也可直接按 1 开始战斗"); }
        SimpleEventBus.RaiseInputModeChanged("键盘 / 鼠标");
        SimpleEventBus.RaiseRecognitionStatusChanged("Calibration: 等待 Confirm 或任一技能输入");
    }

    private void StartTutorialCombat()
    {
        stage = DemoStage.TutorialCombat;
        enemiesRemaining = 1;
        hud.SetPrompt("教程战斗: 横划释放剑气，击破第一只石精");
        enemySpawner.SpawnStoneSpirit(playerTransform.position + playerTransform.forward * 14f + playerTransform.right * 1.5f);
        SimpleEventBus.RaiseRecognitionStatusChanged("TutorialCombat: 第一只石精出现");
    }

    private void EnterMemoryAltar()
    {
        stage = DemoStage.MemoryAltar;
        Vector3 anchor = memoryAltarAnchor != null ? memoryAltarAnchor.position : playerTransform.position + playerTransform.forward * 8f;
        hud.SetPrompt("记忆祭坛: Point 观察记忆，Confirm 前往山门");
        SimpleEventBus.RaiseRecognitionStatusChanged("MemoryAltar: 祭坛坐标 " + anchor.ToString("F1"));
    }

    private void EnterBossIntro()
    {
        stage = DemoStage.BossIntro;
        hud.SetPrompt("山门异动: Confirm 迎战山魈，键盘技能也可继续");
        SimpleEventBus.RaiseRecognitionStatusChanged("BossIntro: 等待迎战确认");
    }

    private void StartBossCombat()
    {
        stage = DemoStage.BossCombat;
        skillManager.SetDeitySummonEnabled(true);
        hud.SetPrompt("山魈现身，普通技能压血后用 Q 或右键请神");
        currentBoss = enemySpawner.SpawnBoss(playerTransform.position + playerTransform.forward * 22f);
        SimpleEventBus.RaiseRecognitionStatusChanged("BossCombat: 山魈出现");
    }

    private void EnterFinalChoice()
    {
        stage = DemoStage.FinalChoice;
        selectedChoiceIndex = 0;
        BuildChoiceMarkers();
        UpdateFinalChoiceFeedback();
    }

    private void CompleteFinalChoice()
    {
        string choice = FinalChoices[selectedChoiceIndex];
        if (worldState != null) worldState.ApplyFinalChoice(choice);
        if (skillManager != null) skillManager.SetDeitySummonEnabled(false);
        ClearChoiceMarkers();
        stage = DemoStage.Result;
        hud.SetPrompt("结局: " + GetChoiceLabel(choice) + "。按 R 重新开始");
        SimpleEventBus.RaiseRecognitionStatusChanged("Result: 已写入 WorldStateStore / " + choice);
        SimpleEventBus.RaiseDemoCompleted();
    }

    private void BuildChoiceMarkers()
    {
        Vector3 center = finalChoiceAnchor != null ? finalChoiceAnchor.position : playerTransform.position + playerTransform.forward * 6f;
        for (int index = 0; index < choiceMarkers.Length; index++)
        {
            GameObject marker = finalChoiceMarkerPrefab != null ? Instantiate(finalChoiceMarkerPrefab) : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "FinalChoice_" + FinalChoices[index];
            marker.transform.position = center + new Vector3((index - 1) * 2.5f, 0.1f, 0f);
            marker.transform.localScale = new Vector3(0.8f, 0.08f, 0.8f);
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null) markerCollider.enabled = false;
            choiceMarkers[index] = marker;
        }
    }

    private void UpdateFinalChoiceFeedback()
    {
        string choice = FinalChoices[selectedChoiceIndex];
        for (int index = 0; index < choiceMarkers.Length; index++)
        {
            GameObject marker = choiceMarkers[index];
            Renderer markerRenderer = marker != null ? marker.GetComponentInChildren<Renderer>() : null;
            if (markerRenderer != null) markerRenderer.material.color = index == selectedChoiceIndex ? new Color(1f, 0.8f, 0.2f) : new Color(0.25f, 0.35f, 0.4f);
        }

        string hint = "Point 切换: " + GetChoiceLabel(choice) + " | Confirm 确认（Tab / Enter 可兜底）";
        hud.SetPrompt("最终抉择: " + hint);
        SimpleEventBus.RaiseRecognitionStatusChanged("FinalChoice: " + GetChoiceLabel(choice));
        SimpleEventBus.RaiseFinalChoiceSelectionChanged(choice, hint);
    }

    private void ClearChoiceMarkers()
    {
        for (int index = 0; index < choiceMarkers.Length; index++)
        {
            if (choiceMarkers[index] != null) Destroy(choiceMarkers[index]);
            choiceMarkers[index] = null;
        }
    }

    private static string GetChoiceLabel(string choice)
    {
        switch (choice)
        {
            case "seal": return "封印";
            case "symbiosis": return "共生";
            case "transfer": return "转移";
            default: return choice;
        }
    }
}
