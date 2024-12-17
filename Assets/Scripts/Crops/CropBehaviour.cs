using System;
using UI.WarningSign;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Crops
{
    public class CropBehaviour: MonoBehaviour, IWarnable
    {
        [SerializeField] private Crop crop;
        private float _progress;
        private float _destroyProgress;
        private float _prevDestroyProgress;  // to detect when destroy stops
        private SpriteRenderer _cropSprite;
        private bool _isBeingDestroyed;
        private WarningSignBehaviour _warningSign;
            
        public void Awake()
        {
            _cropSprite = GetComponent<SpriteRenderer>();
        }
        
        public void Update()
        {
            if (_isBeingDestroyed && Math.Abs(_destroyProgress - _prevDestroyProgress) < 0.0001f)
            {
                _isBeingDestroyed = false;
                EventManager.Instance.TriggerEvent(EventManager.CropStoppedBeingDestroyed, transform);
            }
            _prevDestroyProgress = _destroyProgress;
        }
            
        public Crop GetCrop()
        {
            return crop;
        }
            
        public float GetProgress()
        {
            return _progress;
        }
        public void AddToProgress(float progress)
        {
            _progress += progress;
            _cropSprite.sprite = SpriteData.Instance.GetCropSprite(crop, _progress);
        }
            
        public float GetDestroyProgress()
        {
            return _destroyProgress;
        }
            
        public void AddToDestroyProgress(float progress)
        {
            _destroyProgress += progress;
            if (!_isBeingDestroyed)
            {
                _isBeingDestroyed = true;
                EventManager.Instance.TriggerEvent(EventManager.CropBeingDestroyed, (transform, false));
            }
        }

        public void SetWarningSign(WarningSignBehaviour warningSign)
        {
            _warningSign = warningSign;
        }
        
        public bool IsVisible()
        {
            return _cropSprite.isVisible;
        }

        private void OnBecameVisible()
        {
            if (_warningSign == null) return;
            _warningSign.SetVisibility(true);
        }
        
        private void OnBecameInvisible()
        {
            if (_warningSign == null) return;
            _warningSign.SetVisibility(false);
        }
    }
}