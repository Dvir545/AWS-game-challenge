using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;

namespace UI.GameUI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject darkOverlay;
        [SerializeField] private GameObject window;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private GameObject scoreboard;
        private ScoreboardBehaviour _scoreboardBehaviour;
        private Tween _tween;
    
        private void Awake()
        {
            darkOverlay.SetActive(false);
            window.SetActive(false);
            gameOverText.color = new Color(1, 1, 1, 0);
            _scoreboardBehaviour = scoreboard.GetComponent<ScoreboardBehaviour>();
        
            EventManager.Instance.StartListening(EventManager.PlayerDied, ShowScoreboard);
        }
        
        private void ShowScoreboard(object arg0)
        {
            StartCoroutine(Show(arg0));
        }

        private IEnumerator Show(object arg0)
        {
            darkOverlay.SetActive(true);
            window.SetActive(true);
            gameOverText.text = "GAME OVER\n\nyou survived " + GameData.Instance.day + " days";
    
            // Start both operations
            bool tweenComplete = false;
            _tween = gameOverText.DOColor(new Color(1, 1, 1, 1), 5f).OnComplete(() => {
                tweenComplete = true;
                _tween = null;
            });

            // Create a coroutine for the scoreboard operations
            Coroutine scoreboardCoroutine = StartCoroutine(SetScoreboardRoutine());

            // Wait for both operations to complete
            yield return new WaitUntil(() => tweenComplete);
            yield return scoreboardCoroutine;
            darkOverlay.SetActive(false);
            window.SetActive(false);
            gameOverText.color = new Color(1, 1, 1, 0);
            _scoreboardBehaviour.ShowScoreboard(false);
        }

        private IEnumerator SetScoreboardRoutine()
        {
            yield return StartCoroutine(_scoreboardBehaviour.SetPlayerScore(
                GameStatistics.Instance.username, 
                GameData.Instance.day, 
                GameData.Instance.secondsSinceGameStarted, 
                fromMenu: false
            ));
        }


    }
}
