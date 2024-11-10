using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Utils.Data
{
    public class CropsData : MonoBehaviour
    {
        [Header("Crop Sprites")]
        [SerializeField] private Sprite[] wheatSprites;
        [SerializeField] private Sprite[] carrotSprites;
        [SerializeField] private Sprite[] tomatoSprites;
        [SerializeField] private Sprite[] cornSprites;
        [SerializeField] private Sprite[] pumpkinSprites;
        [Header("Crop UI")] 
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
        private Vector2 _UIstartPos = new Vector2(0, 50);
        private int _UIXoffset = 100;

        private int[] _numCrops = {1, 1, 1, 1, 1};

        private void Awake()
        {
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
            
            for (var i = 0; i < _cropUI.Count; i++)
            {
                if (_numCrops[i] == 0)
                    DisableCropUI((Crop)i);
                _cropAmount[(Crop)i].text = _numCrops[i].ToString();
            }
        }
        
        public void AddCrop(Crop crop)
        {
            if (_numCrops[(int)crop] == 0)
                EnableCropUI(crop);
            _numCrops[(int)crop]++;
            _cropAmount[crop].text = _numCrops[(int)crop].ToString();
        }

        public bool HasCrops()
        {
            foreach (var numCrop in _numCrops)
            {
                if (numCrop > 0)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void RemoveCrop(Crop crop)
        {
            _numCrops[(int)crop]--;
            if (_numCrops[(int)crop] == 0)
                DisableCropUI(crop);
            else
                _cropAmount[crop].text = _numCrops[(int)crop].ToString();
        }
        
        private void EnableCropUI(Crop crop)
        {
            _cropUI[crop].SetActive(true);
            _cropUI[crop].transform.localPosition = _UIstartPos;
            _UIstartPos.x += _UIXoffset;
        }

        private void DisableCropUI(Crop crop)
        {
            _cropUI[crop].SetActive(false);
            _UIstartPos.x -= _UIXoffset;
        }

        public Sprite[] GetCropSprites(Crop crop)
        {
            return crop switch
            {
                Crop.Wheat => wheatSprites,
                Crop.Carrot => carrotSprites,
                Crop.Tomato => tomatoSprites,
                Crop.Corn => cornSprites,
                Crop.Pumpkin => pumpkinSprites,
                _ => throw new KeyNotFoundException("Crop not found")
            };
        }

        public Crop GetBestAvailableCrop()
        {
            for (var i = _numCrops.Length - 1; i >= 0; i--)
            {
                if (_numCrops[i] > 0)
                {
                    return (Crop)i;
                }
            }
            throw new KeyNotFoundException("No available crops");
        }
    }
}