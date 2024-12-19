using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GameUI
{
    public class SpeechBubbleBehaviour : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textUI;

        private Image _midImage;
        private const float baseHeight = 28.32f;
        private const float heightPerFontSize = 0.92f;
    
        void Awake()
        {
            _midImage = GetComponent<Image>();
        }
    
        void Start()
        {
            SetText(textUI.text);
        }

        public void SetText(string text)
        {
            if (_midImage == null)
            {
                _midImage = GetComponent<Image>();
            }
            textUI.text = text;
            // change the size of the speech bubble based on the text length
            var preferredHeightPerLineByFont = heightPerFontSize * textUI.fontSize;
            var numLines = Mathf.RoundToInt((textUI.preferredHeight - baseHeight) / preferredHeightPerLineByFont);
            var lineHeight = preferredHeightPerLineByFont;
            var height = numLines * lineHeight;
            var rect = _midImage.rectTransform;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        }
    }
}
