using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using World;

namespace UI.GameUI
{
    public class MinimapBehaviour : Singleton<MinimapBehaviour>
    {
        private Transform _map;
        private Transform _mapOpenButton;

        private RectTransform _ref1UI;
        private RectTransform _ref2UI;
        private Image _darkMask; 
        private RectTransform _playerUI;
        [SerializeField] private Transform ref1;
        [SerializeField] private Transform ref2;
        [SerializeField] private Transform player;

        private Tween _darkenTween;
        private Tween _lightenTween;
        private const float NightAlpha = 0.9f;
        private bool _mapOpen = false;
        
        private void Awake()
        {
            _map = transform.GetChild(0);
            _mapOpenButton = transform.GetChild(1);
            
            _ref1UI = _map.GetChild(0).GetComponent<RectTransform>();
            _ref2UI = _map.GetChild(1).GetComponent<RectTransform>();
            _darkMask = _map.GetChild(2).GetComponent<Image>();
            _playerUI = _map.GetChild(3).GetComponent<RectTransform>();
            _darkMask.color = new Color(0, 0, 0, 0);
        }
        
        private void Update()
        {
            // Get the world space positions
            Vector2 worldRef1 = ref1.position;
            Vector2 worldRef2 = ref2.position;
            Vector2 worldPlayer = player.position;

            // Calculate the scale factor between world space and minimap space
            float worldDistance = Vector2.Distance(worldRef1, worldRef2);
            float minimapDistance = Vector2.Distance(_ref1UI.anchoredPosition, _ref2UI.anchoredPosition);
            float scaleFactor = minimapDistance / worldDistance;

            // Calculate the relative position of the player from ref1
            Vector2 relativePos = worldPlayer - worldRef1;

            // Convert the relative position to minimap space
            var anchoredPosition = _ref1UI.anchoredPosition;
            Vector2 minimapPosition = new Vector2(
                anchoredPosition.x + (relativePos.x * scaleFactor),
                anchoredPosition.y + (relativePos.y * scaleFactor)
            );

            // Update the player UI position on the minimap
            _playerUI.anchoredPosition = minimapPosition;
        }

    
        public void OpenMap()
        {
            SoundManager.Instance.ShortButton();
            _map.gameObject.SetActive(true);
            _mapOpenButton.gameObject.SetActive(false);
            _mapOpen = true;
        }
    
        public void CloseMap()
        {
            SoundManager.Instance.ShortButton();
            _map.gameObject.SetActive(false);
            _mapOpenButton.gameObject.SetActive(true);
            _mapOpen = false;
        }
        
        public void ToggleMap()
        {
            if (_mapOpen)
            {
                CloseMap();
            }
            else
            {
                OpenMap();
            }
        }

        public void DarkenMap(float duration)
        {
            if (_darkenTween != null)
                return;
            _darkenTween = _darkMask.DOFade(NightAlpha, duration);
        }

        public void LightenMap(float duration)
        {
            if (_lightenTween != null)
                return;
            _lightenTween = _darkMask.DOFade(0, duration);
        }

        public void JumpToNight()
        {
            StopTweens();
            SetDarkness(NightAlpha);
        }

        private void SetDarkness(float percent)
        {
            _darkMask.color = new Color(0, 0, 0, percent);
        }

        public void StopTweens()
        {
            if (_darkenTween != null)
            {
                _darkenTween.Kill();
                _darkenTween = null;
            }
            if (_lightenTween != null)
            {
                _lightenTween.Kill();
                _lightenTween = null;
            }
        }

        public void Reset()
        {
            _darkMask.color = new Color(0, 0, 0, 0);
            _map.gameObject.SetActive(false);
            _mapOpenButton.gameObject.SetActive(true);
        }
    }
}
