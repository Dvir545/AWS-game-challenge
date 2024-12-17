using System;
using UI.WarningSign;
using UnityEngine;
using Utils;

namespace Enemies
{
    public class EnemyWarnableBehaviour: MonoBehaviour, IWarnable
    {
        private WarningSignBehaviour _warningSign;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetWarningSign(WarningSignBehaviour warningSign)
        {
            _warningSign = warningSign;
        }

        public bool IsVisible()
        {
            if (_spriteRenderer == null) return false;
            return _spriteRenderer.isVisible;
        }

        public void ShowWarningSign()
        {
            if (_warningSign == null)
                return;
            _warningSign.SetVisibility(false);
        }
        
        public void HideWarningSign()
        {
            if (_warningSign == null)
                return;
            _warningSign.SetVisibility(true);
        }

        private void OnBecameVisible()
        {
            HideWarningSign();
        }

        private void OnBecameInvisible()
        {
            ShowWarningSign();
        }
    }
}