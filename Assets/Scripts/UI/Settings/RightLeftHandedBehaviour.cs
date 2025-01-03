using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;
using World;

namespace UI.Settings
{
    public class RightLeftHandedBehaviour : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Transform _button;
        [SerializeField] private RectTransform[] toFlip;
        private Vector2[] _posLeft;
        private bool _left = true;

        public void Init()
        {
            SetUI(GameStatistics.Instance.leftHanded);
        }

        private void Awake()
        {
            _text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _button = transform.GetChild(1).GetChild(0);
            ToggleLeftHanded(GameStatistics.Instance.leftHanded, true);
        }

        private void ToggleLeftHanded(bool v, bool firstTime=false)
        {
            GameStatistics.Instance.SetLeftHanded(v);
            if(!firstTime)
                SoundManager.Instance.ButtonRelease();
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
            if (firstTime) return;
            SetUI(v);
        }

        private void SetUI(bool left)
        {
            if (_posLeft == null)
            {
                _posLeft = new Vector2[toFlip.Length];
                for (int i = 0; i < toFlip.Length; i++)
                {
                    _posLeft[i] = toFlip[i].localPosition;
                }
            }
            if (left && !_left)  // changed to left handed
            {
                for (int i = 0; i < toFlip.Length; i++)
                {
                    // flip object across the x-axis
                    toFlip[i].anchorMin = new  Vector2(1-toFlip[i].anchorMin.x, toFlip[i].anchorMin.y);
                    toFlip[i].anchorMax = new  Vector2(1-toFlip[i].anchorMax.x, toFlip[i].anchorMax.y);
                    toFlip[i].pivot = new Vector2(1-toFlip[i].pivot.x, toFlip[i].pivot.y);
                    toFlip[i].localPosition = new Vector2(_posLeft[i].x, _posLeft[i].y);
                }

                _left = true;
            }
            else if (!left && _left) // changed to right handed
            {
                for (int i = 0; i < toFlip.Length; i++)
                {
                    // flip object across the x-axis
                    toFlip[i].anchorMin = new  Vector2(1-toFlip[i].anchorMin.x, toFlip[i].anchorMin.y);
                    toFlip[i].anchorMax = new  Vector2(1-toFlip[i].anchorMax.x, toFlip[i].anchorMax.y);
                    toFlip[i].pivot = new Vector2(1-toFlip[i].pivot.x, toFlip[i].pivot.y);
                    toFlip[i].localPosition = new Vector2(-_posLeft[i].x, _posLeft[i].y);
                }
                _left = false;
            }
        }

        public void OnClick()
        {
            ToggleLeftHanded(!GameStatistics.Instance.leftHanded);
        }
    }
}
