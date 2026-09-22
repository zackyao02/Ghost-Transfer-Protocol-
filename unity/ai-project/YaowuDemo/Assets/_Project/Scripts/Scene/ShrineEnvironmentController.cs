using UnityEngine;

public enum SceneEndingVisual
{
    None,
    Seal,
    Coexist,
    Transfer
}

public sealed class ShrineEnvironmentController : MonoBehaviour
{
    private Light altarLight;
    private Light bossLight;
    private ParticleSystem rain;
    private GameObject sealScar;
    private GameObject coexistScar;
    private GameObject transferScar;

    public int ActiveAct { get; private set; } = 1;
    public SceneEndingVisual ActiveEnding { get; private set; }

    private void OnEnable()
    {
        SimpleEventBus.SkillCast += HandleSkillCast;
        SimpleEventBus.BossSpawned += HandleBossSpawned;
        SimpleEventBus.DeitySummonStarted += HandleDeitySummonStarted;
        SimpleEventBus.DemoCompleted += HandleDemoCompleted;
    }

    private void OnDisable()
    {
        SimpleEventBus.SkillCast -= HandleSkillCast;
        SimpleEventBus.BossSpawned -= HandleBossSpawned;
        SimpleEventBus.DeitySummonStarted -= HandleDeitySummonStarted;
        SimpleEventBus.DemoCompleted -= HandleDemoCompleted;
    }

    private void HandleSkillCast(SkillType skill)
    {
        if (ActiveAct == 1) ApplyActVisual(2);
    }

    private void HandleBossSpawned(EnemyHealth boss)
    {
        ApplyActVisual(4);
    }

    private void HandleDeitySummonStarted()
    {
        ApplyActVisual(5);
    }

    private void HandleDemoCompleted()
    {
        ApplyActVisual(5);
    }
    public void Initialize(Light altar, Light boss, ParticleSystem rainSystem, GameObject seal, GameObject coexist, GameObject transfer)
    {
        altarLight = altar;
        bossLight = boss;
        rain = rainSystem;
        sealScar = seal;
        coexistScar = coexist;
        transferScar = transfer;
        ApplyActVisual(1);
        ApplyEndingVisual(SceneEndingVisual.None);
    }

    public void ApplyActVisual(int actIndex)
    {
        ActiveAct = Mathf.Clamp(actIndex, 1, 5);
        Color fog;
        Color altar;
        float density;
        float altarPower;
        float bossPower;

        switch (ActiveAct)
        {
            case 1:
                fog = new Color(0.025f, 0.04f, 0.055f);
                altar = new Color(0.2f, 0.8f, 1f);
                density = 0.04f;
                altarPower = 1.2f;
                bossPower = 0.85f;
                break;
            case 2:
                fog = new Color(0.08f, 0.04f, 0.045f);
                altar = new Color(1f, 0.3f, 0.12f);
                density = 0.032f;
                altarPower = 1.8f;
                bossPower = 1.15f;
                break;
            case 3:
                fog = new Color(0.035f, 0.07f, 0.08f);
                altar = new Color(0.15f, 0.85f, 0.85f);
                density = 0.026f;
                altarPower = 2.3f;
                bossPower = 0.7f;
                break;
            case 4:
                fog = new Color(0.12f, 0.025f, 0.025f);
                altar = new Color(1f, 0.12f, 0.06f);
                density = 0.038f;
                altarPower = 1.5f;
                bossPower = 3.2f;
                break;
            default:
                fog = new Color(0.045f, 0.055f, 0.085f);
                altar = new Color(1f, 0.82f, 0.3f);
                density = 0.02f;
                altarPower = 4f;
                bossPower = 1.2f;
                break;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ActiveAct == 4
            ? new Color(0.33f, 0.27f, 0.32f)
            : new Color(0.38f, 0.45f, 0.51f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fog;
        RenderSettings.fogDensity = density;

        ShrineSceneArt.ApplyAct(transform, ActiveAct);
        Camera sceneCamera = Camera.main;
        if (sceneCamera != null)
        {
            ShrineCameraGrade grade = sceneCamera.GetComponent<ShrineCameraGrade>();
            if (grade != null) grade.SetAct(ActiveAct);
        }

        if (altarLight != null)
        {
            altarLight.color = altar;
            altarLight.intensity = altarPower;
        }
        if (bossLight != null)
        {
            bossLight.intensity = bossPower;
        }
        if (rain != null)
        {
            ParticleSystem.EmissionModule emission = rain.emission;
            emission.rateOverTime = ActiveAct == 5 ? 280f : 650f;
        }
    }

    public void ApplyEndingVisual(SceneEndingVisual ending)
    {
        ActiveEnding = ending;
        SetActive(sealScar, ending == SceneEndingVisual.Seal);
        SetActive(coexistScar, ending == SceneEndingVisual.Coexist);
        SetActive(transferScar, ending == SceneEndingVisual.Transfer);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}