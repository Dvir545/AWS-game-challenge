using TMPro;
using UnityEngine;
using Utils.Data;

namespace UI.Settings
{
    public class RightLeftHandedBehaviour : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Transform _button;

        private void Awake()
        {
            _text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _button = transform.GetChild(1).GetChild(0);
            ToggleLeftHanded(GameStatistics.Instance.leftHanded);
        }

        private void ToggleLeftHanded(bool v)
        {
            GameStatistics.Instance.leftHanded = v;
            var bPos = _button.localPosition;
            var tPos = _text.transform.localPosition;
            if (v)  // left handed
            {
                _button.localPosition = new Vector3(-Mathf.Abs(bPos.x), bPos.y, bPos.z);
                _text.text = "LEFT HANDED";
                _text.transform.localPosition = new Vector3(-Mathf.Abs(tPos.x), tPos.y, tPos.z);
            }
            else  // right handed
            {
                _button.localPosition = new Vector3(Mathf.Abs(bPos.x), bPos.y, bPos.z);
                _text.text = "RIGHT HANDED";
                _text.transform.localPosition = new Vector3(Mathf.Abs(tPos.x), tPos.y, tPos.z);
            }
        }

        public void OnClick()
        {
            ToggleLeftHanded(!GameStatistics.Instance.leftHanded);
        }
    }
}
