using System;
using System.Collections.Generic;
using Player;
using UI.WarningSign;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using Utils.Data;

namespace Crops
{
    public class FarmingManager: MonoBehaviour
    {
        private Tilemap _canFarmTilemap;
        private Tilemap _farmTilemap;
        [SerializeField] private TileBase farmTile;
        [SerializeField] private CropManager cropManager;
        [SerializeField] private GameObject cropsParent;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerData playerData;
        [SerializeField] private ProgressBarBehavior progressBarBehavior;
        [SerializeField] private EffectsManager effectsManager;
        
        [Header("Crop Prefabs")]
        [SerializeField] private GameObject wheatPrefab;
        [SerializeField] private GameObject carrotPrefab;
        [SerializeField] private GameObject tomatoPrefab;
        [SerializeField] private GameObject cornPrefab;
        [SerializeField] private GameObject pumpkinPrefab;
        
        private Vector2 _cropInstantiationOffset = new Vector2(.5f, 0.4f);
        
        public Dictionary<Vector3Int, CropBehaviour> Farms { get; private set; }= new();
        
        public bool IsFarming { get; private set; }
        
        private void Awake()
        {
            _canFarmTilemap = GameObject.FindGameObjectWithTag("canFarmTilemap").GetComponent<Tilemap>();
            _farmTilemap = GameObject.FindGameObjectWithTag("farmTilemap").GetComponent<Tilemap>();
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
            Vector3Int tilePos = _canFarmTilemap.WorldToCell(t.position);
            return new Tuple<bool, Vector3Int>(_farmTilemap.GetTile(tilePos) != null, tilePos);
        }

        private void Farm()
        {
            bool isStandingOnFarmTile;
            Vector3Int tilePos;
            (isStandingOnFarmTile, tilePos) = IsStandingOnFarmTile(playerTransform);
            bool canFarm = _canFarmTilemap.GetTile(tilePos) != null && cropManager.HasCrops();
            if (canFarm && !isStandingOnFarmTile)
            {
                // Start farming on this tile
                if (!Farms.ContainsKey(tilePos))
                {
                    _farmTilemap.SetTile(tilePos, farmTile);
                    Vector3 cropPos = tilePos + new Vector3(_cropInstantiationOffset.x, _cropInstantiationOffset.y, 0f);
                    var bestAvailableCrop = cropManager.GetBestAvailableCrop();
                    GameObject cropSpritePrefab = bestAvailableCrop switch
                    {
                        Crop.Wheat => wheatPrefab,
                        Crop.Carrot => carrotPrefab,
                        Crop.Tomato => tomatoPrefab,
                        Crop.Corn => cornPrefab,
                        Crop.Pumpkin => pumpkinPrefab,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    GameObject crop = Instantiate(cropSpritePrefab, cropPos, Quaternion.identity);
                    crop.transform.SetParent(cropsParent.transform);
                    Farms[tilePos] = crop.GetComponent<CropBehaviour>();
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
                
            // Check if farming is complete
            if (Farms[tilePos].GetProgress() >= 1f)
            {
                progressBarBehavior.StopWork();
                // Remove the tile and its progress
                _farmTilemap.SetTile(tilePos, null);
                var cropTransform = Farms[tilePos].transform;
                EventManager.Instance.TriggerEvent(EventManager.CropHarvested, cropTransform);
                Destroy(cropTransform.gameObject);
                // Reward player with the crop sell price
                int amount = CropsData.Instance.GetSellPrice(Farms[tilePos].GetCrop());
                playerData.AddCash(amount);
                effectsManager.FloatingTextEffect(tilePos, 1, 1, amount.ToString() + "$", Constants.CashColor);
                
                Farms.Remove(tilePos);
            }
        }
        
        // Returns true if the crop was destroyed
        public bool IncDestroyProgress(Vector3Int tilePos)
        {
            if (Farms.ContainsKey(tilePos))
            {
                Farms[tilePos].AddToDestroyProgress(Time.deltaTime / Constants.TimeToEatCrop);
                if (Farms[tilePos].GetDestroyProgress() >= 1f)
                {
                    _farmTilemap.SetTile(tilePos, null);
                    WarningSignPool.Instance.ReleaseWarningSign(Farms[tilePos].transform);
                    Destroy(Farms[tilePos].gameObject);
                    Farms.Remove(tilePos);
                    return true;
                }
            }
            return false;
        }
    }
}