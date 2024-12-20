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
    
        private void Awake()
        {
            darkOverlay.SetActive(false);
            window.SetActive(false);
            gameOverText.color = new Color(1, 1, 1, 0);
            _scoreboardBehaviour = scoreboard.GetComponent<ScoreboardBehaviour>();
        
            EventManager.Instance.StartListening(EventManager.PlayerDied, Show);
        }

        private void Show(object arg0)
        {
            darkOverlay.SetActive(true);
            window.SetActive(true);
            gameOverText.text = "GAME OVER\n\nyou survived " + GameData.Instance.day + " days";
            gameOverText.DOColor(new Color(1, 1, 1, 1), 5f).OnComplete(() =>
            {
                _scoreboardBehaviour.SetPlayerScore("Player", GameData.Instance.day, GameData.Instance.secondsSinceGameStarted);
                _scoreboardBehaviour.RefreshScores(gameOverText, darkOverlay, window);
            });
        }
    }
}
