using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using Utils.Data;

namespace World
{
    public class FarmingManager: MonoBehaviour
    {
        [SerializeField] private Tilemap canFarmTilemap;
        [SerializeField] private Tilemap farmTilemap;
        [SerializeField] private TileBase farmTile;
        [SerializeField] private CropsData cropsData;
        [SerializeField] private GameObject cropSpritePrefab;
        [SerializeField] private GameObject cropsParent;
        [SerializeField] private Transform playerTransform;
        
        private class FarmData
        {
            private Crop _crop;
            private float _progress;
            private SpriteRenderer _cropSprite;
            
            public FarmData(Crop crop, GameObject  curSpriteObject, float progress)
            {
                _crop = crop;
                _progress = progress;
                _cropSprite = curSpriteObject.GetComponent<SpriteRenderer>();
            }
            
            public Crop GetCrop()
            {
                return _crop;
            }
            
            public float GetProgress()
            {
                return _progress;
            }
            public void AddToProgress(float progress)
            {
                _progress += progress;
            }
            
            public SpriteRenderer GetCropSpriteRenderer()
            {
                return _cropSprite;
            }
        }
        
        private Dictionary<Vector3Int, FarmData> _farms = new();
        
        public void Farm()
        {
            Vector3Int tilePos = canFarmTilemap.WorldToCell(playerTransform.position);
            bool canFarm = !(canFarmTilemap.GetTile(tilePos) is null) && cropsData.HasCrops();
            bool emptyTile = farmTilemap.GetTile(tilePos) is null;

            if (canFarm && emptyTile)
            {
                // Start farming on this tile
                if (!_farms.ContainsKey(tilePos))
                {
                    farmTilemap.SetTile(tilePos, farmTile);
                    GameObject cropSprite = Instantiate(cropSpritePrefab, tilePos, Quaternion.identity);
                    cropSprite.transform.SetParent(cropsParent.transform);
                    _farms[tilePos] = new FarmData(cropsData.GetBestAvailableCrop(), cropSprite, 0f);
                    cropsData.RemoveCrop(_farms[tilePos].GetCrop());
                }
            }

            // If we're on a tile that's being farmed, increase its progress
            if (_farms.ContainsKey(tilePos))
            {
                _farms[tilePos].AddToProgress(Constants.FarmProgressIncreasePerSecond * 
                                   GameData.GetProgressSpeedMultiplier * Time.deltaTime);
                Sprite  cropSprite = GetCropSprite(_farms[tilePos]);
                if (!(cropSprite is null))
                {
                    _farms[tilePos].GetCropSpriteRenderer().sprite = cropSprite;
                }
                
                // Check if farming is complete
                if (_farms[tilePos].GetProgress() >= 100f)
                {
                    // Remove the tile and its progress
                    farmTilemap.SetTile(tilePos, null);
                    Destroy(_farms[tilePos].GetCropSpriteRenderer().gameObject);
                    _farms.Remove(tilePos);
            
                    // Here you can add additional effects or rewards
                    // For example: SpawnReward(tilePos);
                }
            }
        }

        private Sprite GetCropSprite(FarmData farmData)
        {
            Sprite[] sprites = cropsData.GetCropSprites(farmData.GetCrop());
            if (farmData.GetProgress() >= 80)
            {
                return sprites[3];
            } if (farmData.GetProgress() >= 60)
            {
                return sprites[2];
            } if (farmData.GetProgress() >= 40)
            {
                return sprites[1];
            } if (farmData.GetProgress() >= 20)
            {
                return sprites[0];
            } 
            // no sprite
            return null;
        }
    }
}