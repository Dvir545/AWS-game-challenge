using System.Collections.Generic;
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

        private int[] _numCrops = {1, 1, 1, 1, 1};

        public void AddCrop(Crop crop)
        {
            _numCrops[(int)crop]++;
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