using System;
using TMPro;
using UnityEngine;
using World;

namespace UI
{
    public class UIButtonPressBehaviour: MonoBehaviour
    {
        private Transform _text;

        private void Awake()
        {
            _text = transform.childCount == 0 ? null : transform.GetChild(0);
        }
        
        public void OnPress()
        {
            SoundManager.Instance.ButtonPress();
            if (_text == null) return;
            _text.position = new Vector3(_text.position.x, _text.position.y - transform.localScale.x, _text.position.z);
        }
        
        public void OnRelease()
        {
            SoundManager.Instance.ButtonRelease();
            if (_text == null) return;
            _text.position = new Vector3(_text.position.x, _text.position.y + transform.localScale.x, _text.position.z);
        }
    }
}