using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Utils
{
    public class EffectsManager : MonoBehaviour
    {
        [SerializeField] private GameObject cashRewardPrefab;


        public void CashRewardEffect(Vector3Int pos, string amount)
        {
            Vector3 centeredPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z);
            GameObject rewardEffect = Instantiate(cashRewardPrefab, centeredPos, Quaternion.identity);
            float targetY = pos.y + 1;
            TextMeshProUGUI text = rewardEffect.GetComponentInChildren<TextMeshProUGUI>();
            text.text = amount + "$";
            rewardEffect.transform.DOMoveY(targetY, 1).SetEase(Ease.OutCubic);
            // also fade out
            text.DOFade(0, 1).SetEase(Ease.InCubic).OnComplete(() =>
            {
                Destroy(rewardEffect);
            });
        }
    }
}