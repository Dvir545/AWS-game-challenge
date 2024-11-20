using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject darkOverlay;
    [SerializeField] private GameObject window;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject restartButton;
    
    private void Awake()
    {
        darkOverlay.SetActive(false);
        window.SetActive(false);
        gameOverText.color = new Color(1, 1, 1, 0);
        restartButton.SetActive(false);
        
        EventManager.Instance.StartListening(EventManager.PlayerDied, Show);
    }

    private void Show(object arg0)
    {
        darkOverlay.SetActive(true);
        window.SetActive(true);
        gameOverText.DOColor(new Color(1, 1, 1, 1), 3f).OnComplete(() =>
        {
            restartButton.SetActive(true);
        });
    }
}
