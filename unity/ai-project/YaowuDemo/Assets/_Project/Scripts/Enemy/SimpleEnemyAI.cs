using UnityEngine;

public sealed class SimpleEnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float stopDistance = 2.2f;

    private Transform target;

    public void Initialize(Transform targetTransform, float speed, float distance)
    {
        target = targetTransform;
        moveSpeed = speed;
        stopDistance = distance;
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        Vector3 offset = target.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;
        if (distance <= stopDistance)
        {
            return;
        }

        Vector3 direction = offset.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.forward = Vector3.Lerp(transform.forward, direction, Time.deltaTime * 6f);
    }
}
