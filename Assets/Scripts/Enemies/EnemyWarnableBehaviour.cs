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
        private EnemyHealthManager _healthManager;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _healthManager = transform.parent.GetComponent<EnemyHealthManager>();
        }

        public virtual void SetWarningSign(WarningSignBehaviour warningSign)
        {
            _warningSign = warningSign;
        }

        public virtual bool IsVisible()
        {
            if (_spriteRenderer == null) return false;
            return _spriteRenderer.isVisible;
        }

        public virtual void ShowWarningSign()
        {
            if (_warningSign == null)
                return;
            _warningSign.SetVisibility(false);
        }
        
        public virtual void HideWarningSign()
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
            if (_healthManager.IsDead) return;
            ShowWarningSign();
        }

        public virtual void Reset()
        {
            _warningSign = null;
        }
    }
}