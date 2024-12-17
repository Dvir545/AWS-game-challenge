using System.Collections.Generic;
using Player;
using TMPro;
using UnityEngine;
using Utils;

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
        private float _uiParentStartX;
        private Vector2 _uiStartPos = new(0, 50);
        private int _uiXoffset = -128;

        private Vector2 _uiCurOffset;

        private void Awake()
        {
            _uiParentStartX = cropUIParent.localPosition.x;
            
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
            
            SetCropUIParentPosition();

            _uiCurOffset = new Vector2(_cropUI.Count * 128, 0);
        }

        private void SetCropUIParentPosition()
        {
            var numActiveCrops = 0;
            foreach (var cropUI in _cropUI)
            {
                if (cropUI.Value.activeSelf)
                    numActiveCrops++;
            }
            cropUIParent.localPosition = new Vector2(_uiParentStartX + (5-numActiveCrops) * 64, 0);
        }

        public void AddCrop(Crop crop)
        {
            if (playerData.GetNumCrops(crop) == 0)
                EnableCropUI(crop);
            playerData.AddCrop(crop);
            _cropAmount[crop].text = playerData.GetNumCrops(crop).ToString();
            SetCropUIParentPosition();
        }

        public void AddCrop(int cropN)
        {
            AddCrop((Crop)cropN);
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
                DisableCropUI(crop);
            else
                _cropAmount[crop].text = playerData.GetNumCrops(crop).ToString();
        }
        
        private void EnableCropUI(Crop crop)
        {
            float xOffset = _uiCurOffset.x;
            // move all crops after this crop by offset to the right
            foreach (var cropUI in _cropUI)
            {
                if ((int)cropUI.Key > (int)crop && cropUI.Value.activeSelf)
                {
                    if (cropUI.Value.transform.localPosition.x < xOffset) 
                        xOffset = cropUI.Value.transform.localPosition.x;
                    cropUI.Value.transform.localPosition += new Vector3(_uiXoffset, 0, 0);
                }
            }
            _cropUI[crop].transform.localPosition = _uiStartPos + new Vector2(xOffset, 0);
            _uiCurOffset += new Vector2(_uiXoffset, 0);
            _cropUI[crop].SetActive(true);
        }

        private void DisableCropUI(Crop crop)
        {
            _cropUI[crop].SetActive(false);
            // move all crops after this one to the left
            foreach (var cropUI in _cropUI)
            {
                if ((int)cropUI.Key > (int)crop && cropUI.Value.activeSelf)
                {
                    cropUI.Value.transform.localPosition -= new Vector3(_uiXoffset, 0, 0);
                }
            }
            _uiCurOffset.x -= _uiXoffset;
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