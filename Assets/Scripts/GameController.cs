using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameController : MonoBehaviour
{
    public static IReactiveProperty<GameState> CurrentState { get; private set; } = new ReactiveProperty<GameState>();

    [SerializeField] CanvasGroup startUI;
    [SerializeField] Button startBtn;
    [SerializeField] Button pauseBtn;
    [SerializeField] Button continueBtn;
    [SerializeField] GameObject resultPanel;
    [SerializeField] Text resultScoreCounter;
    [SerializeField] GameObject winText;
    [SerializeField] GameObject loseText;    

    [SerializeField] Canvas gameUI;
    [SerializeField] Map[] maps;
    [SerializeField] Text scoreCounter;

    private readonly IReactiveProperty<int> currentScore = new ReactiveProperty<int>();
    private readonly IReactiveProperty<int> loseCounter = new ReactiveProperty<int>();
    private readonly IReactiveProperty<int> winCounter = new ReactiveProperty<int>();    

    void Start()
    {
        startUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
        startBtn.gameObject.SetActive(true);
        continueBtn.gameObject.SetActive(false);

        startBtn.OnClickAsObservable()
            .Subscribe(_ => 
            {
                Clear();
                StartGame(); 
            });

        pauseBtn.OnClickAsObservable()
            .Subscribe(_ => OnPauseOn());
        continueBtn.OnClickAsObservable()
            .Subscribe(_ => OnPauseOff());

        currentScore.SubscribeToText(scoreCounter);

        foreach (var map in maps)
        {
            map.OnScoreChange.Subscribe(scoreDelta => currentScore.Value += scoreDelta);
            map.IsLose.Where(l => l == true).Subscribe(_ => loseCounter.Value++);
            map.IsWin.Where(w => w == true).Subscribe(_ => winCounter.Value++);
        }

        loseCounter.Where(lc => lc == maps.Length)
            .Subscribe(_ => OnGameComplete(false));
        winCounter.Where(wc => wc > 0 && wc + loseCounter.Value == maps.Length)
            .Subscribe(_ => OnGameComplete(true));

    }

    private void StartGame()
    {
        resultPanel.SetActive(false);
        startBtn.gameObject.SetActive(false);
        continueBtn.gameObject.SetActive(false);

        LeanTween.alphaCanvas(startUI, 0f, 1f).setOnComplete(() =>
            {
                startUI.gameObject.SetActive(false);
                gameUI.gameObject.SetActive(true);

                CurrentState.Value = GameState.Active;
            });
    }

    private void Clear()
    {
        currentScore.Value = loseCounter.Value = winCounter.Value = 0;
    }

    private void OnGameComplete(bool isWin)
    {
        CurrentState.Value = GameState.Stopped;
        gameUI.gameObject.SetActive(false);
        startUI.gameObject.SetActive(true);

        LeanTween.alphaCanvas(startUI, 1f, 0.5f).setOnComplete(() =>
        {
            resultScoreCounter.text = currentScore.Value.ToString();
            winText.SetActive(isWin);
            loseText.SetActive(!isWin);
            resultPanel.SetActive(true);
            startBtn.gameObject.SetActive(true);
        });
    }

    private void OnPauseOn()
    {
        CurrentState.Value = GameState.Paused;
        gameUI.gameObject.SetActive(false);
        startUI.gameObject.SetActive(true);

        LeanTween.alphaCanvas(startUI, 1f, 0.5f).setOnComplete(() =>
        {
            continueBtn.gameObject.SetActive(true);
        });
    }

    private void OnPauseOff()
    {
        continueBtn.gameObject.SetActive(false);

        LeanTween.alphaCanvas(startUI, 0f, 1f).setOnComplete(() =>
        {
            startUI.gameObject.SetActive(false);
            gameUI.gameObject.SetActive(true);

            CurrentState.Value = GameState.Active;
        });
    }
}

