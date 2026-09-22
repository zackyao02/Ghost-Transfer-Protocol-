using System.Collections.Generic;
using UnityEngine;

public sealed class GestureTrailView : MonoBehaviour
{
    private readonly List<Vector3> worldPoints = new List<Vector3>();

    private Camera targetCamera;
    private LineRenderer lineRenderer;

    public void Initialize(Camera cameraRef)
    {
        targetCamera = cameraRef;

        GameObject lineObject = new GameObject("GestureTrail");
        lineObject.transform.SetParent(transform, false);
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.04f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        lineRenderer.numCapVertices = 4;
        lineRenderer.startColor = new Color(1f, 0.75f, 0.25f, 0.95f);
        lineRenderer.endColor = new Color(1f, 0.15f, 0.05f, 0.3f);
    }

    public void Begin()
    {
        worldPoints.Clear();
        lineRenderer.positionCount = 0;
        lineRenderer.enabled = true;
    }

    public void Append(Vector2 screenPoint)
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 worldPoint = targetCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 2.5f));

        if (worldPoints.Count > 0 && Vector3.Distance(worldPoints[worldPoints.Count - 1], worldPoint) < 0.02f)
        {
            return;
        }

        worldPoints.Add(worldPoint);
        lineRenderer.positionCount = worldPoints.Count;
        lineRenderer.SetPosition(worldPoints.Count - 1, worldPoint);
    }

    public void End()
    {
        CancelInvoke(nameof(Clear));
        Invoke(nameof(Clear), 0.4f);
    }

    private void Clear()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;
    }
}
