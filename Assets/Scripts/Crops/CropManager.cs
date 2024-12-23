using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Utils;
using Utils.Data;

namespace Crops
{
    public class CropManager : MonoBehaviour
    {
        [SerializeField] private PlayerData  playerData;
        
        [Header("Crop UI")] 
        [SerializeField] private Transform cropUIParent;
        [SerializeField] private GameObject wheatUI;
        [SerializeField] private TextMeshProUGUI wheatAmount;
        [SerializeField] private GameObject carrotUI;
        [SerializeField] private TextMeshProUGUI carrotAmount;
        [SerializeField] private GameObject tomatoUI;
        [SerializeField] private TextMeshProUGUI tomatoAmount;
        [SerializeField] private GameObject cornUI;
        [SerializeField] private TextMeshProUGUI cornAmount;
        [SerializeField] private GameObject pumpkinUI;
        [SerializeField] private TextMeshProUGUI pumpkinAmount;
        private Dictionary<Crop, GameObject> _cropUI = new Dictionary<Crop, GameObject>();
        private Dictionary<Crop, TextMeshProUGUI> _cropAmount = new Dictionary<Crop, TextMeshProUGUI>();
        private int _uiStartX = 256;
        private int _uiXoffsetBetweenCrops = 128;
        private int _uiXoffsetPerCrop;

        private void Awake()
        {
            _uiXoffsetPerCrop = _uiXoffsetBetweenCrops / 2;
            _cropUI.Add(Crop.Wheat, wheatUI);
            _cropUI.Add(Crop.Carrot, carrotUI);
            _cropUI.Add(Crop.Tomato, tomatoUI);
            _cropUI.Add(Crop.Corn, cornUI);
            _cropUI.Add(Crop.Pumpkin, pumpkinUI);

            _cropAmount.Add(Crop.Wheat, wheatAmount);
            _cropAmount.Add(Crop.Carrot, carrotAmount);
            _cropAmount.Add(Crop.Tomato, tomatoAmount);
            _cropAmount.Add(Crop.Corn, cornAmount);
            _cropAmount.Add(Crop.Pumpkin, pumpkinAmount);
            
            for (var i = 0; i < playerData.GetNumCropTypes(); i++)
            {
                if (playerData.GetNumCrops((Crop)i) == 0)
                    DisableCropUI((Crop)i);
                _cropAmount[(Crop)i].text = playerData.GetNumCrops((Crop)i).ToString();
            }
        }

        public void AddCrop(Crop crop)
        {
            if (playerData.GetNumCrops(crop) == 0)
            {
                EnableCropUI(crop);
            }
            playerData.AddCrop(crop);
            _cropAmount[crop].text = playerData.GetNumCrops(crop).ToString();
        }

        public bool HasCrops()
        {
            for (var i = 0; i < playerData.GetNumCropTypes(); i++)
            {
                if (playerData.GetNumCrops((Crop)i) > 0)
                    return true;
            }
            return false;
        }
        
        public void RemoveCrop(Crop crop)
        {
            playerData.RemoveCrop(crop);
            if (playerData.GetNumCrops(crop) == 0)
            {
                DisableCropUI(crop);
            }
            else
                _cropAmount[crop].text = playerData.GetNumCrops(crop).ToString();
        }
        
        private void EnableCropUI(Crop crop)
        {
            var xOffset = _uiStartX;
            foreach (var cropUI in _cropUI)
            {
                if ((int)cropUI.Key < (int)crop && cropUI.Value.activeSelf)
                {
                    cropUI.Value.transform.localPosition -= new Vector3(_uiXoffsetPerCrop, 0, 0);
                    xOffset += _uiXoffsetPerCrop;
                }
                else if ((int)cropUI.Key > (int)crop && cropUI.Value.activeSelf)
                {
                    cropUI.Value.transform.localPosition += new Vector3(_uiXoffsetPerCrop, 0, 0);
                    xOffset -= _uiXoffsetPerCrop;
                }
            }
            _cropUI[crop].transform.localPosition = new Vector3(xOffset, 0, 0);
            _cropUI[crop].SetActive(true);
        }

        private void DisableCropUI(Crop crop)
        {
            _cropUI[crop].SetActive(false);
            foreach (var cropUI in _cropUI)
            {
                if ((int)cropUI.Key < (int)crop && cropUI.Value.activeSelf)
                {
                    cropUI.Value.transform.localPosition += new Vector3(_uiXoffsetPerCrop, 0, 0);
                }
                else if ((int)cropUI.Key > (int)crop && cropUI.Value.activeSelf)
                {
                    cropUI.Value.transform.localPosition -= new Vector3(_uiXoffsetPerCrop, 0, 0);
                }
            }
        }

        public Crop GetBestAvailableCrop()
        {
            for (var i = playerData.GetNumCropTypes() - 1; i >= 0; i--)
            {
                if (playerData.GetNumCrops((Crop)i) > 0)
                {
                    return (Crop)i;
                }
            }
            throw new KeyNotFoundException("No available crops");
        }
    }
}