using System;
using UnityEngine;

public static class SimpleEventBus
{
    public static event Action<SkillType> SkillCast;
    public static event Action<SkillType, GameObject> SkillHit;
    public static event Action<EnemyHealth> EnemyKilled;
    public static event Action<EnemyHealth> BossSpawned;
    public static event Action DeitySummonStarted;
    public static event Action DemoCompleted;
    public static event Action<string> InputModeChanged;
    public static event Action<string> RecognitionStatusChanged;
    public static event Action<float, float> BossHealthChanged;

    public static void RaiseSkillCast(SkillType skill)
    {
        SkillCast?.Invoke(skill);
    }

    public static void RaiseSkillHit(SkillType skill, GameObject target)
    {
        SkillHit?.Invoke(skill, target);
    }

    public static void RaiseEnemyKilled(EnemyHealth enemy)
    {
        EnemyKilled?.Invoke(enemy);
    }

    public static void RaiseBossSpawned(EnemyHealth boss)
    {
        BossSpawned?.Invoke(boss);
    }

    public static void RaiseDeitySummonStarted()
    {
        DeitySummonStarted?.Invoke();
    }

    public static void RaiseDemoCompleted()
    {
        DemoCompleted?.Invoke();
    }

    public static void RaiseInputModeChanged(string mode)
    {
        InputModeChanged?.Invoke(mode);
    }

    public static void RaiseRecognitionStatusChanged(string status)
    {
        RecognitionStatusChanged?.Invoke(status);
    }

    public static void RaiseBossHealthChanged(float current, float max)
    {
        BossHealthChanged?.Invoke(current, max);
    }
}
