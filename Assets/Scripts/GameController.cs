using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameController : MonoBehaviour
{
    [SerializeField] Map[] maps;
    [SerializeField] Text scoreCounter;

    private readonly IReactiveProperty<int> currentScore = new ReactiveProperty<int>();
    private IReadOnlyReactiveProperty<bool> gameWin;

    void Start()
    {
        currentScore.SubscribeToText(scoreCounter);

        foreach (var map in maps)
            map.OnScoreChange.Subscribe(scoreDelta => currentScore.Value += scoreDelta);

        gameWin = maps.Select(m => m.IsLose).Aggregate((w1, w2) => new ReactiveProperty<bool>(w1.Value && w2.Value)).Where(w => w == true).ToReactiveProperty();
        gameWin.Subscribe(_ => Debug.Log("lose"));

    }
}

