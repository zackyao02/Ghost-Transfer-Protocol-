using System.Collections;
using UnityEngine;

public sealed class CameraShake : MonoBehaviour
{
    private Vector3 baseLocalPosition;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition;
    }

    public void Play(float duration, float strength)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
        }

        shakeRoutine = StartCoroutine(Shake(duration, strength));
    }

    private IEnumerator Shake(float duration, float strength)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localPosition = baseLocalPosition + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.localPosition = baseLocalPosition;
        shakeRoutine = null;
    }
}
