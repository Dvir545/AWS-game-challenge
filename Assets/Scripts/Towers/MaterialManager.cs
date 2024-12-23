using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;

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
        private int _uiStartX = 256;
        private int _uiXoffsetBetweenMaterials = 128;
        private int _uiXoffsetPerMaterial;

        private void Awake()
        {
            _uiXoffsetPerMaterial = _uiXoffsetBetweenMaterials / 2;
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
        }
        
        public void AddMaterial(TowerMaterial towerMaterial)
        {
            if (playerData.GetNumMaterials(towerMaterial) == 0)
                EnableMaterialUI(towerMaterial);
            playerData.AddMaterial(towerMaterial);
            _materialAmount[towerMaterial].text = playerData.GetNumMaterials(towerMaterial).ToString();
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
            var xOffset = _uiStartX;
            foreach (var materialUI in _materialUI)
            {
                if ((int)materialUI.Key < (int)towerMaterial && materialUI.Value.activeSelf)
                {
                    materialUI.Value.transform.localPosition -= new Vector3(_uiXoffsetPerMaterial, 0, 0);
                    xOffset += _uiXoffsetPerMaterial;
                }
                else if ((int)materialUI.Key > (int)towerMaterial && materialUI.Value.activeSelf)
                {
                    materialUI.Value.transform.localPosition += new Vector3(_uiXoffsetPerMaterial, 0, 0);
                    xOffset -= _uiXoffsetPerMaterial;
                }
            }
            _materialUI[towerMaterial].transform.localPosition = new Vector3(xOffset, 0, 0);
            _materialUI[towerMaterial].SetActive(true);
        }

        private void DisableMaterialUI(TowerMaterial towerMaterial)
        {
            _materialUI[towerMaterial].SetActive(false);
            foreach (var materialUI in _materialUI)
            {
                if ((int)materialUI.Key < (int)towerMaterial && materialUI.Value.activeSelf)
                {
                    materialUI.Value.transform.localPosition += new Vector3(_uiXoffsetPerMaterial, 0, 0);
                }
                else if ((int)materialUI.Key > (int)towerMaterial && materialUI.Value.activeSelf)
                {
                    materialUI.Value.transform.localPosition -= new Vector3(_uiXoffsetPerMaterial, 0, 0);
                }
            }
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