using System;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using Utils.Data;

namespace UI.GameUI
{
    public class ProgressBarBehavior : MonoBehaviour
    {
        private Sprite[] _progressSprites;
        private ProgressBarType _type;
        private SpriteRenderer _spriteRenderer;
        private float _changeSpriteInterval;
        public bool IsWorking { get; private set; }

        void Awake()
        {
            SetType(ProgressBarType.Default);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            IsWorking = false;
        }

        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.PlayerDied, StopWork);
        }

        private void StopWork(object arg0)
        {
            StopWork();
        }

        public void StartWork(float progress)
        {
            UpdateProgress(progress);
            IsWorking = true;
            _spriteRenderer.enabled = true;
        }

        public void StopWork()
        {
            _spriteRenderer.enabled = false;
            IsWorking = false;
        }


        public void UpdateProgress(float progress)
        {
            int i = (int)math.floor(progress / _changeSpriteInterval);
            if (i < _progressSprites.Length)
            {
                _spriteRenderer.sprite = _progressSprites[i];
            }
        }

        public ProgressBarType GetType() => _type;

        public void SetType(ProgressBarType type)
        {
            _type = type;
            _progressSprites = SpriteData.Instance.GetProgressBarSprites(type);
            _changeSpriteInterval = 1f / _progressSprites.Length;
        }
    }
}
