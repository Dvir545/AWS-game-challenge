using System;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace UI.GameUI
{
    public class DayNightRollBehaviour: Singleton<DayNightRollBehaviour>
    {
        private RectTransform _rectTransform;
        private float CurX => _rectTransform.anchoredPosition.x;
        private const float DayStartX = 54f;  // full light
        private const float DayEndX = -56;  // start darkening
        private const float NightStartX = -85;  // full dark
        private const float NightEndX = 86;  // start lightening
        private const float NightEndX2 = -172;
        
        private float DayXWidth => Mathf.Abs(DayEndX - DayStartX);
        private float DayEndXWidth => Mathf.Abs(NightStartX - DayEndX);
        private float NightXWidth => Mathf.Abs(NightEndX2 - NightStartX);
        private float NightEndXWidth => DayEndXWidth;

        private DayPhase _nextDayPhase;
        private float _rollProgressWidth;
        private float _rollCurProgress;
        
        private void SetX(float x)
        {
            _rectTransform.anchoredPosition = new Vector2(x, _rectTransform.anchoredPosition.y);
        }
        
        private void Awake()
        {
            Init();
        }

        private void SetRollProgressWidth()
        {
            switch (_nextDayPhase)
            {
                case DayPhase.Day:
                    _rollProgressWidth = DayXWidth;
                    SetX(DayStartX);
                    _nextDayPhase = DayPhase.DayEnd;
                    break;
                case DayPhase.DayEnd:
                    _rollProgressWidth = DayEndXWidth;
                    SetX(DayEndX);
                    _nextDayPhase = DayPhase.Night;
                    break;
                case DayPhase.Night:
                    _rollProgressWidth = NightXWidth;
                    SetX(NightStartX);
                    _nextDayPhase = DayPhase.NightEnd;
                    break;
                case DayPhase.NightEnd:
                    _rollProgressWidth = NightEndXWidth;
                    SetX(NightEndX);
                    _nextDayPhase = DayPhase.Day;
                    break;
            }
            _rollCurProgress = 0;
        }

        public void AddProgress(float progress)
        {
            var xOffset = progress * _rollProgressWidth;
            if (_rollCurProgress + xOffset > _rollProgressWidth)
            {
                xOffset = _rollProgressWidth - _rollCurProgress;
            }
            _rollCurProgress += xOffset;
            SetX(CurX - xOffset);
            if (CurX + _rectTransform.rect.width <= NightEndX)
            {
                SetX(CurX + _rectTransform.rect.width);
            }

            if (_rollCurProgress >= _rollProgressWidth)
            {
                SetRollProgressWidth();
            }

        }

        public void JumpToNight()
        {
            DOTween.To(() => _rectTransform.anchoredPosition, 
                v => _rectTransform.anchoredPosition = v, 
                new Vector2(NightStartX, _rectTransform.anchoredPosition.y), 
                1f).SetEase(Ease.OutExpo).OnComplete(
                () =>
                {
                    _nextDayPhase = DayPhase.Night;
                    SetRollProgressWidth();
                });
        }

        public void Init()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            _nextDayPhase = DayPhase.Day;
            SetRollProgressWidth();
        }
    }
}