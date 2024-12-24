using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIButtonPressBehaviour: MonoBehaviour
    {
        private Transform _text;

        private void Awake()
        {
            _text = transform.GetChild(0);
        }
        
        public void OnPress()
        {
            _text.position = new Vector3(_text.position.x, _text.position.y - transform.localScale.x, _text.position.z);
        }
        
        public void OnRelease()
        {
            _text.position = new Vector3(_text.position.x, _text.position.y + transform.localScale.x, _text.position.z);
        }
    }
}