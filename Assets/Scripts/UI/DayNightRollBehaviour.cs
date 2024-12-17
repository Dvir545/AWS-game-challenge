using DG.Tweening;
using UnityEngine;
using Utils;
using World;

namespace UI
{
    public class DayNightRollBehaviour: Singleton<DayNightRollBehaviour>
    {
        private RectTransform _rectTransform;
        private float CurX => _rectTransform.anchoredPosition.x;
        private const float DayStartX = 54f;  // full light
        private const float DayEndX = -56;  // start darkening
        private const float NightStartX = -85;  // full dark
        private const float NightEndX = -172;  // start lightening
        
        private float DayStartX2NightStartX => DayStartX - NightStartX;  // 139
        private float NightEnd2DayStartX =>  _rectTransform.rect.width + NightEndX - DayStartX;  // 32
        private float FullDayXDuration => NightEnd2DayStartX + DayStartX2NightStartX;  // 171
        public float ChangeLightProgressDuration => (NightEnd2DayStartX) / FullDayXDuration;  // how much progress to darken \ lighten

        private bool _day1 = true;
        private bool _day = false;
        private float _rollProgressWidth;
        private float _rollCurProgress;
        
        private void SetX(float x)
        {
            _rectTransform.anchoredPosition = new Vector2(x, _rectTransform.anchoredPosition.y);
        }
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            SetRollProgressWidth();
        }

        private void SetRollProgressWidth()
        {
            if (_day1)
            {
                _rollProgressWidth = FullDayXDuration - NightEnd2DayStartX;
                SetX(DayStartX);
                _day1 = false;
            }
            else if (!_day)
            {
                _rollProgressWidth = _rectTransform.rect.width - FullDayXDuration;
                SetX(NightStartX);
                _day = true;
            }
            else
            {
                _rollProgressWidth = FullDayXDuration;
                SetX(NightEndX);
                _day = false;
            }

            _rollCurProgress = 0;
        }

        public void AddProgress(float progress)
        {
            var xOffset = progress * _rollProgressWidth;
            _rollCurProgress += xOffset;
            SetX(CurX - xOffset);
            if (CurX + _rectTransform.rect.width <= DayStartX)
            {
                SetX(CurX + _rectTransform.rect.width);
            }
            if (_rollCurProgress >= _rollProgressWidth)
                SetRollProgressWidth();
        }

        public void JumpToNight()
        {
            DOTween.To(() => _rectTransform.anchoredPosition, v => _rectTransform.anchoredPosition = v, new Vector2(NightStartX, _rectTransform.anchoredPosition.y), 1f).SetEase(Ease.OutExpo).OnComplete(SetRollProgressWidth);
        }

    }
}