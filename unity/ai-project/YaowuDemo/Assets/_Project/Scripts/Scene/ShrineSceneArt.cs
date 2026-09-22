using System.Collections.Generic;
using UnityEngine;

// Member A owns this presentation layer. Anchors and gameplay remain in the existing scene scripts.
public static class ShrineSceneArt
{
    private static readonly Color Charcoal = Hex("090A0F");
    private static readonly Color Slate = new Color(0.16f, 0.21f, 0.25f);
    private static readonly Color Roof = new Color(0.30f, 0.09f, 0.085f);
    private static readonly Color Vermilion = Hex("E44732");
    private static readonly Color Cyan = Hex("39E6D0");
    private static readonly Color Gold = Hex("E7C45A");
    private static readonly Color Bronze = new Color(0.43f, 0.29f, 0.14f);
    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public static void Build(Transform architecture)
    {
        Transform art = Group(architecture, "ArtDirection");
        Camera sceneCamera = Camera.main;
        if (sceneCamera != null)
            sceneCamera.backgroundColor = new Color(0.085f, 0.13f, 0.18f);
        ApplyBasalt(architecture);
        RefinishExisting(architecture);
        BuildEntrance(art);
        BuildShrineRoof(art);
        BuildAltarDetails(art);
        BuildCourtyard(art);
        BuildCyberHardware(art);
        BuildActLayers(art);
        BuildArtLights(art);
    }

    public static void ApplyAct(Transform sceneRoot, int act)
    {
        Transform art = sceneRoot.Find("Architecture/ArtDirection");
        if (art == null) return;
        // Lower the memory monolith during the Boss beat so its spawn gate has a clear sightline.
        string[] memoryPieces = { "MemorySlab", "MemorySlab_Light", "MemorySlab_Cross" };
        foreach (string name in memoryPieces)
        {
            Transform piece = sceneRoot.Find("Architecture/" + name);
            if (piece != null) piece.gameObject.SetActive(act != 4);
        }
        string[] reliefPieces = { "MemoryCrown", "MemoryEye" };
        foreach (string name in reliefPieces)
        {
            Transform piece = art.Find("Altar_MechanicalRelief/" + name);
            if (piece != null) piece.gameObject.SetActive(act != 4);
        }
        for (int index = 1; index <= 5; index++)
        {
            Transform layer = art.Find("Act_" + index);
            if (layer != null) layer.gameObject.SetActive(index == act);
        }
    }

    private static void ApplyBasalt(Transform architecture)
    {
        Texture2D texture = Resources.Load<Texture2D>("SceneTextures/BasaltPaving");
        if (texture == null)
        {
            Debug.LogWarning("Scene A: basalt texture missing; using flat stone fallback.");
            return;
        }
        texture.wrapMode = TextureWrapMode.Repeat;
        foreach (Renderer renderer in architecture.GetComponentsInChildren<Renderer>())
        {
            string name = renderer.gameObject.name;
            if (name != "CourtyardFloor" && !name.StartsWith("PathStone_")
                && name != "AltarTop" && !name.StartsWith("SideShrine_Platform_")
                && !name.StartsWith("AltarStep_")) continue;
            Material material = renderer.sharedMaterial;
            material.mainTexture = texture;
            material.mainTextureScale = name == "CourtyardFloor"
                ? new Vector2(7f, 10f) : name.StartsWith("PathStone_")
                    ? new Vector2(1.3f, 1f) : new Vector2(2f, 2f);
            material.color = name == "CourtyardFloor"
                ? new Color(0.68f, 0.75f, 0.82f) : new Color(0.86f, 0.91f, 0.95f);
            material.SetFloat("_Glossiness", 0.13f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", name == "CourtyardFloor"
                ? new Color(0.018f, 0.029f, 0.042f)
                : new Color(0.035f, 0.054f, 0.071f));
        }
    }

