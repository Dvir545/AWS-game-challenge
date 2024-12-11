using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using Utils.Data;

namespace Crops
{
    public class FarmingManager: MonoBehaviour
    {
        private Tilemap canFarmTilemap;
        private Tilemap farmTilemap;
        [SerializeField] private TileBase farmTile;
        [SerializeField] private CropManager cropManager;
        [SerializeField] private GameObject cropSpritePrefab;
        [SerializeField] private GameObject cropsParent;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        [SerializeField] private EffectsManager effectsManager;
        
        private Vector2 _cropInstantiationOffset = new Vector2(.5f, 0.4f);
        
        public Dictionary<Vector3Int, FarmData> Farms { get; private set; }= new();
        
        public bool IsFarming { get; private set; }
        
        private void Awake()
        {
            canFarmTilemap = GameObject.FindGameObjectWithTag("canFarmTilemap").GetComponent<Tilemap>();
            farmTilemap = GameObject.FindGameObjectWithTag("farmTilemap").GetComponent<Tilemap>();
        }

        public class FarmData
        {
            private Crop _crop;
            private float _progress;
            private float _destroyProgress;
            private SpriteRenderer _cropSprite;
            
            public FarmData(Crop crop, GameObject curSpriteObject, float progress)
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
            
            public float GetDestroyProgress()
            {
                return _destroyProgress;
            }
            
            public void AddToDestroyProgress(float progress)
            {
                _destroyProgress += progress;
            }
            
            public SpriteRenderer GetCropSpriteRenderer()
            {
                return _cropSprite;
            }
        }

        public void StartFarming()
        {
            IsFarming = true;
        }
        
        public void StopFarming()
        {
            IsFarming = false;
        }

        private void Update()
        {
            if (IsFarming)
            {
                Farm();
            }
        }
        
        public Tuple<bool, Vector3Int> IsStandingOnFarmTile(Transform t)
        {
            Vector3Int tilePos = canFarmTilemap.WorldToCell(t.position);
            return new Tuple<bool, Vector3Int>(farmTilemap.GetTile(tilePos) != null, tilePos);
        }

        private void Farm()
        {
            bool isStandingOnFarmTile;
            Vector3Int tilePos;
            (isStandingOnFarmTile, tilePos) = IsStandingOnFarmTile(playerTransform);
            bool canFarm = canFarmTilemap.GetTile(tilePos) != null && cropManager.HasCrops();
            if (canFarm && !isStandingOnFarmTile)
            {
                // Start farming on this tile
                if (!Farms.ContainsKey(tilePos))
                {
                    farmTilemap.SetTile(tilePos, farmTile);
                    Vector3 cropPos = tilePos + new Vector3(_cropInstantiationOffset.x, _cropInstantiationOffset.y, 0f);
                    GameObject cropSprite = Instantiate(cropSpritePrefab, cropPos, Quaternion.identity);
                    cropSprite.transform.SetParent(cropsParent.transform);
                    Farms[tilePos] = new FarmData(cropManager.GetBestAvailableCrop(), cropSprite, 0f);
                    cropManager.RemoveCrop(Farms[tilePos].GetCrop());
                }
                progressBarBehavior.StartWork(Farms[tilePos].GetProgress());
            }

            // If we're on a tile that's being farmed, increase its progress
            if (Farms.ContainsKey(tilePos))
            {
                HandleFarm(tilePos);
            }
            else  // check if there's an adjacent tile to farm
            {
                bool foundAdjacentFarm = false;
                foreach (Vector3Int adjacentTilePos in tilePos.GetAdjacentTiles(playerMovement.GetFacingDirection()))
                {
                    if (Farms.ContainsKey(adjacentTilePos))
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
            progressBarBehavior.StartWork(Farms[tilePos].GetProgress());
            Farms[tilePos].AddToProgress(playerData.GetProgressSpeedMultiplier * Time.deltaTime 
                                          / CropsData.Instance.GetGrowthTime(Farms[tilePos].GetCrop()));
            progressBarBehavior.UpdateProgress(Farms[tilePos].GetProgress());
            Sprite  cropSprite = GetCropSprite(Farms[tilePos]);
            if (cropSprite != null)
            {
                Farms[tilePos].GetCropSpriteRenderer().sprite = cropSprite;
            }
                
            // Check if farming is complete
            if (Farms[tilePos].GetProgress() >= 1f)
            {
                progressBarBehavior.StopWork();
                // Remove the tile and its progress
                farmTilemap.SetTile(tilePos, null);
                Destroy(Farms[tilePos].GetCropSpriteRenderer().gameObject);
                EventManager.Instance.TriggerEvent(EventManager.CropHarvested, null);
                // Reward player with the crop sell price
                int amount = CropsData.Instance.GetSellPrice(Farms[tilePos].GetCrop());
                playerData.AddCash(amount);
                effectsManager.FloatingTextEffect(tilePos, 1, 1, amount.ToString() + "$", Constants.CashColor);
                
                Farms.Remove(tilePos);
            }
        }

        private Sprite GetCropSprite(FarmData farmData)
        {
            return CropsData.Instance.GetSprite(farmData.GetCrop(), farmData.GetProgress());
        }
        
        // Returns true if the crop was destroyed
        public bool IncDestroyProgress(Vector3Int tilePos)
        {
            if (Farms.ContainsKey(tilePos))
            {
                Farms[tilePos].AddToDestroyProgress(Time.deltaTime / Constants.TimeToEatCrop);
                if (Farms[tilePos].GetDestroyProgress() >= 1f)
                {
                    farmTilemap.SetTile(tilePos, null);
                    Destroy(Farms[tilePos].GetCropSpriteRenderer().gameObject);
                    Farms.Remove(tilePos);
                    return true;
                }
            }
            return false;
        }
    }
}