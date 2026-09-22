using System.Collections.Generic;
using UnityEngine;

// Runtime fallback presentation. Prefab teams can replace this single root without touching gameplay.
public sealed class CyberShrinePresentation : MonoBehaviour
{
    private const string RootName = "CyberShrinePresentation";
    private readonly List<GlowNode> glowNodes = new List<GlowNode>();
    private readonly List<PulseNode> pulses = new List<PulseNode>();
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    private Material charcoalMaterial;
    private Material vermilionMaterial;
    private Material jadeMaterial;
    private Material goldMaterial;
    private Vector3 playerSpawn;
    private Color environmentTint = Color.white;

    public static CyberShrinePresentation CreateOrReplace(Vector3 spawn)
    {
        GameObject previous = GameObject.Find(RootName);
        if (previous != null)
        {
            Destroy(previous);
        }

        GameObject root = new GameObject(RootName);
        CyberShrinePresentation presentation = root.AddComponent<CyberShrinePresentation>();
        presentation.Initialize(spawn);
        return presentation;
    }

    public void Initialize(Vector3 spawn)
    {
        playerSpawn = spawn;
        CreateMaterials();
        BuildEnvironment();
    }

    private void OnEnable()
    {
        SimpleEventBus.SkillCast += HandleSkillCast;
        SimpleEventBus.SkillHit += HandleSkillHit;
        SimpleEventBus.BossSpawned += HandleBossSpawned;
        SimpleEventBus.DeitySummonStarted += HandleDeitySummon;
        SimpleEventBus.DemoCompleted += HandleDemoCompleted;
        SimpleEventBus.RecognitionStatusChanged += HandleStatusChanged;
    }

    private void OnDisable()
    {
        SimpleEventBus.SkillCast -= HandleSkillCast;
        SimpleEventBus.SkillHit -= HandleSkillHit;
        SimpleEventBus.BossSpawned -= HandleBossSpawned;
        SimpleEventBus.DeitySummonStarted -= HandleDeitySummon;
        SimpleEventBus.DemoCompleted -= HandleDemoCompleted;
        SimpleEventBus.RecognitionStatusChanged -= HandleStatusChanged;
    }

    private void Update()
    {
        float time = Time.time;
        for (int index = 0; index < glowNodes.Count; index++)
        {
            GlowNode node = glowNodes[index];
            if (node.Renderer == null) continue;
            float pulse = 0.72f + Mathf.Sin(time * node.Speed + node.Offset) * 0.28f;
            ApplyEmission(node.Renderer, node.Color * environmentTint, pulse * node.Intensity);
        }

        for (int index = pulses.Count - 1; index >= 0; index--)
        {
            PulseNode pulse = pulses[index];
            float progress = (time - pulse.StartTime) / pulse.Duration;
            if (progress >= 1f || pulse.Transform == null)
            {
                if (pulse.Transform != null) Destroy(pulse.Transform.gameObject);
                pulses.RemoveAt(index);
                continue;
            }

            float scale = Mathf.Lerp(pulse.StartScale, pulse.EndScale, progress);
            pulse.Transform.localScale = new Vector3(scale, 0.025f, scale);
            ApplyEmission(pulse.Renderer, pulse.Color, 1f - progress);
        }
    }

    private void BuildEnvironment()
    {
        CreatePrimitive(PrimitiveType.Plane, "ObsidianCourtyard", Vector3.zero, new Vector3(5f, 1f, 5f), charcoalMaterial);
        CreateTorii(new Vector3(0f, 0f, -1f), 1f);
        CreateTorii(new Vector3(0f, 0f, 12f), 1.25f);
        CreateShrine(new Vector3(0f, 0f, 22f));
        CreateAltar(new Vector3(0f, 0.26f, 8f));
        CreateDataColumns();
        CreateSafeZone();

        Light altarLight = CreateLight("AltarJadeLight", new Vector3(0f, 3.4f, 8f), new Color(0.18f, 1f, 0.72f), 2.2f, 16f);
        altarLight.transform.SetParent(transform, true);
        Light shrineLight = CreateLight("ShrineGoldLight", new Vector3(0f, 4f, 20f), new Color(1f, 0.55f, 0.18f), 2.5f, 20f);
        shrineLight.transform.SetParent(transform, true);
    }

