using System;
using Amazon.Runtime.Internal.Transform;
using UI.WarningSign;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Crops
{
    public class CropBehaviour: MonoBehaviour, IWarnable
    {
        private float _prevDestroyProgress;  // to detect when destroy stops
        private SpriteRenderer _cropSprite;
        private bool _isBeingDestroyed;
        private WarningSignBehaviour _warningSign;
        private Vector2Int _position;
        private Crop Crop => (Crop)GameData.Instance.plantedCrops[_position].cropType;
        private float Progress
        {
            get => GameData.Instance.plantedCrops[_position].growthProgress;
            set => GameData.Instance.plantedCrops[_position].growthProgress = value;
        }

        private float DestroyProgress
        {
            get => GameData.Instance.plantedCrops[_position].destroyProgress;
            set => GameData.Instance.plantedCrops[_position].destroyProgress = value;
        }

        public void Awake()
        {
            _cropSprite = GetComponent<SpriteRenderer>();
        }

        public void Init(Vector2Int position, Crop crop)
        {
            _position = position;
            if (!GameData.Instance.plantedCrops.ContainsKey(position))
                GameData.Instance.plantedCrops[position] = new PlantedCropInfo((int)crop, 0, 0);
        }
        
        public void Update()
        {
            if (_isBeingDestroyed && Math.Abs(DestroyProgress - _prevDestroyProgress) < 0.0001f)
            {
                _isBeingDestroyed = false;
                EventManager.Instance.TriggerEvent(EventManager.CropStoppedBeingDestroyed, transform);
            }
            _prevDestroyProgress = DestroyProgress;
        }
            
        public Crop GetCrop()
        {
            return Crop;
        }
            
        public float GetProgress()
        {
            return Progress;
        }
        public void AddToProgress(float progress)
        {
            Progress += progress;
            _cropSprite.sprite = SpriteData.Instance.GetCropSprite(Crop, Progress);
        }
            
        public float GetDestroyProgress()
        {
            return DestroyProgress;
        }
            
        public void AddToDestroyProgress(float progress)
        {
            DestroyProgress += progress;
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