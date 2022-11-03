using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class LineMover : MonoBehaviour
{
    //public SpriteShapeController con;
    public Transform[] routes;

    private float t;
    private Vector2 currentPos;
    private float speed;
    private int routeToGo;
    private bool coroutineAllowed;

    // Start is called before the first frame update
    void Start()
    {
        //var v = con.spline.GetPosition(0);
        //var v1 = con.spline.GetLeftTangent(0);
        //Debug.Log(v.x + " " + v1.x);

        speed = 0.5f;
        coroutineAllowed = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (coroutineAllowed)
            StartCoroutine(Move(routeToGo));
    }

    private IEnumerator Move(int routeNum)
    {
        coroutineAllowed = false;

        var p0 = routes[routeNum].GetChild(0).position;
        var p1 = routes[routeNum].GetChild(1).position;
        var p2 = routes[routeNum].GetChild(2).position;
        var p3 = routes[routeNum].GetChild(3).position;

        //var p0 = con.spline.GetPosition(0);
        //var p1 = con.spline.GetRightTangent(0);
        //var p2 = con.spline.GetLeftTangent(1);
        //var p3 = con.spline.GetPosition(1);

        while (t < 1)
        {
            t += Time.deltaTime * speed;

            currentPos = Mathf.Pow(1 - t, 3) * p0 +
                3 * Mathf.Pow(1 - t, 2) * t * p1 +
                3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                Mathf.Pow(t, 3) * p3;

            transform.position = currentPos;
            yield return new WaitForEndOfFrame();
        }

        t = 0f;

        routeToGo++;
        if (routeToGo > routes.Length - 1)
            routeToGo = 0;

        coroutineAllowed = true;
    }
}
