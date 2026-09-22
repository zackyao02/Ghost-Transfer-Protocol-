using UnityEngine;

// Lightweight built-in-pipeline grade for the shrine camera; no package dependency.
[RequireComponent(typeof(Camera))]
public sealed class ShrineCameraGrade : MonoBehaviour
{
    private Material gradeMaterial;
    private Color actTint = Color.white;
    private float exposure = 1.19f;

    public static void Attach(Camera cameraRef)
    {
        if (cameraRef != null && cameraRef.GetComponent<ShrineCameraGrade>() == null)
            cameraRef.gameObject.AddComponent<ShrineCameraGrade>();
    }

    public void SetAct(int act)
    {
        switch (act)
        {
            case 2:
                actTint = new Color(1.05f, 0.96f, 0.96f);
                exposure = 1.17f;
                break;
            case 3:
                actTint = new Color(0.96f, 1.04f, 1.05f);
                exposure = 1.19f;
                break;
            case 4:
                actTint = new Color(1.08f, 0.91f, 0.91f);
                exposure = 1.20f;
                break;
            case 5:
                actTint = new Color(1.07f, 1.04f, 0.96f);
                exposure = 1.20f;
                break;
            default:
                actTint = new Color(0.98f, 1.02f, 1.05f);
                exposure = 1.19f;
                break;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (gradeMaterial == null)
        {
            Shader shader = Resources.Load<Shader>("ShrineColorGrade");
            if (shader == null || !shader.isSupported)
            {
                Graphics.Blit(source, destination);
                return;
            }
            gradeMaterial = new Material(shader);
            gradeMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        gradeMaterial.SetColor("_ActTint", actTint);
        gradeMaterial.SetFloat("_Exposure", exposure);
        Graphics.Blit(source, destination, gradeMaterial);
    }

    private void OnDestroy()
    {
        if (gradeMaterial != null)
        {
            if (Application.isPlaying) Destroy(gradeMaterial);
            else DestroyImmediate(gradeMaterial);
        }
    }
}
