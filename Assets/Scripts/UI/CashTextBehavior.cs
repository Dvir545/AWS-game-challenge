using TMPro;
using UnityEngine;
using Utils;
using RectTransform = UnityEngine.RectTransform;

namespace UI
{
    public class CashTextBehavior : MonoBehaviour
    {
        private TextMeshProUGUI _cashText;
        [SerializeField] private RectTransform parentRectTransform;
        private int _digitWidth;
        
        void Start()
        {
            _cashText = GetComponent<TextMeshProUGUI>();
            EventManager.Instance.StartListening(EventManager.CashChanged, OnCashChanged);
            _digitWidth = (int)(4*_cashText.fontSize/5);
            SetText(Constants.StartingCash);
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
    }
}
