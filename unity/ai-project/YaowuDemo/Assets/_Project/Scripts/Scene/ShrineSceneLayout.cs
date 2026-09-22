using UnityEngine;

public static class ShrineSceneLayout
{
    public const string RootName = "Scene_A_CyberShrine";
    private static readonly Color Dark = new Color(0.07f, 0.075f, 0.085f);
    private static readonly Color Stone = new Color(0.17f, 0.18f, 0.19f);
    private static readonly Color Red = new Color(0.72f, 0.08f, 0.035f);
    private static readonly Color Cyan = new Color(0.05f, 0.72f, 0.82f);
    private static readonly Color Gold = new Color(0.95f, 0.62f, 0.12f);

    public static ShrineEnvironmentController Build(Vector3 playerSpawn)
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) return existing.GetComponent<ShrineEnvironmentController>();

        GameObject root = new GameObject(RootName);
        ShrineEnvironmentController controller = root.AddComponent<ShrineEnvironmentController>();
        Transform architecture = Group(root.transform, "Architecture");
        Transform skyline = Group(root.transform, "CitySkyline");
        Transform anchors = Group(root.transform, "Anchors");
        Transform atmosphere = Group(root.transform, "Atmosphere");
        Transform scars = Group(root.transform, "EndingScars");

        BuildArchitecture(architecture);
        BuildSkyline(skyline);
        BuildAnchors(anchors, playerSpawn);

        Light altarLight = PointLight(atmosphere, "Altar_CyanLight", new Vector3(0f, 3.2f, 8f), Cyan, 2.3f, 17f);
        Light bossLight = PointLight(atmosphere, "Boss_RedLight", new Vector3(0f, 4f, 20f), Red, 0f, 15f);
        BuildMoon(atmosphere);
        ParticleSystem rain = BuildRain(atmosphere);

        GameObject seal = BuildSealScar(scars);
        GameObject coexist = BuildCoexistScar(scars);
        GameObject transfer = BuildTransferScar(scars);
        controller.Initialize(altarLight, bossLight, rain, seal, coexist, transfer);
        return controller;
    }

    private static void BuildArchitecture(Transform parent)
    {
        Box(parent, "CourtyardFloor", new Vector3(0f, -0.3f, 8f), new Vector3(26f, 0.5f, 34f), Stone);
        Box(parent, "ProcessionPath", new Vector3(0f, 0.01f, 6f), new Vector3(4.5f, 0.08f, 28f), Dark, Cyan * 0.18f);
        Box(parent, "LeftBoundary", new Vector3(-13f, 1.2f, 8f), new Vector3(0.5f, 2.4f, 34f), Dark);
        Box(parent, "RightBoundary", new Vector3(13f, 1.2f, 8f), new Vector3(0.5f, 2.4f, 34f), Dark);

        for (int i = 0; i < 6; i++)
        {
            float z = -3f + i * 5f;
            Lantern(parent, "Lantern_L_" + (i + 1), new Vector3(-5.5f, 1.2f, z));
            Lantern(parent, "Lantern_R_" + (i + 1), new Vector3(5.5f, 1.2f, z));
        }

        Box(parent, "AltarBase", new Vector3(0f, 0.45f, 8f), new Vector3(8f, 0.9f, 6f), Dark);
        Box(parent, "AltarCore", new Vector3(0f, 1.25f, 8.8f), new Vector3(3.2f, 1.6f, 2.2f), Stone, Cyan * 0.45f);
        Box(parent, "MemorySlab", new Vector3(0f, 2.5f, 9.5f), new Vector3(1.8f, 3.5f, 0.35f), Dark, Cyan);
        Box(parent, "ShrineBack", new Vector3(0f, 2.8f, 14f), new Vector3(11f, 5.6f, 1.2f), Dark);
        Box(parent, "ShrineRoof", new Vector3(0f, 5.8f, 13.8f), new Vector3(14f, 0.55f, 3.4f), Red);
        Box(parent, "ShrineRoofCap", new Vector3(0f, 6.3f, 13.8f), new Vector3(9f, 0.35f, 2.8f), Dark);

        Box(parent, "BossGate_LeftPillar", new Vector3(-4.3f, 3.2f, 20f), new Vector3(1.1f, 6.4f, 1.1f), Red);
        Box(parent, "BossGate_RightPillar", new Vector3(4.3f, 3.2f, 20f), new Vector3(1.1f, 6.4f, 1.1f), Red);
        Box(parent, "BossGate_Beam", new Vector3(0f, 6.2f, 20f), new Vector3(11f, 0.8f, 1.2f), Red);
        Box(parent, "BossGate_TopBeam", new Vector3(0f, 7f, 20f), new Vector3(8.5f, 0.45f, 1f), Dark);
        Box(parent, "BossGate_Rift", new Vector3(0f, 3f, 20.55f), new Vector3(5.8f, 5.5f, 0.08f), Red, Red * 2.2f);
    }

    private static void BuildSkyline(Transform parent)
    {
        for (int i = 0; i < 12; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float height = 8f + (i % 5) * 2.5f;
            Box(parent, "Tower_" + (i + 1),
                new Vector3(side * (15f + (i % 4) * 3.2f), height * 0.5f - 0.2f, 22f + (i % 3) * 5f),
                new Vector3(2.4f, height, 2.4f), Dark, Cyan * 0.3f);
        }
    }

    private static void BuildAnchors(Transform parent, Vector3 playerSpawn)
    {
        CameraAnchor(parent, "CameraNode_Act1_Calibration", 1, playerSpawn + new Vector3(0f, 1.5f, 0f), new Vector3(0f, 1.8f, 8.5f));
        CameraAnchor(parent, "CameraNode_Act2_Courtyard", 2, new Vector3(0f, 4.2f, -5f), new Vector3(0f, 1.2f, 8f));
        CameraAnchor(parent, "CameraNode_Act3_MemoryAltar", 3, new Vector3(-7.5f, 3.4f, 5f), new Vector3(0f, 2f, 9f));
        CameraAnchor(parent, "CameraNode_Act4_Boss", 4, new Vector3(0f, 4.5f, -1f), new Vector3(0f, 3f, 20f));
        CameraAnchor(parent, "CameraNode_Act5_Deity", 5, new Vector3(0f, 5.5f, 4.5f), new Vector3(0f, 4.2f, 11f));
        Anchor(parent, "Spawn_Stone_Left", SceneAnchorType.StoneSpawn, 2, new Vector3(-4.2f, 0.8f, 10f));
        Anchor(parent, "Spawn_Stone_Right", SceneAnchorType.StoneSpawn, 2, new Vector3(4.2f, 0.8f, 10f));
        Anchor(parent, "Spawn_Boss_Mandrill", SceneAnchorType.BossSpawn, 4, new Vector3(0f, 1.1f, 20f));
        Anchor(parent, "Reveal_Deity", SceneAnchorType.DeityReveal, 5, new Vector3(0f, 5.8f, 11f));
        Anchor(parent, "Focus_EndingChoice", SceneAnchorType.EndingFocus, 5, new Vector3(0f, 1.4f, 8f));
    }

    private static GameObject BuildSealScar(Transform parent)
    {
        GameObject group = NewObject(parent, "EndingScar_Seal_RedBindings");
        for (int i = 0; i < 5; i++)
            Box(group.transform, "Seal_" + (i + 1), new Vector3(-1.8f + i * 0.9f, 1.7f + (i % 2) * 0.55f, 9.75f), new Vector3(0.6f, 1.7f, 0.08f), Red, Red * 1.5f);
        return group;
    }

    private static GameObject BuildCoexistScar(Transform parent)
    {
        GameObject group = NewObject(parent, "EndingScar_Coexist_CyanHalo");
        Cylinder(group.transform, "Coexist_Core", new Vector3(0f, 2.4f, 9.8f), new Vector3(2.2f, 0.08f, 2.2f), Cyan, Cyan * 1.8f, Quaternion.Euler(90f, 0f, 0f));
        return group;
    }

    private static GameObject BuildTransferScar(Transform parent)
    {
        GameObject group = NewObject(parent, "EndingScar_Transfer_EmptyShell");
        Box(group.transform, "Transfer_Crack", new Vector3(0f, 1.8f, 9.78f), new Vector3(0.18f, 3f, 0.12f), Gold, Gold * 1.7f, Quaternion.Euler(0f, 0f, 22f));
        Box(group.transform, "Transfer_Fragment", new Vector3(0.9f, 0.7f, 9.4f), new Vector3(0.7f, 0.25f, 0.5f), Stone, Gold * 0.8f, Quaternion.Euler(12f, 30f, 8f));
        return group;
    }

    private static ParticleSystem BuildRain(Transform parent)
    {
        GameObject target = NewObject(parent, "RainVolume");
        target.transform.position = new Vector3(0f, 15f, 8f);
        target.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        ParticleSystem rain = target.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = rain.main;
        main.loop = true;
        main.startLifetime = 1.8f;
        main.startSpeed = 18f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.018f, 0.035f);
        main.startColor = new Color(0.45f, 0.72f, 0.85f, 0.55f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 2400;
        ParticleSystem.EmissionModule emission = rain.emission;
        emission.rateOverTime = 650f;
        ParticleSystem.ShapeModule shape = rain.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(25f, 1f, 32f);
        return rain;
    }

    private static void BuildMoon(Transform parent)
    {
        GameObject target = NewObject(parent, "Moon_KeyLight");
        target.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        Light light = target.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.35f, 0.48f, 0.68f);
        light.intensity = 0.55f;
        light.shadows = LightShadows.Soft;
    }

    private static void Lantern(Transform parent, string name, Vector3 position)
    {
        Cylinder(parent, name + "_Post", position, new Vector3(0.12f, 1.2f, 0.12f), Dark);
        Box(parent, name + "_Glow", position + Vector3.up * 1.35f, new Vector3(0.45f, 0.65f, 0.45f), Red, Red * 1.6f);
    }

    private static void CameraAnchor(Transform parent, string name, int act, Vector3 position, Vector3 target)
    {
        GameObject anchor = Anchor(parent, name, SceneAnchorType.Camera, act, position);
        anchor.transform.LookAt(target);
    }

    private static GameObject Anchor(Transform parent, string name, SceneAnchorType type, int act, Vector3 position)
    {
        GameObject anchor = NewObject(parent, name);
        anchor.transform.position = position;
        anchor.AddComponent<SceneAnchor>().Configure(type, act, name);
        return anchor;
    }

    private static Light PointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject target = NewObject(parent, name);
        target.transform.position = position;
        Light light = target.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
        return light;
    }

    private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Color? emission = null, Quaternion? rotation = null)
    {
        return Primitive(PrimitiveType.Cube, parent, name, position, scale, color, emission, rotation);
    }

    private static GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Color? emission = null, Quaternion? rotation = null)
    {
        return Primitive(PrimitiveType.Cylinder, parent, name, position, scale, color, emission, rotation);
    }

    private static GameObject Primitive(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Color color, Color? emission, Quaternion? rotation)
    {
        GameObject target = GameObject.CreatePrimitive(type);
        target.name = name;
        target.transform.SetParent(parent, false);
        target.transform.position = position;
        target.transform.localScale = scale;
        target.transform.rotation = rotation ?? Quaternion.identity;
        Renderer renderer = target.GetComponent<Renderer>();
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        if (emission.HasValue)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        renderer.material = material;
        return target;
    }

    private static Transform Group(Transform parent, string name) => NewObject(parent, name).transform;

    private static GameObject NewObject(Transform parent, string name)
    {
        GameObject target = new GameObject(name);
        target.transform.SetParent(parent, false);
        return target;
    }
}