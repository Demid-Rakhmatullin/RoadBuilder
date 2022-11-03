using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public float topY;
    public float bottomY;
    private float yDelta;

    public GameObject SegmentA;
    public GameObject SegmentB;

    public GameObject travelerObj;

    public Transform HolderLeft;
    private Vector2 holderLeftStartPos;

    RoadSegmentPresenter segmA;
    RoadSegmentPresenter segmB;

    // Start is called before the first frame update
    void Start()
    {
        yDelta = topY - bottomY;
        holderLeftStartPos = HolderLeft.transform.position;
        //Debug.Log(SegmentA.transform.position.x);
        segmA = Instantiate(SegmentA, HolderLeft).GetComponent<RoadSegmentPresenter>();
        segmA.transform.localPosition = new Vector2(0, topY);
        //Debug.Log(segmA.transform.position.x);
        var traveler = Instantiate(travelerObj, HolderLeft).GetComponent<LineMover>();
        traveler.SetRoutes(segmA.routes);
        traveler.StartTravel();

        Observable.Timer(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
                {
                    segmA.HideEdge();
                    segmB = Instantiate(SegmentB, HolderLeft).GetComponent<RoadSegmentPresenter>();
                    segmB.transform.localPosition = new Vector2(0, bottomY);
                    traveler.SetRoutes(segmB.routes);                    
                })
            .AddTo(this);

        traveler.NextSegment
            .Where(ns => ns == true)
            .Subscribe(_ =>
                {
                    LeanTween.moveY(HolderLeft.gameObject, HolderLeft.position.y + yDelta, 0.3f)
                    .setOnComplete(() => 
                        {
                            Destroy(segmA.gameObject);
                            HolderLeft.position = holderLeftStartPos;
                            segmB.transform.localPosition = new Vector2(0, topY);
                            traveler.StartTravel();
                        });

                    //LeanTween.moveY(segmA.gameObject, topCoord.y + yDelta, 0.5f).setDestroyOnComplete(true);
                    //LeanTween.moveY(segmB.gameObject, topCoord.y, 0.5f);
                });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
