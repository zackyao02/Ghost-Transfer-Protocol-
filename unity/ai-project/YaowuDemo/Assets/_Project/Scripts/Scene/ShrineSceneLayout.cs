using UnityEngine;

public static class ShrineSceneLayout
{
    public const string RootName = "Scene_A_CyberShrine";
    private static readonly Color Dark = new Color(0.07f, 0.075f, 0.085f);
    private static readonly Color Stone = new Color(0.26f, 0.28f, 0.30f);
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
        BuildRitualFloor(architecture);
        BuildSideShrines(architecture);
        ShrineSceneArt.Build(architecture);
        BuildSkyline(skyline);
        BuildEclipse(skyline);
        BuildAnchors(anchors, playerSpawn);

        Light altarLight = PointLight(atmosphere, "Altar_CyanLight", new Vector3(0f, 3.2f, 8f), Cyan, 2.3f, 17f);
        Light bossLight = PointLight(atmosphere, "Boss_RedLight", new Vector3(0f, 4f, 20f), Red, 0f, 15f);
        BuildMoon(atmosphere);
        ParticleSystem rain = BuildRain(atmosphere);

        GameObject seal = BuildSealScar(scars);
        GameObject coexist = BuildCoexistScar(scars);
        GameObject transfer = BuildTransferScar(scars);
        ShrineCameraGrade.Attach(Camera.main);
        controller.Initialize(altarLight, bossLight, rain, seal, coexist, transfer);
        return controller;
    }

    private static void BuildArchitecture(Transform parent)
    {
        Box(parent, "CourtyardFloor", new Vector3(0f, -0.3f, 8f), new Vector3(26f, 0.5f, 34f), new Color(0.18f, 0.20f, 0.23f));
        Box(parent, "LeftBoundary", new Vector3(-13f, 1.2f, 8f), new Vector3(0.5f, 2.4f, 34f), Dark);
        Box(parent, "RightBoundary", new Vector3(13f, 1.2f, 8f), new Vector3(0.5f, 2.4f, 34f), Dark);
        Box(parent, "CourtyardFarWall", new Vector3(0f, 1.3f, 24f), new Vector3(26f, 2.6f, 0.5f), Dark);

        for (int i = 0; i < 5; i++)
        {
            float z = -5f + i * 4.6f;
            Lantern(parent, "Lantern_L_" + (i + 1), new Vector3(-6.2f, 1.2f, z));
            Lantern(parent, "Lantern_R_" + (i + 1), new Vector3(6.2f, 1.2f, z));
        }

        Box(parent, "AltarStep_Low", new Vector3(0f, 0.14f, 5.7f), new Vector3(9.5f, 0.28f, 1.6f), Stone);
        Box(parent, "AltarStep_High", new Vector3(0f, 0.35f, 6.7f), new Vector3(8.4f, 0.36f, 1.3f), Dark);
        Box(parent, "AltarBase", new Vector3(0f, 0.65f, 9f), new Vector3(8f, 0.9f, 4.5f), Dark);
        Box(parent, "AltarGoldRim", new Vector3(0f, 1.14f, 9f), new Vector3(8.25f, 0.1f, 4.7f), Gold, Gold * 0.15f);
        Box(parent, "AltarTop", new Vector3(0f, 1.24f, 9f), new Vector3(7.6f, 0.15f, 4.25f), Stone);
        Box(parent, "MemorySlab", new Vector3(0f, 2.6f, 10.5f), new Vector3(1.8f, 2.7f, 0.36f), Dark);
        Box(parent, "MemorySlab_Light", new Vector3(0f, 2.6f, 10.25f), new Vector3(0.16f, 2.1f, 0.04f), Cyan, Cyan * 1.8f);
        Box(parent, "MemorySlab_Cross", new Vector3(0f, 2.6f, 10.23f), new Vector3(1.2f, 0.12f, 0.04f), Cyan, Cyan * 1.8f);

        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 4.8f;
            Cylinder(parent, "ShrineColumn_" + side, new Vector3(x, 2.4f, 13.8f), new Vector3(0.38f, 2.4f, 0.38f), Red);
            Box(parent, "ShrineBracket_" + side, new Vector3(x, 4.9f, 13.8f), new Vector3(1.5f, 0.22f, 2f), Gold);
            Box(parent, "EaveUpturn_" + side, new Vector3(side * 6.5f, 5.45f, 13.8f), new Vector3(3f, 0.4f, 4.5f), Dark,
                null, Quaternion.Euler(0f, 0f, side * 18f));
        }

        Box(parent, "ShrineBack", new Vector3(0f, 2.5f, 15f), new Vector3(11f, 5f, 0.9f), new Color(0.11f, 0.105f, 0.115f));
        Box(parent, "ShrineLintel", new Vector3(0f, 4.7f, 13.7f), new Vector3(10.5f, 0.35f, 0.6f), Gold);
        Box(parent, "ShrineRoofLower", new Vector3(0f, 5.25f, 13.8f), new Vector3(14f, 0.34f, 5f), Red);
        Box(parent, "ShrineRoofUpper", new Vector3(0f, 5.9f, 13.8f), new Vector3(10.8f, 0.55f, 4.2f), Dark);
        Box(parent, "ShrineRidge", new Vector3(0f, 6.35f, 13.8f), new Vector3(11.7f, 0.22f, 0.5f), Gold, Gold * 0.15f);

        for (int i = -2; i <= 2; i++)
        {
            Box(parent, "RoofRafter_" + i, new Vector3(i * 2.2f, 5.06f, 11.42f), new Vector3(0.16f, 0.18f, 0.5f), Gold);
        }

        Box(parent, "BossGate_LeftPillar", new Vector3(-4.3f, 3.2f, 20f), new Vector3(1.1f, 6.4f, 1.1f), Red);
        Box(parent, "BossGate_RightPillar", new Vector3(4.3f, 3.2f, 20f), new Vector3(1.1f, 6.4f, 1.1f), Red);
        Box(parent, "BossGate_Beam", new Vector3(0f, 6.2f, 20f), new Vector3(11f, 0.8f, 1.2f), Red);
        Box(parent, "BossGate_TopBeam", new Vector3(0f, 7f, 20f), new Vector3(8.5f, 0.45f, 1f), Dark);
        Box(parent, "BossGate_GoldEdge", new Vector3(0f, 6.52f, 19.36f), new Vector3(9.8f, 0.1f, 0.08f), Gold, Gold * 0.5f);
        Box(parent, "BossGate_Rift", new Vector3(0f, 3f, 20.55f), new Vector3(5.8f, 5.5f, 0.08f), new Color(0.13f, 0.015f, 0.025f), Red * 0.35f);
    }

    private static void BuildRitualFloor(Transform parent)
    {
        Color tileA = new Color(0.25f, 0.28f, 0.31f);
        Color tileB = new Color(0.18f, 0.21f, 0.24f);

        for (int row = 0; row < 13; row++)
        {
            float z = -8.3f + row * 1.15f;
            for (int lane = -1; lane <= 1; lane++)
            {
                float x = lane * 1.75f;
                float offset = ((row + lane + 3) % 3 == 0) ? 0.12f : 0f;
                Box(parent, "PathStone_" + row + "_" + lane, new Vector3(x, 0.01f + offset * 0.15f, z),
                    new Vector3(1.68f, 0.06f, 1.04f), (row + lane) % 2 == 0 ? tileA : tileB);
            }

            Box(parent, "PathLight_L_" + row, new Vector3(-2.65f, 0.055f, z), new Vector3(0.035f, 0.02f, 0.82f), Cyan, Cyan * 1.5f);
            Box(parent, "PathLight_R_" + row, new Vector3(2.65f, 0.055f, z), new Vector3(0.035f, 0.02f, 0.82f), Cyan, Cyan * 1.5f);
        }

        for (int ring = 0; ring < 3; ring++)
        {
            float radius = 4.2f + ring * 1.2f;
            for (int segment = 0; segment < 24; segment++)
            {
                float angle = segment * Mathf.PI * 2f / 24f;
                Vector3 position = new Vector3(Mathf.Sin(angle) * radius, 0.065f, 7f + Mathf.Cos(angle) * radius);
                Box(parent, "RitualRing_" + ring + "_" + segment, position,
                    new Vector3(0.07f, 0.025f, ring == 1 ? 0.65f : 0.43f), Cyan, Cyan * 1.5f,
                    Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f));
            }
        }
    }

    private static void BuildSideShrines(Transform parent)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 9.5f;
            Box(parent, "SideShrine_Platform_" + side, new Vector3(x, 0.22f, 5f), new Vector3(3.3f, 0.45f, 3.3f), Stone);
            Box(parent, "SideShrine_Body_" + side, new Vector3(x, 1.55f, 5f), new Vector3(2.1f, 2.3f, 1.7f), Dark);
            Box(parent, "SideShrine_Roof_" + side, new Vector3(x, 3f, 5f), new Vector3(3.2f, 0.32f, 2.5f), Red);
            Box(parent, "SideShrine_Sigil_" + side, new Vector3(x, 1.7f, 4.1f), new Vector3(0.9f, 0.12f, 0.05f), Cyan, Cyan * 1.6f);
            Box(parent, "SideShrine_SigilVertical_" + side, new Vector3(x, 1.7f, 4.08f), new Vector3(0.12f, 1.2f, 0.05f), Cyan, Cyan * 1.6f);
            Box(parent, "WardPillar_" + side, new Vector3(side * 9f, 2.9f, 11f), new Vector3(1.15f, 5.8f, 1.15f), Stone);
            Box(parent, "WardPillar_Cap_" + side, new Vector3(side * 9f, 5.9f, 11f), new Vector3(1.8f, 0.35f, 1.8f), Dark);
            Box(parent, "WardPillar_Rune_" + side, new Vector3(side * 9f, 3.2f, 10.4f), new Vector3(0.14f, 2f, 0.06f), Cyan, Cyan * 2f);
        }
    }

    private static void BuildSkyline(Transform parent)
    {
        for (int i = 0; i < 12; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float height = 7f + (i % 5) * 2.4f;
            float x = side * (16f + (i % 4) * 3.1f);
            float z = 23f + (i % 3) * 4f;
            float facingX = x - side * 1.26f;
            Box(parent, "Tower_" + (i + 1), new Vector3(x, height * 0.5f, z),
                new Vector3(2.4f, height, 2.5f), new Color(0.055f, 0.075f, 0.105f));
            Box(parent, "Tower_Antenna_" + (i + 1), new Vector3(x, height + 1f, z),
                new Vector3(0.08f, 2f, 0.08f), Dark);
            Box(parent, "Tower_AntennaLight_" + (i + 1), new Vector3(x, height + 2f, z),
                new Vector3(0.16f, 0.16f, 0.16f), Red, Red * 1.8f);

            for (int window = 0; window < 4; window++)
            {
                float y = 2f + window * 2.2f;
                if (y > height - 0.7f) break;
                Box(parent, "Tower_Window_" + i + "_" + window, new Vector3(facingX, y, z),
                    new Vector3(0.05f, 0.55f, 0.65f), window % 3 == 0 ? Red : Cyan,
                    (window % 3 == 0 ? Red : Cyan) * 0.7f);
            }
        }
    }

    private static void BuildEclipse(Transform parent)
    {
        Vector3 center = new Vector3(0f, 12.5f, 29f);
        Primitive(PrimitiveType.Sphere, parent, "GhostMoon_DarkCore", center,
            new Vector3(4.8f, 4.8f, 0.6f), new Color(0.08f, 0.13f, 0.18f), null, null);

        for (int ring = 0; ring < 2; ring++)
        {
            float radius = 3f + ring * 0.58f;
            for (int segment = 0; segment < 32; segment++)
            {
                float angle = segment * Mathf.PI * 2f / 32f;
                Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -0.45f);
                Box(parent, "GhostMoon_Halo_" + ring + "_" + segment, position,
                    new Vector3(0.11f, 0.68f, 0.09f), ring == 0 ? Cyan : Gold,
                    (ring == 0 ? Cyan : Gold) * (ring == 0 ? 1.2f : 0.65f),
                    Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg - 90f));
            }
        }

        Box(parent, "GhostMoon_VerticalSignal", center + new Vector3(0f, 0f, -0.55f),
            new Vector3(0.09f, 4.3f, 0.08f), Red, Red * 1.1f);
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
        emission.rateOverTime = 260f;
        ParticleSystem.ShapeModule shape = rain.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(25f, 1f, 32f);
        ParticleSystemRenderer rainRenderer = rain.GetComponent<ParticleSystemRenderer>();
        Shader rainShader = Shader.Find("Particles/Standard Unlit");
        if (rainShader == null) rainShader = Shader.Find("Sprites/Default");
        if (rainShader != null)
        {
            rainRenderer.sharedMaterial = new Material(rainShader);
        }
        rainRenderer.renderMode = ParticleSystemRenderMode.Stretch;
        rainRenderer.lengthScale = 1.35f;
        rainRenderer.velocityScale = 0.04f;
        return rain;
    }

    private static void BuildMoon(Transform parent)
    {
        GameObject target = NewObject(parent, "Moon_KeyLight");
        target.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
        Light light = target.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.55f, 0.72f, 0.9f);
        light.intensity = 0.75f;
        light.shadows = LightShadows.Soft;
        Light[] sceneLights = Object.FindObjectsOfType<Light>();
        foreach (Light sceneLight in sceneLights)
        {
            if (sceneLight != light && sceneLight.type == LightType.Directional)
            {
                sceneLight.intensity = 0.18f;
            }
        }
    }

    private static void Lantern(Transform parent, string name, Vector3 position)
    {
        Cylinder(parent, name + "_Post", position, new Vector3(0.16f, 1.2f, 0.16f), Dark);
        Box(parent, name + "_Base", position + Vector3.down * 1.08f, new Vector3(0.8f, 0.2f, 0.8f), Stone);
        Box(parent, name + "_Glow", position + Vector3.up * 1.3f,
            new Vector3(0.5f, 0.65f, 0.5f), Gold, Gold * 1.5f);
        Box(parent, name + "_Top", position + Vector3.up * 1.73f,
            new Vector3(0.85f, 0.22f, 0.85f), Red);
        Box(parent, name + "_Eave", position + Vector3.up * 1.9f,
            new Vector3(0.65f, 0.14f, 0.65f), Dark);
        Box(parent, name + "_FrontBar", position + new Vector3(0f, 1.3f, -0.27f),
            new Vector3(0.08f, 0.72f, 0.09f), Dark);
        Box(parent, name + "_SideBar", position + new Vector3(0.27f, 1.3f, 0f),
            new Vector3(0.09f, 0.72f, 0.08f), Dark);
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
        Collider collider = target.GetComponent<Collider>();
        if (collider != null && name != "CourtyardFloor") collider.enabled = false;
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