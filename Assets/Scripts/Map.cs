using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [SerializeField] int minCompleteCount;
    [SerializeField] int maxCompleteCount;

    [SerializeField] float topY;
    [SerializeField] float bottomY;
    [SerializeField] GameObject startSegmentPrefab;
    [SerializeField] GameObject travelerPrefab;
    [SerializeField] Button[] nextSegmentButtons;
    [SerializeField] Text remainingSegmentsCounter;
    [SerializeField] Text winText;
    [SerializeField] Text loseText;

    private float yDelta;
    private Vector2 startPos;
    private bool isSwapping;
    private Traveller traveler;
    private int winSegmentsCount;
    private bool gameStarted;

    private RouteSegment currentSegment;
    private RouteSegment nextSegment;

    private readonly IReactiveProperty<int> reaminingSegmentsCount = new ReactiveProperty<int>();
    private readonly IReactiveProperty<bool> buttonsActive = new ReactiveProperty<bool>();

    private readonly Subject<int> onScoreChange = new Subject<int>();
    public IObservable<int> OnScoreChange { get => onScoreChange; }

    public IReadOnlyReactiveProperty<bool> IsWin;
    public IReactiveProperty<bool> IsLose = new ReactiveProperty<bool>();

    void Start()
    {
        
        reaminingSegmentsCount.SubscribeToText(remainingSegmentsCounter);
        IsWin = reaminingSegmentsCount
            .Where(_ => GameController.CurrentState.Value == GameState.Active)
            .Select(v => v == 0).ToReactiveProperty();
        IsWin.Where(win => win == true)
            .Subscribe(_ => ShowWin());        

        foreach (var btn in nextSegmentButtons)
        {
            btn.onClick.AsObservable()
                .Where(_ => !isSwapping)
                .Subscribe(_ =>
                    {
                        var nextSegmentPrefab = btn.GetComponent<NextSegmentBtn>().NextSegmentPrefab;
                        PlaceNextSegment(nextSegmentPrefab);
                    });

            buttonsActive.SubscribeToInteractable(btn);
        }      

        GameController.CurrentState.Where(s => s == GameState.Active && !gameStarted)
            .Subscribe(_ => StartGame());

        GameController.CurrentState.Where(s => s == GameState.Stopped)
            .Subscribe(_ => gameStarted = false);
    }
    
    private void StartGame()
    {
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);

        IsLose.Value = false;

        winSegmentsCount = UnityEngine.Random.Range(minCompleteCount, maxCompleteCount + 1);
        reaminingSegmentsCount.Value = winSegmentsCount;

        yDelta = topY - bottomY;
        startPos = transform.position;

        LeanTween.delayedCall(0.5f, () =>
        {
            currentSegment = Instantiate(startSegmentPrefab, transform).GetComponent<RouteSegment>();
            currentSegment.transform.localPosition = new Vector2(0, topY);

            traveler = Instantiate(travelerPrefab, transform).GetComponent<Traveller>();
            traveler.SetNextRoutes(currentSegment.Routes);
            traveler.StartTravel();

            traveler.OnNextSegment
               .Where(ns => ns == true)
               .Subscribe(_ =>
               {
                   SwapSegments();
                   onScoreChange.OnNext(100);
               });

            traveler.OnNextSegment
               .Where(ns => ns == false)
               .Subscribe(_ =>
               {
                   DisableMap(() =>
                   {
                       loseText.gameObject.SetActive(true);
                       IsLose.Value = true;
                   });
               });

            buttonsActive.Value = true;
            gameStarted = true;
        });
    }

    private void PlaceNextSegment(GameObject nextSegmentPrefab)
    {
        if (nextSegment != null)
        {
            Destroy(nextSegment.gameObject);
            traveler.SetNextRoutes(null);
        }

        currentSegment.HideEdge();
        nextSegment = Instantiate(nextSegmentPrefab, transform).GetComponent<RouteSegment>();
        nextSegment.transform.localPosition = new Vector2(0, bottomY);

        if (currentSegment.ConnectionBottom == nextSegment.ConnectionTop)
            traveler.SetNextRoutes(nextSegment.Routes);
    }

    private void SwapSegments()
    {
        isSwapping = true;

        LeanTween
            .moveY(gameObject, transform.position.y + yDelta, 0.3f)
            .setOnComplete(() =>
                {
                    Destroy(currentSegment.gameObject);
                    if (nextSegment != null)
                    {
                        currentSegment = nextSegment;
                        nextSegment = null;
                        currentSegment.transform.localPosition = new Vector2(0, topY);
                        traveler.StartTravel();
                    }

                    reaminingSegmentsCount.Value--;
                    transform.position = startPos;
                    isSwapping = false;
                });
    }

    private void DisableMap(Action onComplete)
    {
        buttonsActive.Value = false;
        FadeSegment(currentSegment).setOnComplete(onComplete);
        if (nextSegment != null)
            FadeSegment(nextSegment);
    }    

    private LTDescr FadeSegment(RouteSegment segment)
        => LeanTween.alpha(segment.gameObject, 0f, 0.5f).setDestroyOnComplete(true);

    private void ShowWin()
    {
        Destroy(traveler.gameObject);
        DisableMap(() => winText.gameObject.SetActive(true));
    }
}
