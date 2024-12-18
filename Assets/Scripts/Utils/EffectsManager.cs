using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Utils
{
    public class EffectsManager : MonoBehaviour
    {
        [SerializeField] private GameObject cashRewardPrefab;


        public void FloatingTextEffect(Vector2 pos, float distance, float duration, string inputText, Color color, float sizeMultiplier=1f)
        {
            var centeredPos = new Vector2(pos.x, pos.y + 0.5f);
            var floatingTextObj = Instantiate(cashRewardPrefab, centeredPos, Quaternion.identity);
            floatingTextObj.transform.localScale *= sizeMultiplier;
            var targetY = pos.y + distance;
            var text = floatingTextObj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = inputText;
            text.color = color;
            floatingTextObj.transform.DOMoveY(targetY, duration).SetEase(Ease.OutCubic);
            // also fade out
            text.DOFade(0, duration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                Destroy(floatingTextObj);
            });
        }
    }
}