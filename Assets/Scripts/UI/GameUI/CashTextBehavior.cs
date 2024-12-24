using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;
using RectTransform = UnityEngine.RectTransform;

namespace UI.GameUI
{
    public class CashTextBehavior : MonoBehaviour
    {
        private TextMeshProUGUI _cashText;
        [SerializeField] private RectTransform parentRectTransform;
        [SerializeField] private Transform coinIcon;  // for tweening
        private int _digitWidth;
        private float _coinIconScale;

        private void Awake()
        {
            _cashText = GetComponent<TextMeshProUGUI>();
            EventManager.Instance.StartListening(EventManager.CashChanged, OnCashChanged);
            _digitWidth = (int)(4*_cashText.fontSize/5);
            _coinIconScale = coinIcon.localScale.x;
            Init();
        }

        private void SetText(int amount)
        {
            _cashText.text = amount.ToString();
            parentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _cashText.text.Length * _digitWidth);
        }

        private void OnCashChanged(object arg0)
        {
            Debug.Log(arg0);
            if (arg0 is int cash)
            {
                SetText(cash); 
                coinIcon.DOScale(_coinIconScale*1.3f, 0.2f).OnComplete(() => coinIcon.DOScale(_coinIconScale, 0.2f));
            }
        }

        public void Init()
        {
            if (_cashText == null)
            {
                _cashText = GetComponent<TextMeshProUGUI>();
            }

            SetText(GameData.Instance.cash);
        }
    }
}
