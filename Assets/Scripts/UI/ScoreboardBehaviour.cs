using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject scorePrefab;
    [SerializeField] private GameObject scoresParent;
    [SerializeField] private GameObject playerScore;
    private const float StartY = 92.6f;
    private const float YOffsetBetweenScores = 65;

    private int _nScores = 0;

    private void AddScore(string playerName, int daysSurvived, float secondsPlayed)
    {  // this should be used to load high scores from aws
        var score = Instantiate(scorePrefab, scoresParent.transform);
        SetScore(score, playerName, daysSurvived, secondsPlayed);
        score.transform.localPosition = new Vector3(0, StartY - YOffsetBetweenScores * _nScores, 0);
        _nScores++;
    }

    private void SetPlayerScore(string playerName, int daysSurvived, float secondsPlayed)
    {  // this should be used on your current score when losing
        SetScore(playerScore, playerName, daysSurvived, secondsPlayed);
    }

    private void SetScore(GameObject score, string playerName, int daysSurvived, float secondsPlayed)
    {
        var nameText = score.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        nameText.text = playerName;
        var daysText = score.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        daysText.text = daysSurvived.ToString();
        var timeText = score.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        TimeSpan time = TimeSpan.FromSeconds(secondsPlayed);
        timeText.text = time.Hours > 0 
            ? $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}"
            : $"{time.Minutes:D2}:{time.Seconds:D2}";
    }

    private void Start()
    {
        AddScore("Player 1", 1, 100);
        AddScore("Player 2", 2, 200);
        AddScore("Player 3", 3, 300);
        AddScore("Player 4", 4, 400);
        AddScore("Player 5", 5, 500);
        AddScore("Player 6", 6, 600);
        AddScore("Player 7", 7, 700);
        AddScore("Player 8", 8, 800);
        AddScore("Player 9", 9, 900);
        AddScore("Player 10", 10, 1000);
        SetPlayerScore("Player 11", 11, 1100);
    }
}
