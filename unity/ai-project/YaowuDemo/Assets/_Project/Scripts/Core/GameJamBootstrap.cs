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

        CyberShrinePresentation.CreateOrReplace(playerRoot.transform.position);
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

}
