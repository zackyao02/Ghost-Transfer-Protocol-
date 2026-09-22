using UnityEngine;

public sealed class GameJamBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<GameJamBootstrap>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject("GameJamBootstrap");
        bootstrapObject.AddComponent<GameJamBootstrap>();
    }

    private void Awake()
    {
        BuildDemo();
    }

    private void BuildDemo()
    {
        ConfigureAtmosphere();

        Camera cameraRef = EnsureCamera();
        CameraShake cameraShake = cameraRef.gameObject.GetComponent<CameraShake>();
        if (cameraShake == null)
        {
            cameraShake = cameraRef.gameObject.AddComponent<CameraShake>();
        }

        GameObject playerRoot = new GameObject("PlayerRoot");
        playerRoot.transform.position = new Vector3(0f, 1.2f, -12f);
        CharacterController controller = playerRoot.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        cameraRef.transform.SetParent(playerRoot.transform, false);
        cameraRef.transform.localPosition = new Vector3(0f, 0.75f, 0f);
        cameraRef.transform.localRotation = Quaternion.identity;

        GestureTrailView trailView = new GameObject("GestureTrailView").AddComponent<GestureTrailView>();
        trailView.Initialize(cameraRef);

        MouseGestureInput mouseInput = playerRoot.AddComponent<MouseGestureInput>();
        mouseInput.Initialize(trailView);

        KeyboardSkillInput keyboardInput = playerRoot.AddComponent<KeyboardSkillInput>();
        MediaPipeHandInput mediaPipeInput = playerRoot.AddComponent<MediaPipeHandInput>();

        WorldStateStore worldState = new GameObject("WorldStateStore").AddComponent<WorldStateStore>();
        worldState.Load();
        DirectorDebugPanel debugPanel = new GameObject("DirectorDebugPanel").AddComponent<DirectorDebugPanel>();
        debugPanel.Initialize(keyboardInput, mediaPipeInput, worldState);

        SkillManager skillManager = playerRoot.AddComponent<SkillManager>();
        skillManager.Initialize(cameraRef.transform, cameraRef, cameraShake);

        GestureInputRouter router = playerRoot.AddComponent<GestureInputRouter>();
        router.Initialize(skillManager, keyboardInput, mouseInput, mediaPipeInput);

        SimpleFirstPersonController firstPersonController = playerRoot.AddComponent<SimpleFirstPersonController>();
        firstPersonController.Initialize(cameraRef, mouseInput);

        EnemySpawner enemySpawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
        enemySpawner.Initialize(playerRoot.transform);

        DemoHUD hud = new GameObject("DemoHUD").AddComponent<DemoHUD>();
        hud.Initialize(skillManager);

        DemoFlowController flow = new GameObject("DemoFlowController").AddComponent<DemoFlowController>();
        flow.Initialize(enemySpawner, hud, skillManager, playerRoot.transform);
        flow.SetWorldState(worldState);

        BuildTempleEnvironment(playerRoot.transform.position);
        flow.StartDemo();
    }

    private static void ConfigureAtmosphere()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.1f, 0.14f, 0.16f);
        RenderSettings.fogDensity = 0.025f;
        RenderSettings.ambientLight = new Color(0.34f, 0.34f, 0.38f);
    }

    private static Camera EnsureCamera()
    {
        Camera cameraRef = Camera.main;
        if (cameraRef == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraRef = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<AudioListener>();
        }

        cameraRef.clearFlags = CameraClearFlags.SolidColor;
        cameraRef.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
        cameraRef.fieldOfView = 72f;
        return cameraRef;
    }

    private static void BuildTempleEnvironment(Vector3 playerSpawn)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "TempleGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        groundRenderer.material = new Material(Shader.Find("Standard"));
        groundRenderer.material.color = new Color(0.16f, 0.17f, 0.18f);

        CreateAltar(new Vector3(0f, 0.5f, 8f), new Vector3(8f, 1f, 8f), new Color(0.22f, 0.2f, 0.19f));
        CreateAltar(new Vector3(0f, 0.15f, 8f), new Vector3(6f, 0.15f, 6f), new Color(0.78f, 0.52f, 0.16f), true);
        CreateAltar(new Vector3(0f, 2.25f, 20f), new Vector3(5f, 4.5f, 1f), new Color(0.28f, 0.24f, 0.2f));
        CreateAltar(new Vector3(-3f, 2f, 20f), new Vector3(1f, 4f, 1f), new Color(0.25f, 0.2f, 0.18f));
        CreateAltar(new Vector3(3f, 2f, 20f), new Vector3(1f, 4f, 1f), new Color(0.25f, 0.2f, 0.18f));
        CreateAltar(new Vector3(0f, 1.2f, 19.6f), new Vector3(2f, 2.4f, 0.6f), new Color(0.62f, 0.58f, 0.42f));

        for (int index = 0; index < 10; index++)
        {
            float x = index < 5 ? -16f - index * 1.4f : 16f + (index - 5) * 1.4f;
            float z = 4f + (index % 5) * 6f;
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.position = new Vector3(x, 2f, z);
            trunk.transform.localScale = new Vector3(0.22f, 2f, 0.22f);
            Renderer trunkRenderer = trunk.GetComponent<Renderer>();
            trunkRenderer.material = new Material(Shader.Find("Standard"));
            trunkRenderer.material.color = new Color(0.14f, 0.1f, 0.08f);
        }

        Light altarLight = new GameObject("AltarLight").AddComponent<Light>();
        altarLight.type = LightType.Point;
        altarLight.range = 18f;
        altarLight.intensity = 2.2f;
        altarLight.color = new Color(1f, 0.75f, 0.36f);
        altarLight.transform.position = new Vector3(0f, 3.2f, 9f);

        GameObject hint = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hint.name = "PlayerSafeZone";
        hint.transform.position = playerSpawn + new Vector3(0f, -1.19f, 1.8f);
        hint.transform.localScale = new Vector3(2.2f, 0.02f, 2.2f);
        Renderer hintRenderer = hint.GetComponent<Renderer>();
        hintRenderer.material = new Material(Shader.Find("Standard"));
        hintRenderer.material.EnableKeyword("_EMISSION");
        hintRenderer.material.color = new Color(0.16f, 0.22f, 0.2f);
        hintRenderer.material.SetColor("_EmissionColor", new Color(0.1f, 0.35f, 0.28f));
    }

    private static void CreateAltar(Vector3 position, Vector3 scale, Color color, bool emissive = false)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = color;
        if (emissive)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", color * 0.75f);
        }
    }
}