    private void CreateTorii(Vector3 center, float scale)
    {
        CreatePrimitive(PrimitiveType.Cube, "ToriiPillar_L", center + new Vector3(-3.2f * scale, 2.2f * scale, 0f), new Vector3(0.42f, 4.4f, 0.5f) * scale, vermilionMaterial);
        CreatePrimitive(PrimitiveType.Cube, "ToriiPillar_R", center + new Vector3(3.2f * scale, 2.2f * scale, 0f), new Vector3(0.42f, 4.4f, 0.5f) * scale, vermilionMaterial);
        CreatePrimitive(PrimitiveType.Cube, "ToriiLintel", center + new Vector3(0f, 4.25f * scale, 0f), new Vector3(7.8f, 0.46f, 0.72f) * scale, vermilionMaterial);
        CreatePrimitive(PrimitiveType.Cube, "ToriiCrest", center + new Vector3(0f, 4.72f * scale, 0f), new Vector3(8.8f, 0.2f, 0.9f) * scale, charcoalMaterial);
    }

    private void CreateShrine(Vector3 center)
    {
        CreatePrimitive(PrimitiveType.Cube, "ShrineBody", center + new Vector3(0f, 2.2f, 0f), new Vector3(8f, 4.4f, 1.4f), charcoalMaterial);
        CreatePrimitive(PrimitiveType.Cube, "ShrineRoof", center + new Vector3(0f, 4.65f, -0.1f), new Vector3(9.4f, 0.35f, 2.3f), vermilionMaterial);
        CreatePrimitive(PrimitiveType.Cube, "ShrineDoor", center + new Vector3(0f, 2.1f, -0.78f), new Vector3(2.4f, 3.4f, 0.1f), goldMaterial);
        CreateSealRing(center + new Vector3(0f, 2.4f, -0.95f), 1.35f, goldMaterial, 1.5f);
    }

    private void CreateAltar(Vector3 center)
    {
        CreatePrimitive(PrimitiveType.Cylinder, "MemoryAltar", center, new Vector3(3.2f, 0.22f, 3.2f), charcoalMaterial);
        CreatePrimitive(PrimitiveType.Cylinder, "MemoryAltarTrim", center + Vector3.up * 0.24f, new Vector3(2.75f, 0.03f, 2.75f), goldMaterial);
        CreateSealRing(center + Vector3.up * 0.31f, 2.1f, jadeMaterial, 1.9f);
    }

    private void CreateDataColumns()
    {
        for (int index = 0; index < 10; index++)
        {
            float side = index < 5 ? -1f : 1f;
            float row = index % 5;
            float height = 2.3f + (row % 3) * 0.75f;
            Vector3 position = new Vector3(side * (8.5f + row * 1.35f), height * 0.5f, 3f + row * 5f);
            Renderer renderer = CreatePrimitive(PrimitiveType.Cube, "NeonDataColumn", position, new Vector3(0.28f, height, 0.28f), row % 2 == 0 ? jadeMaterial : goldMaterial);
            AddGlow(renderer, row % 2 == 0 ? new Color(0.08f, 1f, 0.68f) : new Color(1f, 0.58f, 0.16f), 1.1f, 0.8f + row * 0.13f);
        }
    }

    private void CreateSafeZone()
    {
        Vector3 position = playerSpawn + new Vector3(0f, -1.17f, 1.8f);
        CreateSealRing(position, 1.15f, jadeMaterial, 1.4f);
    }

    private void CreateSealRing(Vector3 center, float radius, Material material, float intensity)
    {
        const int segments = 16;
        for (int index = 0; index < segments; index++)
        {
            float angle = index * Mathf.PI * 2f / segments;
            Vector3 position = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Renderer renderer = CreatePrimitive(PrimitiveType.Cube, "TalismanRing", position, new Vector3(0.09f, 0.06f, 0.42f), material);
            renderer.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
            AddGlow(renderer, material == jadeMaterial ? new Color(0.08f, 1f, 0.68f) : new Color(1f, 0.64f, 0.22f), intensity, 1.2f);
        }
    }

    private Renderer CreatePrimitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = objectName;
        item.transform.SetParent(transform, true);
        item.transform.position = position;
        item.transform.localScale = scale;
        Collider collider = item.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        Renderer renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    private void CreateMaterials()
    {
        charcoalMaterial = CreateMaterial(new Color(0.055f, 0.07f, 0.08f), Color.black);
        vermilionMaterial = CreateMaterial(new Color(0.58f, 0.06f, 0.04f), new Color(0.5f, 0.02f, 0.01f));
        jadeMaterial = CreateMaterial(new Color(0.03f, 0.42f, 0.3f), new Color(0.03f, 0.8f, 0.5f));
        goldMaterial = CreateMaterial(new Color(0.65f, 0.38f, 0.08f), new Color(0.9f, 0.42f, 0.06f));
    }

