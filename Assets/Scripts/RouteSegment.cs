using UnityEngine;

public class RouteSegment : MonoBehaviour
{
    [SerializeField] Transform[] routes;
    [SerializeField] SpriteRenderer bottomEdge;

    [SerializeField] SegmentConnection connectionTop;
    [SerializeField] SegmentConnection connectionBottom;

    public Transform[] Routes => routes;
    public SegmentConnection ConnectionTop => connectionTop;
    public SegmentConnection ConnectionBottom => connectionBottom;

    void Start()
    {
        bottomEdge.enabled = true;
    }

    public void HideEdge()
        => bottomEdge.enabled = false;
}

