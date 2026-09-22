using System.Collections.Generic;
using UnityEngine;

public sealed class SkillManager : MonoBehaviour
{
    private readonly Dictionary<SkillType, float> cooldownReadyTimes = new Dictionary<SkillType, float>();

    private SkillCooldowns cooldowns;
    private Transform castOrigin;
    private Camera playerCamera;
    private CameraShake cameraShake;
    private bool deityUsed;

    public void Initialize(Transform origin, Camera cameraRef, CameraShake shake)
    {
        castOrigin = origin;
        playerCamera = cameraRef;
        cameraShake = shake;
        cooldowns = new SkillCooldowns();
        deityUsed = false;
        cooldownReadyTimes.Clear();
    }

    public float GetRemainingCooldown(SkillType skill)
    {
        float readyTime;
        if (!cooldownReadyTimes.TryGetValue(skill, out readyTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, readyTime - Time.time);
    }

    public void ResetRuntimeState()
    {
        deityUsed = false;
        cooldownReadyTimes.Clear();
    }

    public void CastSkill(SkillType skill)
    {
        if (!CanCast(skill))
        {
            return;
        }

        cooldownReadyTimes[skill] = Time.time + cooldowns.GetCooldown(skill);
        SimpleEventBus.RaiseSkillCast(skill);

        switch (skill)
        {
            case SkillType.SwordQi:
                CastSwordQi();
                break;
            case SkillType.FireTalisman:
                CastFireTalisman();
                break;
            case SkillType.DeitySummon:
                CastDeitySummon();
                break;
        }
    }

    private bool CanCast(SkillType skill)
    {
        if (skill == SkillType.DeitySummon && deityUsed)
        {
            SimpleEventBus.RaiseRecognitionStatusChanged("请神大招本局仅可释放一次");
            return false;
        }

        float remainingCooldown = GetRemainingCooldown(skill);
        if (remainingCooldown > 0f)
        {
            SimpleEventBus.RaiseRecognitionStatusChanged("技能冷却中: " + remainingCooldown.ToString("0.0") + "s");
            return false;
        }

        return true;
    }

    private void CastSwordQi()
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "SwordQiProjectile";
        projectile.transform.position = castOrigin.position + castOrigin.forward * 1.2f;
        projectile.transform.localScale = new Vector3(0.28f, 0.12f, 1.1f);
        projectile.GetComponent<Collider>().isTrigger = true;

        Renderer renderer = projectile.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.color = new Color(0.6f, 0.9f, 1f);
        renderer.material.SetColor("_EmissionColor", new Color(0.3f, 0.8f, 1f) * 1.6f);

        TrailRenderer trail = projectile.AddComponent<TrailRenderer>();
        trail.time = 0.15f;
        trail.startWidth = 0.2f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = new Color(0.85f, 1f, 1f, 0.9f);
        trail.endColor = new Color(0.3f, 0.8f, 1f, 0.1f);

        ProjectileSkill skill = projectile.AddComponent<ProjectileSkill>();
        skill.Initialize(castOrigin.forward, 24f, 1.1f, 35, cameraShake);
    }

    private void CastFireTalisman()
    {
        Vector3 targetPoint = castOrigin.position + castOrigin.forward * 10f;
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 30f))
        {
            targetPoint = hit.point;
        }

        targetPoint.y = 0.15f;

        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        effect.name = "FireTalismanArea";
        effect.transform.position = targetPoint;
        effect.GetComponent<Collider>().enabled = false;

        Renderer renderer = effect.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.color = new Color(1f, 0.35f, 0.1f, 0.65f);
        renderer.material.SetColor("_EmissionColor", new Color(1f, 0.25f, 0.05f) * 1.6f);

        AreaSkill skill = effect.AddComponent<AreaSkill>();
        skill.Initialize(0.45f, 3.6f, 60, cameraShake);
    }

    private void CastDeitySummon()
    {
        deityUsed = true;
        SimpleEventBus.RaiseDeitySummonStarted();

        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        effect.name = "DeitySummonField";
        effect.transform.position = new Vector3(castOrigin.position.x, 0.06f, castOrigin.position.z + 7f);
        effect.GetComponent<Collider>().enabled = false;

        Renderer renderer = effect.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.color = new Color(1f, 0.85f, 0.25f, 0.75f);
        renderer.material.SetColor("_EmissionColor", new Color(1f, 0.7f, 0.2f) * 2f);

        DeitySummonSkill skill = effect.AddComponent<DeitySummonSkill>();
        skill.Initialize(1.1f, cameraShake);

        for (int index = EnemyHealth.ActiveEnemies.Count - 1; index >= 0; index--)
        {
            EnemyHealth enemy = EnemyHealth.ActiveEnemies[index];
            if (enemy != null)
            {
                enemy.ApplyDamage(999, SkillType.DeitySummon);
            }
        }
    }
}
