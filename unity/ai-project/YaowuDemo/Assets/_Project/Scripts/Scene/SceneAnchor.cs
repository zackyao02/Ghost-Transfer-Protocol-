using UnityEngine;

public enum SceneAnchorType
{
    Camera,
    StoneSpawn,
    BossSpawn,
    DeityReveal,
    EndingFocus
}

public sealed class SceneAnchor : MonoBehaviour
{
    [SerializeField] private SceneAnchorType anchorType;
    [SerializeField] private int actIndex;
    [SerializeField] private string anchorId;

    public SceneAnchorType AnchorType => anchorType;
    public int ActIndex => actIndex;
    public string AnchorId => anchorId;

    public void Configure(SceneAnchorType type, int act, string id)
    {
        anchorType = type;
        actIndex = act;
        anchorId = id;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = anchorType == SceneAnchorType.Camera ? Color.cyan :
            anchorType == SceneAnchorType.BossSpawn ? Color.red :
            anchorType == SceneAnchorType.DeityReveal ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, anchorType == SceneAnchorType.Camera ? 0.45f : 0.7f);
        if (anchorType == SceneAnchorType.Camera)
        {
            Gizmos.DrawRay(transform.position, transform.forward * 2.5f);
        }
    }
}