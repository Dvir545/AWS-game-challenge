using System;
using System.Collections;
using UI.WarningSign;
using UnityEngine;
using Utils;
using Utils.Data;
using World;

namespace Crops
{
    public class CropBehaviour: MonoBehaviour, IWarnable
    {
        private float _prevDestroyProgress;  // to detect when destroy stops
        private SpriteRenderer _cropSprite;
        private AudioSource _audioSource;
        [SerializeField] private AudioClip cropStageChangedAudio;
        private bool _isBeingDestroyed;
        private WarningSignBehaviour _warningSign;
        private Vector2Int _position;
        private Crop Crop => (Crop)GameData.Instance.plantedCrops[_position].cropType;
        private Coroutine _cr;
        private CropStage _nextStage;
        private bool _destroyed;
        public CropStage Stage
        {
            get => GameData.Instance.plantedCrops[_position].stage;
            private set => GameData.Instance.plantedCrops[_position].stage = value;
        }

        private float PlantProgress
        {
            get => GameData.Instance.plantedCrops[_position].plantProgress;
            set => GameData.Instance.plantedCrops[_position].plantProgress = value;
        }

        private float DestroyProgress
        {
            get => GameData.Instance.plantedCrops[_position].destroyProgress;
            set => GameData.Instance.plantedCrops[_position].destroyProgress = value;
        }

        public void Awake()
        {
            _cropSprite = GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void Init(Vector2Int position, Crop crop, CropStage stage, float plantProgress=0f, float destroyProgress=0f)
        {
            _position = position;
            if (!GameData.Instance.plantedCrops.ContainsKey(position))
                GameData.Instance.plantedCrops[position] = new PlantedCropInfo((int)crop, stage, plantProgress, destroyProgress);
            _cropSprite.sprite = SpriteData.Instance.GetCropSprite(crop, stage);
            if (stage == CropStage.Planted && plantProgress == 0)
                SoundManager.Instance.PlaySFX(_audioSource, cropStageChangedAudio);
            _destroyed = false;
        }

        public void Update()
        {
            if (_isBeingDestroyed && Math.Abs(DestroyProgress - _prevDestroyProgress) < 0.0001f)
            {
                _isBeingDestroyed = false;
                EventManager.Instance.TriggerEvent(EventManager.CropStoppedBeingDestroyed, transform);
                EventManager.Instance.TriggerEvent(EventManager.CropReadyForHarvest, (transform, WarningSignType.Harvest));
                
            }
            _prevDestroyProgress = DestroyProgress;
        }
            
        public Crop GetCrop()
        {
            return Crop;
        }
            
        public float GetPlantProgress()
        {
            return PlantProgress;
        }
        public bool AddToPlantProgress(float progress)
        {
            PlantProgress += progress;
            if (PlantProgress >= 1)
            {
                PlantProgress = 1;
                _nextStage = CropStage.Seed;
                SetCropStage();
                return true;
            }
            return false;
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
                EventManager.Instance.TriggerEvent(EventManager.CropBeingDestroyed, (transform, WarningSignType.Warning));
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_destroyed) return;
            if (Stage == CropStage.Ripe && other.CompareTag("Player"))
            {
                EventManager.Instance.TriggerEvent(EventManager.CropHarvested, transform);
                Destroy(gameObject);
            }
        }

        public bool IsEdible()
        {
            return Stage > CropStage.Planted;
        }

        private void SetCropStage()
        {
            Stage = _nextStage;
            _cropSprite.sprite = SpriteData.Instance.GetCropSprite(Crop, Stage);
            SoundManager.Instance.PlaySFX(_audioSource, cropStageChangedAudio);
            if (Stage == CropStage.Ripe && !_isBeingDestroyed)
            {
                EventManager.Instance.TriggerEvent(EventManager.CropReadyForHarvest, (transform, WarningSignType.Harvest));
            }
        }
        
        private IEnumerator SetStageCR(CropStage stage)
        {
            _nextStage = stage;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 5f));
            SetCropStage();
        } 

        public void SetStage(CropStage stage)
        {
            if (_cr != null)
            {
                StopCoroutine(_cr);
                _cr = null;
                SetCropStage();
            }
            _cr = StartCoroutine(SetStageCR(stage));
        }

        private void OnDestroy()
        {
            _destroyed = true;
        }
    }
}