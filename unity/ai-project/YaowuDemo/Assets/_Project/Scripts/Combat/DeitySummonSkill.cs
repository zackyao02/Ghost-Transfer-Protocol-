using UnityEngine;

public sealed class DeitySummonSkill : MonoBehaviour
{
    private CameraShake cameraShake;
    private float duration;

    public void Initialize(float effectDuration, CameraShake shake)
    {
        duration = effectDuration;
        cameraShake = shake;
        transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
        float t = 1f - Mathf.Clamp01(duration / 1.1f);
        float scale = Mathf.Lerp(0.5f, 18f, t);
        transform.localScale = new Vector3(scale, 0.05f, scale);

        if (cameraShake != null)
        {
            cameraShake.Play(0.08f, 0.06f + t * 0.08f);
        }

        if (duration <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
