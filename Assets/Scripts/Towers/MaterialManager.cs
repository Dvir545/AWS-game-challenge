using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Utils;

namespace Towers
{
    public class MaterialManager : MonoBehaviour
    {
        [SerializeField] private PlayerData  playerData;
        
        [Header("Material UI")] 
        [SerializeField] private Transform materialUIParent;
        [SerializeField] private GameObject woodUI;
        [SerializeField] private TextMeshProUGUI woodAmount;
        [SerializeField] private GameObject stoneUI;
        [SerializeField] private TextMeshProUGUI stoneAmount;
        [SerializeField] private GameObject steelUI;
        [SerializeField] private TextMeshProUGUI steelAmount;
        [SerializeField] private GameObject goldUI;
        [SerializeField] private TextMeshProUGUI goldAmount;
        [SerializeField] private GameObject diamondUI;
        [SerializeField] private TextMeshProUGUI diamondAmount;
        private Dictionary<TowerMaterial, GameObject> _materialUI = new Dictionary<TowerMaterial, GameObject>();
        private Dictionary<TowerMaterial, TextMeshProUGUI> _materialAmount = new Dictionary<TowerMaterial, TextMeshProUGUI>();
        private float _uiParentStartX;
        private Vector2 _uiStartPos = new(0, 50);
        private int _uiXoffset = 128;

        private Vector2 _uiCurOffset;

        private void Awake()
        {
            _uiParentStartX = materialUIParent.localPosition.x;

            _materialUI.Add(TowerMaterial.Wood, woodUI);
            _materialUI.Add(TowerMaterial.Stone, stoneUI);
            _materialUI.Add(TowerMaterial.Steel, steelUI);
            _materialUI.Add(TowerMaterial.Gold, goldUI);
            _materialUI.Add(TowerMaterial.Diamond, diamondUI);

            _materialAmount.Add(TowerMaterial.Wood, woodAmount);
            _materialAmount.Add(TowerMaterial.Stone, stoneAmount);
            _materialAmount.Add(TowerMaterial.Steel, steelAmount);
            _materialAmount.Add(TowerMaterial.Gold, goldAmount);
            _materialAmount.Add(TowerMaterial.Diamond, diamondAmount);
            
            for (var i = 0; i < playerData.GetNumMaterialTypes(); i++)
            {
                if (playerData.GetNumMaterials((TowerMaterial)i) == 0)
                    DisableMaterialUI((TowerMaterial)i);
                _materialAmount[(TowerMaterial)i].text = playerData.GetNumMaterials((TowerMaterial)i).ToString();
            }

            SetMaterialUIParentPosition();

            _uiCurOffset = new Vector2(_materialUI.Count * 128, 0);
        }
        
        private void SetMaterialUIParentPosition()
        {
            var numActiveMaterials = 0;
            foreach (var materialUI in _materialUI)
            {
                if (materialUI.Value.activeSelf)
                    numActiveMaterials++;
            }
            materialUIParent.localPosition = new Vector2(_uiParentStartX + (5-numActiveMaterials) * 64, 0);
        }
        
        public void AddMaterial(TowerMaterial towerMaterial)
        {
            if (playerData.GetNumMaterials(towerMaterial) == 0)
                EnableMaterialUI(towerMaterial);
            playerData.AddMaterial(towerMaterial);
            _materialAmount[towerMaterial].text = playerData.GetNumMaterials(towerMaterial).ToString();
            SetMaterialUIParentPosition();
        }

        public void AddMaterial(int materialN)
        {
            AddMaterial((TowerMaterial)materialN);
        }

        public bool HasMaterials()
        {
            for (var i = 0; i < playerData.GetNumMaterialTypes(); i++)
            {
                if (playerData.GetNumMaterials((TowerMaterial)i) > 0)
                    return true;
            }
            return false;
        }
        
        public void RemoveMaterial(TowerMaterial towerMaterial)
        {
            playerData.RemoveMaterial(towerMaterial);
            if (playerData.GetNumMaterials(towerMaterial) == 0)
                DisableMaterialUI(towerMaterial);
            else
                _materialAmount[towerMaterial].text = playerData.GetNumMaterials(towerMaterial).ToString();
        }
        
        private void EnableMaterialUI(TowerMaterial towerMaterial)
        {
            float xOffset = _uiCurOffset.x;
            // move all materials after this one by offset to the right
            foreach (var materialUI in _materialUI)
            {
                if ((int)materialUI.Key > (int)towerMaterial && materialUI.Value.activeSelf)
                {
                    if (materialUI.Value.transform.localPosition.x < xOffset)
                        xOffset = materialUI.Value.transform.localPosition.x;
                    materialUI.Value.transform.localPosition += new Vector3(_uiXoffset, 0, 0);
                }
            }
            _materialUI[towerMaterial].transform.localPosition = _uiStartPos + new Vector2(xOffset, 0);
            _uiCurOffset += new Vector2(_uiXoffset, 0);
            _materialUI[towerMaterial].SetActive(true);
        }

        private void DisableMaterialUI(TowerMaterial towerMaterial)
        {
            _materialUI[towerMaterial].SetActive(false);
            // move all crops after this one to the left
            foreach (var materialUI in _materialUI)
            {
                if ((int)materialUI.Key > (int)towerMaterial && materialUI.Value.activeSelf)
                {
                    materialUI.Value.transform.localPosition -= new Vector3(_uiXoffset, 0, 0);
                }
            }
            _uiCurOffset.x -= _uiXoffset;
        }

        public TowerMaterial GetBestAvailableMaterial()
        {
            for (var i = playerData.GetNumMaterialTypes() - 1; i >= 0; i--)
            {
                if (playerData.GetNumMaterials((TowerMaterial)i) > 0)
                {
                    return (TowerMaterial)i;
                }
            }
            throw new KeyNotFoundException("No available materials");
        }
    }
}