using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RoadSegmentPresenter : MonoBehaviour
{
    public Transform[] routes;

    public SpriteRenderer bottomEdge;

    void Start()
    {
        bottomEdge.enabled = true;
    }

    public void HideEdge()
        => bottomEdge.enabled = false;
}