    private static Material CreateMaterial(Color color, Color emission)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission);
        return material;
    }

    private void AddGlow(Renderer renderer, Color color, float intensity, float speed)
    {
        glowNodes.Add(new GlowNode { Renderer = renderer, Color = color, Intensity = intensity, Speed = speed, Offset = glowNodes.Count * 0.73f });
    }

    private void HandleSkillCast(SkillType skill)
    {
        Color color = skill == SkillType.SwordQi ? new Color(0.2f, 0.9f, 1f) : new Color(1f, 0.2f, 0.08f);
        CreatePulse(playerSpawn + Vector3.forward * 5f, color, 0.5f, 3.5f, 0.6f);
    }

    private void HandleSkillHit(SkillType skill, GameObject target)
    {
        if (target != null) CreatePulse(target.transform.position + Vector3.up * 0.08f, new Color(1f, 0.7f, 0.16f), 0.25f, 2.2f, 0.35f);
    }

    private void HandleBossSpawned(EnemyHealth boss)
    {
        environmentTint = new Color(1f, 0.58f, 0.58f);
        if (boss != null) CreatePulse(boss.transform.position, new Color(1f, 0.08f, 0.04f), 0.8f, 8f, 1.1f);
    }

    private void HandleDeitySummon()
    {
        environmentTint = new Color(1f, 0.9f, 0.5f);
        CreatePulse(playerSpawn + Vector3.forward * 7f, new Color(1f, 0.78f, 0.18f), 0.8f, 10f, 1.35f);
    }

    private void HandleDemoCompleted()
    {
        environmentTint = new Color(0.6f, 1f, 0.78f);
        CreatePulse(new Vector3(0f, 0.08f, 8f), new Color(0.12f, 1f, 0.62f), 0.6f, 8f, 1.1f);
    }

    private void HandleStatusChanged(string status)
    {
        if (status == null) return;
        if (status.StartsWith("Calibration:"))
        {
            ClearTransientEffects();
            environmentTint = Color.white;
        }
        else if (status.StartsWith("MemoryAltar:")) environmentTint = new Color(0.7f, 1f, 0.9f);
        else if (status.StartsWith("BossIntro:")) environmentTint = new Color(1f, 0.72f, 0.55f);
        else if (status.StartsWith("FinalChoice:")) environmentTint = new Color(0.72f, 0.86f, 1f);
    }

    private void CreatePulse(Vector3 position, Color color, float startScale, float endScale, float duration)
    {
        Renderer renderer = CreatePrimitive(PrimitiveType.Cylinder, "RitualPulse", position, new Vector3(startScale, 0.025f, startScale), goldMaterial);
        pulses.Add(new PulseNode { Transform = renderer.transform, Renderer = renderer, Color = color, StartScale = startScale, EndScale = endScale, StartTime = Time.time, Duration = duration });
    }

    private void ClearTransientEffects()
    {
        for (int index = 0; index < pulses.Count; index++)
        {
            if (pulses[index].Transform != null) Destroy(pulses[index].Transform.gameObject);
        }
        pulses.Clear();
    }

    private void ApplyEmission(Renderer renderer, Color color, float intensity)
    {
        propertyBlock.Clear();
        propertyBlock.SetColor("_Color", color);
        propertyBlock.SetColor("_EmissionColor", color * intensity);
        renderer.SetPropertyBlock(propertyBlock);
    }

    private static Light CreateLight(string objectName, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(objectName);
        lightObject.transform.position = position;
        Light lightRef = lightObject.AddComponent<Light>();
        lightRef.type = LightType.Point;
        lightRef.color = color;
        lightRef.intensity = intensity;
        lightRef.range = range;
        return lightRef;
    }

    private struct GlowNode { public Renderer Renderer; public Color Color; public float Intensity; public float Speed; public float Offset; }
    private struct PulseNode { public Transform Transform; public Renderer Renderer; public Color Color; public float StartScale; public float EndScale; public float StartTime; public float Duration; }
}