    private static void RefinishExisting(Transform architecture)
    {
        foreach (Renderer renderer in architecture.GetComponentsInChildren<Renderer>())
        {
            string name = renderer.gameObject.name;
            if (name == "ShrineRoofLower" || name.StartsWith("SideShrine_Roof_"))
                renderer.sharedMaterial.color = Roof;
            if (name.StartsWith("BossGate_") && !name.Contains("Gold") && !name.Contains("Rift"))
                renderer.sharedMaterial.color = Roof;
            if (name == "MemorySlab") renderer.sharedMaterial.color = Slate;
            if (name == "LeftBoundary" || name == "RightBoundary" || name == "CourtyardFarWall")
                renderer.sharedMaterial.color = new Color(0.17f, 0.22f, 0.27f);
            if (name.StartsWith("SideShrine_Body_") || name == "ShrineRoofUpper")
                renderer.sharedMaterial.color = new Color(0.20f, 0.25f, 0.29f);
            if (name.StartsWith("Tower_") && !name.Contains("Window") && !name.Contains("Antenna"))
                renderer.sharedMaterial.color = new Color(0.12f, 0.17f, 0.22f);
            if (name == "ShrineBack") renderer.gameObject.SetActive(false);
            if (name.EndsWith("_Glow"))
            {
                renderer.sharedMaterial.color = new Color(0.48f, 0.34f, 0.16f);
                renderer.sharedMaterial.SetColor("_EmissionColor", Gold * 0.70f);
            }
        }
    }

