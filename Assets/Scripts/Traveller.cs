using System;
using System.Collections;
using UniRx;
using UnityEngine;

public class Traveller : MonoBehaviour
{
    [SerializeField] Transform[] routes;
    [SerializeField] float speed;

    private float t;
    private Vector2 hiddenPos = new Vector3(-100, -100, -100);
    private int routeToGo;
    private bool coroutineAllowed;
    private IReadOnlyReactiveProperty<bool> paused;
    private Transform[] nextRoutes;

    private readonly Subject<bool> onNextSegment = new Subject<bool>();
    public IObservable<bool> OnNextSegment { get => onNextSegment; }

    void Start()
    {
        paused = GameController.CurrentState
            .Select(s => s == GameState.Paused)
            .ToReactiveProperty();
    }

    void Update()
    {
        if (coroutineAllowed && !paused.Value)
            StartCoroutine(Move(routeToGo));
    }

    public void StartTravel()
    {
        if (routes == null || routes.Length == 0)
        {
            routes = nextRoutes;
            nextRoutes = null;
        }

        transform.position = hiddenPos;
        coroutineAllowed = true;
    }

    public void SetNextRoutes(Transform[] routes)
    { 
        nextRoutes = routes != null ? (Transform[])routes.Clone() : null;
    }

    private IEnumerator Move(int routeNum)
    {
        coroutineAllowed = false;

        var p0 = routes[routeNum].GetChild(0).position;
        var p1 = routes[routeNum].GetChild(1).position;
        var p2 = routes[routeNum].GetChild(2).position;
        var p3 = routes[routeNum].GetChild(3).position;

        while (t < 1)
        {
            if (!paused.Value)
            {
                t += Time.deltaTime * speed;

                var currentPos = Mathf.Pow(1 - t, 3) * p0 +
                    3 * Mathf.Pow(1 - t, 2) * t * p1 +
                    3 * (1 - t) * Mathf.Pow(t, 2) * p2 +
                    Mathf.Pow(t, 3) * p3;

                transform.position = currentPos;
            }
            yield return new WaitForEndOfFrame();
        }

        t = 0f;

        routeToGo++;
        if (routeToGo > routes.Length - 1)
        {
            if (nextRoutes != null && nextRoutes.Length > 0)
            {
                routes = nextRoutes;
                routeToGo = 0;
                nextRoutes = null;                
                onNextSegment.OnNext(true);
            }
            else
            {
                Destroy(gameObject);
                onNextSegment.OnNext(false);
            }
        }
        else
            coroutineAllowed = true;
    }
}
