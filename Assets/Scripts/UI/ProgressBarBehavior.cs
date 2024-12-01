using Unity.Mathematics;
using UnityEngine;

namespace Player
{
    public class ProgressBarBehavior : MonoBehaviour
    {
        [SerializeField] private Sprite[] progressSprites;
        private SpriteRenderer _spriteRenderer;
        private float _changeSpriteInterval;
        private int _curSprite;
        public bool IsWorking { get; private set; }

        void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _changeSpriteInterval = 1f / progressSprites.Length;
            _curSprite = -1;
            IsWorking = false;
        }

        public void StartWork(float progress)
        {
            _curSprite = -1;
            UpdateProgress(progress);
            IsWorking = true;
            _spriteRenderer.enabled = true;
        }

        public void StopWork()
        {
            _spriteRenderer.enabled = false;
            IsWorking = false;
            _curSprite = -1;
        }


        public void UpdateProgress(float progress)
        {
            int i = (int)math.floor(progress / _changeSpriteInterval);
            if (i > _curSprite && i < progressSprites.Length)
            {
                _spriteRenderer.sprite = progressSprites[i];
                _curSprite++;
            }
        }
    }
}