    private static void BuildEntrance(Transform parent)
    {
        Transform gate = Group(parent, "Entrance_ToriiFrame");
        Box(gate, "Foot_Left", new Vector3(-5.5f, 0.18f, -5.4f), new Vector3(1.4f, 0.36f, 1.4f), Slate);
        Box(gate, "Foot_Right", new Vector3(5.5f, 0.18f, -5.4f), new Vector3(1.4f, 0.36f, 1.4f), Slate);
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 5.5f;
            Box(gate, "Pillar_" + side, new Vector3(x, 2.8f, -5.4f), new Vector3(0.72f, 5.6f, 0.72f), Roof);
            Box(gate, "Pillar_GoldInlay_" + side, new Vector3(x, 3.5f, -5.78f), new Vector3(0.12f, 2.2f, 0.03f), Gold, 0.9f);
            Box(gate, "Pillar_Collar_" + side, new Vector3(x, 4.8f, -5.4f), new Vector3(0.95f, 0.20f, 0.92f), Charcoal);
            Box(gate, "HangingOfuda_" + side, new Vector3(side * 4.25f, 4.5f, -5.7f), new Vector3(0.55f, 1.55f, 0.055f), Gold);
            Box(gate, "OfudaStroke_" + side, new Vector3(side * 4.25f, 4.45f, -5.75f), new Vector3(0.1f, 1.0f, 0.02f), Vermilion);
        }
        Box(gate, "Lintel", new Vector3(0f, 5.25f, -5.4f), new Vector3(12.5f, 0.48f, 1.05f), Roof);
        Box(gate, "LintelBlackCap", new Vector3(0f, 5.64f, -5.4f), new Vector3(13.9f, 0.36f, 1.3f), Charcoal);
        Box(gate, "LintelGoldLine", new Vector3(0f, 5.01f, -5.94f), new Vector3(10.7f, 0.045f, 0.05f), Gold, 1.2f);
        Box(gate, "NamePlaque", new Vector3(0f, 4.54f, -5.98f), new Vector3(2.15f, 0.72f, 0.14f), Charcoal);
        Box(gate, "NamePlaqueGlyph", new Vector3(0f, 4.54f, -6.07f), new Vector3(0.14f, 0.46f, 0.025f), Cyan, 1.4f);
    }

    private static void BuildShrineRoof(Transform parent)
    {
        Transform roof = Group(parent, "Shrine_RoofCraft");
        // Existing roof is the structural underside; these pitched planes create the recognizable silhouette.
        for (int side = -1; side <= 1; side += 2)
        {
            Box(roof, "PitchedRoof_" + side, new Vector3(side * 3.7f, 6.13f, 13.8f),
                new Vector3(7.65f, 0.23f, 5.35f), Roof, 0f,
                Quaternion.Euler(0f, 0f, side * 10f));
            Box(roof, "EaveGold_" + side, new Vector3(side * 7.05f, 5.52f, 13.8f),
                new Vector3(0.12f, 0.14f, 5.55f), Gold, 0.5f);
            for (int row = 0; row < 4; row++)
            {
                float x = side * (1.1f + row * 1.75f);
                Beam(roof, "RoofTileRidge_" + side + "_" + row,
                    new Vector3(x, 6.58f - row * 0.17f, 11.08f),
                    new Vector3(x, 6.58f - row * 0.17f, 16.53f), 0.09f, Slate);
            }
            Box(roof, "SideScreen_" + side, new Vector3(side * 5.65f, 3.05f, 13.1f),
                new Vector3(0.13f, 2.45f, 3.0f), Charcoal);
            for (int stripe = 0; stripe < 3; stripe++)
                Box(roof, "SideScreenLine_" + side + "_" + stripe,
                    new Vector3(side * 5.75f, 2.2f + stripe * 0.8f, 11.8f),
                    new Vector3(0.035f, 0.05f, 0.5f), Cyan, 0.45f);
        }
        Box(roof, "RoofCrest", new Vector3(0f, 6.82f, 13.8f),
            new Vector3(0.42f, 0.42f, 5.65f), Charcoal);
        for (int i = 0; i < 7; i++)
            Box(roof, "EaveTooth_" + i, new Vector3(-5.4f + i * 1.8f, 5.12f, 10.99f),
                new Vector3(0.17f, 0.38f, 0.42f), Gold);
        // Open central sightline for the Boss gate instead of leaving a solid rear wall.
        Box(roof, "PortalWall_Left", new Vector3(-3.9f, 2.25f, 15f),
            new Vector3(3.2f, 4.5f, 0.9f), Charcoal);
        Box(roof, "PortalWall_Right", new Vector3(3.9f, 2.25f, 15f),
            new Vector3(3.2f, 4.5f, 0.9f), Charcoal);
        Box(roof, "PortalLintel", new Vector3(0f, 4.72f, 15f),
            new Vector3(4.8f, 0.56f, 1.0f), Roof);
        for (int side = -1; side <= 1; side += 2)
        {
            Box(roof, "PortalJamb_" + side, new Vector3(side * 2.25f, 2.2f, 14.48f),
                new Vector3(0.16f, 4.4f, 0.11f), Gold, 0.35f);
            Box(roof, "PortalSignal_" + side, new Vector3(side * 2.25f, 2.2f, 14.39f),
                new Vector3(0.045f, 3.4f, 0.025f), Cyan, 0.8f);
        }
        // Vertical slats break up the rear walls without covering the portal.
        for (int i = 0; i < 9; i += 6)
            Box(roof, "BackWallSlat_" + i, new Vector3(-4.4f + i * 1.1f, 2.4f, 14.48f),
                new Vector3(0.10f, 3.6f, 0.09f), new Color(0.36f, 0.21f, 0.15f));
    }

    private static void BuildAltarDetails(Transform parent)
    {
        Transform altar = Group(parent, "Altar_MechanicalRelief");
        for (int side = -1; side <= 1; side += 2)
        {
            Box(altar, "AltarSideRail_" + side, new Vector3(side * 4.25f, 0.65f, 8.6f),
                new Vector3(0.16f, 0.52f, 5.5f), Bronze);
            Box(altar, "AltarCable_" + side, new Vector3(side * 2.2f, 1.38f, 9f),
                new Vector3(0.055f, 0.05f, 3.0f), Cyan, 0.7f);
            for (int i = 0; i < 3; i++)
                Box(altar, "AltarVent_" + side + "_" + i,
                    new Vector3(side * 3.3f, 0.68f, 7.3f + i * 1.0f),
                    new Vector3(0.66f, 0.04f, 0.08f), Cyan, 0.7f);
            Box(altar, "MemoryWing_" + side, new Vector3(side * 1.25f, 2.45f, 10.35f),
                new Vector3(0.16f, 2.2f, 0.12f), Bronze);
            Box(altar, "MemoryWingLight_" + side, new Vector3(side * 1.25f, 2.45f, 10.26f),
                new Vector3(0.045f, 1.55f, 0.025f), Cyan, 1.2f);
        }
        for (int i = 0; i < 7; i++)
            Box(altar, "StepEdge_" + i, new Vector3(-3.3f + i * 1.1f, 0.31f, 5.89f),
                new Vector3(0.68f, 0.035f, 0.06f), Gold, 0.35f);
        Box(altar, "MemoryCrown", new Vector3(0f, 4.05f, 10.5f),
            new Vector3(2.55f, 0.3f, 0.6f), Bronze);
        Box(altar, "MemoryEye", new Vector3(0f, 3.34f, 10.2f),
            new Vector3(0.42f, 0.42f, 0.055f), Gold, 1.4f);
    }

    private static void BuildCourtyard(Transform parent)
    {
        Transform details = Group(parent, "Courtyard_DebrisAndDepth");
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 5; i++)
            {
                float z = -5f + i * 5.5f;
                float x = side * 12.65f;
                Box(details, "BoundaryButtress_" + side + "_" + i,
                    new Vector3(x, 1.65f, z), new Vector3(0.8f, 3.3f, 1.1f), Slate);
                Box(details, "ButtressCap_" + side + "_" + i,
                    new Vector3(x, 3.25f, z), new Vector3(1.05f, 0.16f, 1.3f), Charcoal);
                Box(details, "ButtressSignal_" + side + "_" + i,
                    new Vector3(x - side * 0.43f, 2.0f, z),
                    new Vector3(0.04f, 1.4f, 0.08f), i % 2 == 0 ? Cyan : Vermilion, 0.9f);
            }
            for (int i = 0; i < 6; i++)
            {
                float z = -7.2f + i * 4.2f;
                Box(details, "BrokenStone_" + side + "_" + i,
                    new Vector3(side * (7.0f + i % 3), 0.16f, z),
                    new Vector3(0.8f + (i % 2) * 0.5f, 0.24f, 0.75f),
                    i % 2 == 0 ? Slate : Charcoal, 0f,
                    Quaternion.Euler(0f, i * 29f, (i % 3 - 1) * 8f));
            }
            // Low dark water catches the cyan light and gives the floor a second material read.
            Box(details, "RainPool_" + side,
                new Vector3(side * 9.2f, -0.021f, -1f),
                new Vector3(4.0f, 0.018f, 6.0f),
                new Color(0.025f, 0.10f, 0.15f));
            Box(details, "PoolReflection_" + side,
                new Vector3(side * 9.2f, -0.008f, -1f),
                new Vector3(0.07f, 0.01f, 3.1f), Cyan, 0.35f);
        }
        for (int row = 0; row < 9; row++)
        {
            float z = -6.8f + row * 2.5f;
            Box(details, "PathSeam_" + row, new Vector3(0f, 0.065f, z),
                new Vector3(4.9f, 0.008f, 0.018f), Charcoal);
        }
    }

    private static void BuildCyberHardware(Transform parent)
    {
        Transform hardware = Group(parent, "GhostShell_Interfaces");
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * 9.5f;
            // A shell-transfer docking frame turns each side shrine into a readable machine.
            Box(hardware, "DockBack_" + side, new Vector3(x, 1.65f, 4.05f),
                new Vector3(1.55f, 2.15f, 0.12f), Charcoal);
            Box(hardware, "DockHeader_" + side, new Vector3(x, 2.76f, 4.0f),
                new Vector3(1.83f, 0.18f, 0.25f), Slate);
            for (int rail = -1; rail <= 1; rail += 2)
            {
                Box(hardware, "DockRail_" + side + "_" + rail,
                    new Vector3(x + rail * 0.79f, 1.65f, 3.95f),
                    new Vector3(0.12f, 2.25f, 0.20f), Bronze);
                Box(hardware, "DockData_" + side + "_" + rail,
                    new Vector3(x + rail * 0.58f, 1.64f, 3.96f),
                    new Vector3(0.035f, 1.65f, 0.025f), Cyan, 0.9f);
            }
            Ring(hardware, "ShellSocket_" + side,
                new Vector3(x, 1.68f, 3.91f), 0.55f, Cyan, 20, false);
            Box(hardware, "ShellCore_" + side, new Vector3(x, 1.68f, 3.87f),
                new Vector3(0.24f, 0.24f, 0.045f), Gold, 0.8f);
            Box(hardware, "DockFoot_" + side, new Vector3(x, 0.54f, 3.65f),
                new Vector3(2.0f, 0.18f, 0.86f), Slate);
            Beam(hardware, "DataConduit_" + side,
                new Vector3(x, 3.05f, 5.0f), new Vector3(side * 6.1f, 4.6f, 11.1f),
                0.09f, Charcoal);
            Beam(hardware, "DataConduitLit_" + side,
                new Vector3(x, 3.08f, 4.98f), new Vector3(side * 6.1f, 4.63f, 11.08f),
                0.025f, Cyan, 0.45f);
        }
        for (int i = -2; i <= 2; i++)
        {
            Box(hardware, "AltarCircuit_" + i, new Vector3(i * 0.6f, 1.34f, 8.55f),
                new Vector3(0.12f, 0.018f, 1.35f), Cyan, 0.6f);
            Box(hardware, "AltarContact_" + i, new Vector3(i * 0.6f, 1.36f, 7.85f),
                new Vector3(0.30f, 0.025f, 0.16f), Gold, 0.4f);
        }
    }
    private static void BuildActLayers(Transform parent)
    {
        Transform calibration = Group(parent, "Act_1");
        Ring(calibration, "Calibration", new Vector3(0f, 2.65f, 10.12f), 1.4f, Cyan, 24, false);
        Box(calibration, "CalibrationCore", new Vector3(0f, 2.65f, 10.05f),
            new Vector3(0.16f, 0.16f, 0.04f), Cyan, 2f);

        Transform corruption = Group(parent, "Act_2");
        for (int side = -1; side <= 1; side += 2)
        {
            Ring(corruption, "Rift_" + side, new Vector3(side * 4.2f, 0.08f, 10f),
                1.4f, Vermilion, 18, true);
            Box(corruption, "RiftCore_" + side,
                new Vector3(side * 4.2f, 0.11f, 10f),
                new Vector3(1.55f, 0.025f, 0.15f), Vermilion, 1.4f,
                Quaternion.Euler(0f, side * 30f, 0f));
        }

        Transform memory = Group(parent, "Act_3");
        for (int i = 0; i < 5; i++)
            Box(memory, "MemoryDataColumn_" + i,
                new Vector3(-2.4f + i * 1.2f, 2.2f + (i % 2) * 0.4f, 10.0f),
                new Vector3(0.035f, 2.2f + (i % 2) * 0.8f, 0.04f), Cyan, 1.1f);

        Transform boss = Group(parent, "Act_4");
        Ring(boss, "BossWarning", new Vector3(0f, 0.09f, 17.3f), 3.2f, Vermilion, 32, true);
        for (int i = -2; i <= 2; i++)
            Box(boss, "GatePulse_" + i, new Vector3(i * 1.0f, 3.1f, 19.38f),
                new Vector3(0.07f, 3.5f, 0.04f), Vermilion, 1.3f);

        Transform deity = Group(parent, "Act_5");
        Ring(deity, "DeityHalo", new Vector3(0f, 5.8f, 10.05f), 2.7f, Gold, 40, false);
        Ring(deity, "DeityFloorSeal", new Vector3(0f, 1.36f, 9f), 2.2f, Gold, 32, true);
        for (int i = 0; i < 7; i++)
            Box(deity, "DeityRay_" + i, new Vector3(-3f + i, 3.7f, 10.1f),
                new Vector3(0.05f, 3.4f + (3 - Mathf.Abs(i - 3)) * 0.5f, 0.045f),
                Gold, 0.75f);
    }

    private static void BuildArtLights(Transform parent)
    {
        Transform lights = Group(parent, "ArtLights");
        Point(lights, "ShrineInterior_Warm", new Vector3(0f, 4.4f, 11.8f),
            new Color(1f, 0.61f, 0.29f), 2.8f, 11f);
        Point(lights, "RoofRim_Red", new Vector3(0f, 7.2f, 17.1f),
            Vermilion, 2.1f, 15f);
        Point(lights, "CourtyardFill_Left", new Vector3(-8f, 3.5f, -1f),
            new Color(0.22f, 0.63f, 0.76f), 1.4f, 13f);
        Point(lights, "CourtyardFill_Right", new Vector3(8f, 3.5f, -1f),
            new Color(0.22f, 0.63f, 0.76f), 1.4f, 13f);
        Point(lights, "GateLantern_Left", new Vector3(-5.5f, 3f, -5.2f),
            Gold, 1.0f, 7f);
        Point(lights, "GateLantern_Right", new Vector3(5.5f, 3f, -5.2f),
            Gold, 1.0f, 7f);
    }

    private static void Point(Transform parent, string name, Vector3 position,
        Color color, float intensity, float range)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        item.transform.position = position;
        Light light = item.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }
    private static void Ring(Transform parent, string name, Vector3 center,
        float radius, Color color, int segments, bool horizontal)
    {
        Transform ring = Group(parent, name);
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float nextAngle = (i + 1) * Mathf.PI * 2f / segments;
            Vector3 start = horizontal
                ? center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius)
                : center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Vector3 end = horizontal
                ? center + new Vector3(Mathf.Cos(nextAngle) * radius, 0f, Mathf.Sin(nextAngle) * radius)
                : center + new Vector3(Mathf.Cos(nextAngle) * radius, Mathf.Sin(nextAngle) * radius, 0f);
            Beam(ring, "Segment_" + i, start, end, 0.045f, color, 1.8f);
        }
    }

    private static GameObject Beam(Transform parent, string name, Vector3 start,
        Vector3 end, float width, Color color, float glow = 0f)
    {
        Vector3 delta = end - start;
        return Box(parent, name, (start + end) * 0.5f,
            new Vector3(width, delta.magnitude, width),
            color, glow, Quaternion.FromToRotation(Vector3.up, delta));
    }

    private static GameObject Box(Transform parent, string name, Vector3 position,
        Vector3 scale, Color color, float glow = 0f, Quaternion? rotation = null)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.position = position;
        item.transform.rotation = rotation ?? Quaternion.identity;
        item.transform.localScale = scale;
        Collider collider = item.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        item.GetComponent<Renderer>().sharedMaterial = GetMaterial(color, glow);
        return item;
    }

    private static Material GetMaterial(Color color, float glow)
    {
        string key = ColorUtility.ToHtmlStringRGBA(color) + "_" + glow.ToString("F2");
        Material material;
        if (Materials.TryGetValue(key, out material) && material != null) return material;
        material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.SetFloat("_Glossiness", glow > 0f ? 0.28f : 0.13f);
        if (glow > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * glow);
        }
        Materials[key] = material;
        return material;
    }

    private static Transform Group(Transform parent, string name)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static Color Hex(string value)
    {
        Color color;
        ColorUtility.TryParseHtmlString("#" + value, out color);
        return color;
    }
}
