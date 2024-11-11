using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class CashTextBehavior : MonoBehaviour
    {
        private TextMeshProUGUI _cashText;
        
        void Start()
        {
            _cashText = GetComponent<TextMeshProUGUI>();
            _cashText.text = Constants.StartingCash + " $";
            EventManager.Instance.StartListening(EventManager.CashChanged, OnCashChanged);
        }

        private void OnCashChanged(object arg0)
        {
            Debug.Log(arg0);
            if (arg0 is int cash)
            {
                _cashText.text = cash + " $";
            }
        }
    }
}
