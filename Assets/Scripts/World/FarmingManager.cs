using System.Collections.Generic;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("cropsData")] [SerializeField] private CropManager cropManager;
        [SerializeField] private GameObject cropSpritePrefab;
        [SerializeField] private GameObject cropsParent;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        [SerializeField] private EffectsManager effectsManager;
        
        private Dictionary<Vector3Int, FarmData> _farms = new();
        
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
        
        
        public void Farm()
        {
            Vector3Int tilePos = canFarmTilemap.WorldToCell(playerTransform.position);
            bool canFarm = !(canFarmTilemap.GetTile(tilePos) is null) && cropManager.HasCrops();
            bool emptyTile = farmTilemap.GetTile(tilePos) is null;

            if (canFarm && emptyTile)
            {
                // Start farming on this tile
                if (!_farms.ContainsKey(tilePos))
                {
                    farmTilemap.SetTile(tilePos, farmTile);
                    GameObject cropSprite = Instantiate(cropSpritePrefab, tilePos, Quaternion.identity);
                    cropSprite.transform.SetParent(cropsParent.transform);
                    _farms[tilePos] = new FarmData(cropManager.GetBestAvailableCrop(), cropSprite, 0f);
                    cropManager.RemoveCrop(_farms[tilePos].GetCrop());
                }
                progressBarBehavior.StartWork(_farms[tilePos].GetProgress());
            }

            // If we're on a tile that's being farmed, increase its progress
            if (_farms.ContainsKey(tilePos))
            {
                HandleFarm(tilePos);
            }
            else  // check if there's an adjacent tile to farm
            {
                bool foundAdjacentFarm = false;
                foreach (Vector3Int adjacentTilePos in tilePos.GetAdjacentTiles(playerMovement.GetFacingDirection()))
                {
                    if (_farms.ContainsKey(adjacentTilePos))
                    {
                        HandleFarm(adjacentTilePos);
                        foundAdjacentFarm = true;
                        break;
                    }
                }
                // If no adjacent farm tiles are found, stop the progress bar
                if (!foundAdjacentFarm && progressBarBehavior.IsWorking)
                {
                    progressBarBehavior.StopWork();
                }
            }
        }

        private void HandleFarm(Vector3Int tilePos)
        {
            progressBarBehavior.StartWork(_farms[tilePos].GetProgress());
            _farms[tilePos].AddToProgress(playerData.GetProgressSpeedMultiplier * Time.deltaTime 
                                          / CropsData.Instance.GetGrowthTime(_farms[tilePos].GetCrop()));
            progressBarBehavior.UpdateProgress(_farms[tilePos].GetProgress());
            Sprite  cropSprite = GetCropSprite(_farms[tilePos]);
            if (!(cropSprite is null))
            {
                _farms[tilePos].GetCropSpriteRenderer().sprite = cropSprite;
            }
                
            // Check if farming is complete
            if (_farms[tilePos].GetProgress() >= 1f)
            {
                progressBarBehavior.StopWork();
                // Remove the tile and its progress
                farmTilemap.SetTile(tilePos, null);
                Destroy(_farms[tilePos].GetCropSpriteRenderer().gameObject);
                // Reward player with the crop sell price
                int amount = CropsData.Instance.GetSellPrice(_farms[tilePos].GetCrop());
                playerData.AddCash(amount);
                effectsManager.CashRewardEffect(tilePos, amount.ToString());
                
                _farms.Remove(tilePos);
            }
        }

        private Sprite GetCropSprite(FarmData farmData)
        {
            Sprite[] sprites = cropManager.GetCropSprites(farmData.GetCrop());
            if (farmData.GetProgress() >= .75f)
                return sprites[3];
            if (farmData.GetProgress() >= .5f)
                return sprites[2];
            if (farmData.GetProgress() >= .25f)
                return sprites[1];
            return sprites[0];
        }
    }
}