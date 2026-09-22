using UnityEngine;

public sealed class DemoFlowController : MonoBehaviour
{
    private enum DemoStage
    {
        Intro,
        Wave,
        Boss,
        Victory
    }

    private static DemoFlowController instance;

    private DemoStage stage;
    private EnemySpawner enemySpawner;
    private DemoHUD hud;
    private SkillManager skillManager;
    private Transform playerTransform;
    private int enemiesRemaining;
    private EnemyHealth currentBoss;

    public static void RequestReset()
    {
        if (instance != null)
        {
            instance.ResetDemo();
        }
    }

    public void Initialize(EnemySpawner spawner, DemoHUD demoHud, SkillManager manager, Transform player)
    {
        instance = this;
        enemySpawner = spawner;
        hud = demoHud;
        skillManager = manager;
        playerTransform = player;
    }

    private void OnEnable()
    {
        SimpleEventBus.EnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        SimpleEventBus.EnemyKilled -= HandleEnemyKilled;
    }

    public void StartDemo()
    {
        ResetDemo();
    }

    private void HandleEnemyKilled(EnemyHealth enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (enemy.IsBoss)
        {
            CompleteDemo();
            return;
        }

        enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        if (stage == DemoStage.Intro && enemiesRemaining == 0)
        {
            SpawnWave();
        }
        else if (stage == DemoStage.Wave && enemiesRemaining == 0)
        {
            SpawnBoss();
        }
    }

    private void ResetDemo()
    {
        for (int index = EnemyHealth.ActiveEnemies.Count - 1; index >= 0; index--)
        {
            EnemyHealth enemy = EnemyHealth.ActiveEnemies[index];
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        stage = DemoStage.Intro;
        currentBoss = null;
        enemiesRemaining = 0;
        skillManager.ResetRuntimeState();
        hud.ClearResult();
        hud.SetPrompt("横划释放剑气，击破第一只石精");
        SimpleEventBus.RaiseInputModeChanged("键盘 / 鼠标");
        SimpleEventBus.RaiseRecognitionStatusChanged("准备中: 先用 1 或横划释放剑气");
        SpawnIntroEnemy();
    }

    private void SpawnIntroEnemy()
    {
        enemiesRemaining = 1;
        enemySpawner.SpawnStoneSpirit(playerTransform.position + playerTransform.forward * 14f + playerTransform.right * 1.5f);
    }

    private void SpawnWave()
    {
        stage = DemoStage.Wave;
        enemiesRemaining = 4;
        hud.SetPrompt("画圈释放火符，清掉一波石精");
        Vector3 center = playerTransform.position + playerTransform.forward * 16f;
        enemySpawner.SpawnStoneSpirit(center + new Vector3(-3.5f, 0f, 1f));
        enemySpawner.SpawnStoneSpirit(center + new Vector3(-1f, 0f, 2.5f));
        enemySpawner.SpawnStoneSpirit(center + new Vector3(2f, 0f, 1.5f));
        enemySpawner.SpawnStoneSpirit(center + new Vector3(4f, 0f, -1f));
        SimpleEventBus.RaiseRecognitionStatusChanged("第一只石精已灭，下一步画圈放火符");
    }

    private void SpawnBoss()
    {
        stage = DemoStage.Boss;
        hud.SetPrompt("山魈现身，普通技能压血后用 Q 或右键请神");
        Vector3 position = playerTransform.position + playerTransform.forward * 22f;
        currentBoss = enemySpawner.SpawnBoss(position);
        SimpleEventBus.RaiseRecognitionStatusChanged("山魈出现: 可先用剑气和火符，再用请神终结");
    }

    private void CompleteDemo()
    {
        if (stage == DemoStage.Victory)
        {
            return;
        }

        stage = DemoStage.Victory;
        hud.SetPrompt("演示完成，按 R 重新开始");
        SimpleEventBus.RaiseRecognitionStatusChanged("胜利: 请神清场完成");
        SimpleEventBus.RaiseDemoCompleted();
    }
}
