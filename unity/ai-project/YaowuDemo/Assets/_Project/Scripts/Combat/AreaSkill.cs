using UnityEngine;

public sealed class AreaSkill : MonoBehaviour
{
    private CameraShake cameraShake;
    private float duration;
    private float maxRadius;
    private int damage;
    private bool hasDamaged;

    public void Initialize(float effectDuration, float radius, int hitDamage, CameraShake shake)
    {
        duration = effectDuration;
        maxRadius = radius;
        damage = hitDamage;
        cameraShake = shake;
        transform.localScale = Vector3.one * 0.2f;
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(duration / 0.45f);
        float scale = Mathf.Lerp(0.2f, maxRadius * 2f, t);
        transform.localScale = new Vector3(scale, 0.15f, scale);

        if (!hasDamaged)
        {
            hasDamaged = true;
            Collider[] hits = Physics.OverlapSphere(transform.position, maxRadius);
            for (int index = 0; index < hits.Length; index++)
            {
                EnemyHealth enemy = hits[index].GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.ApplyDamage(damage, SkillType.FireTalisman);
                }
            }

            if (cameraShake != null)
            {
                cameraShake.Play(0.15f, 0.12f);
            }
        }

        if (duration <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
