using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.WarningSign
{
    public class WarningSignBehaviour : MonoBehaviour
    {
        private Transform _leftArrow;
        private Transform _downArrow;
        private Transform _upArrow;
        private Transform _rightArrow;
        private Transform _activeArrow;

        private Image _leftArrowImage;
        private Image _downArrowImage;
        private Image _upArrowImage;
        private Image _rightArrowImage;
        private Image _signImage;
        private Image[] _images;

        private RectTransform _rect;
        private Vector2 _parentBounds;

        private Transform _target;
        private Transform _player;
    
        private bool _isTargetVisible;

        private void Awake()
        {
            _leftArrow = transform.GetChild(0);
            _downArrow = transform.GetChild(1);
            _upArrow = transform.GetChild(2);
            _rightArrow = transform.GetChild(3);
            _activeArrow = _rightArrow;
            _leftArrowImage = _leftArrow.GetComponent<Image>();
            _downArrowImage = _downArrow.GetComponent<Image>();
            _upArrowImage = _upArrow.GetComponent<Image>();
            _rightArrowImage = _rightArrow.GetComponent<Image>();
            _signImage = transform.GetChild(4).GetComponent<Image>();
            _images = new[]
            {
                _leftArrowImage, _downArrowImage, _upArrowImage, _rightArrowImage, _signImage
            };
            _rect = GetComponent<RectTransform>();
            _player = GameObject.FindWithTag("Player").transform;
        }

        public void Init(Transform target)
        {
            _target = target;
            var warnable = target.GetComponent<IWarnable>();
            warnable.SetWarningSign(this);
            _parentBounds = transform.parent.GetComponent<RectTransform>().rect.size * 0.5f;
            SetVisibility(warnable.IsVisible());
        }
        
        public void SetVisibility(bool isTargetVisible)
        {
            _isTargetVisible = isTargetVisible;
            ToggleVisibility(!isTargetVisible);
        }

        private void ToggleVisibility(bool show)
        {
            foreach (var image in _images)
            {
                image.enabled = show;
            }
            if (show)
            {
                if (_rect == null) return;
                UpdatePosition();
            }
        }

        private void Update()
        {
            if (!_isTargetVisible)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (gameObject.activeSelf == false) return;
            Vector2 direction = _target.position - _player.position;
            // Calculate the angle to determine which arrow to show
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _activeArrow.gameObject.SetActive(false);
            if (angle is > -26 and <= 26)
            {
                _rightArrow.gameObject.SetActive(true);
                _activeArrow = _rightArrow;
            }
            else if (angle is > 26 and <= 154)
            {
                _upArrow.gameObject.SetActive(true);
                _activeArrow = _upArrow;
            }
            else if (angle is > 154 or <= -154)
            {
                _leftArrow.gameObject.SetActive(true);
                _activeArrow = _leftArrow;
            }
            else
            {
                _downArrow.gameObject.SetActive(true);
                _activeArrow = _downArrow;
            }

            // Find the intersection point with the parent rect
            Vector2 normalizedDir = direction.normalized;
            Vector2 intersection = FindIntersectionWithRect(normalizedDir, _parentBounds);
    
            // Set the warning sign's local position
            _rect.localPosition = intersection;
        }


        private Vector2 FindIntersectionWithRect(Vector2 direction, Vector2 rectBounds)
        {
            float scale;
    
            // Find which edge the direction vector intersects with
            if (Mathf.Abs(direction.x) * rectBounds.y > Mathf.Abs(direction.y) * rectBounds.x)
            {
                // Intersects with vertical edge
                scale = rectBounds.x / Mathf.Abs(direction.x);
            }
            else
            {
                // Intersects with horizontal edge
                scale = rectBounds.y / Mathf.Abs(direction.y);
            }
    
            // Calculate the intersection point
            return direction * scale;
        }
    }
}
