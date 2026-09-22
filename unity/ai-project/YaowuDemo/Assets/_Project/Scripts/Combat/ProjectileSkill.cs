using UnityEngine;

public sealed class ProjectileSkill : MonoBehaviour
{
    private CameraShake cameraShake;
    private Vector3 direction;
    private float speed;
    private float lifeTime;
    private int damage;

    public void Initialize(Vector3 forward, float moveSpeed, float duration, int hitDamage, CameraShake shake)
    {
        direction = forward.normalized;
        speed = moveSpeed;
        lifeTime = duration;
        damage = hitDamage;
        cameraShake = shake;
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, step + 0.6f))
        {
            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.ApplyDamage(damage, SkillType.SwordQi);
                if (cameraShake != null)
                {
                    cameraShake.Play(0.12f, 0.08f);
                }
                Destroy(gameObject);
                return;
            }
        }

        transform.position += direction * step;
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
