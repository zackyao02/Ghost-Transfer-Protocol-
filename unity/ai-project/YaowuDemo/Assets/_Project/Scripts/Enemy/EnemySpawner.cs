using UnityEngine;

public sealed class EnemySpawner : MonoBehaviour
{
    private Transform player;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    public EnemyHealth SpawnStoneSpirit(Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "StoneSpirit";
        enemy.transform.position = position;
        enemy.transform.localScale = new Vector3(1.1f, 1.4f, 1.1f);

        Renderer renderer = enemy.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(0.42f, 0.46f, 0.45f);

        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        health.Initialize(30, false);

        SimpleEnemyAI ai = enemy.AddComponent<SimpleEnemyAI>();
        ai.Initialize(player, 1.6f, 2f);

        return health;
    }

    public EnemyHealth SpawnBoss(Vector3 position)
    {
        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        boss.name = "MountainYaoBoss";
        boss.transform.position = position;

        Renderer renderer = boss.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = new Color(0.35f, 0.1f, 0.08f);

        EnemyHealth health = boss.AddComponent<EnemyHealth>();
        health.Initialize(200, true);

        SimpleEnemyAI ai = boss.AddComponent<SimpleEnemyAI>();
        ai.Initialize(player, 1.1f, 4f);
        boss.AddComponent<BossController>();

        SimpleEventBus.RaiseBossSpawned(health);
        SimpleEventBus.RaiseBossHealthChanged(health.CurrentHealth, health.MaxHealth);
        return health;
    }
}
