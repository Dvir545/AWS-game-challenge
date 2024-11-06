using TMPro;
using UnityEngine;

namespace Utils
{
    public class ApplyTextShadow : MonoBehaviour
    {
        [SerializeField] private float outlineWidth = 5f;
        [SerializeField] private Color32 outlineColor = new Color32(0, 0, 0, 255);
        [SerializeField] private TextMeshProUGUI text;
        
        private void Awake()
        {
            text.outlineWidth = outlineWidth;
            text.outlineColor = outlineColor;
        }
    }
}
