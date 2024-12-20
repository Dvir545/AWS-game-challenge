using System;
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
        private int _digitWidth;

        private void Awake()
        {
            _cashText = GetComponent<TextMeshProUGUI>();
            EventManager.Instance.StartListening(EventManager.CashChanged, OnCashChanged);
            _digitWidth = (int)(4*_cashText.fontSize/5);
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
