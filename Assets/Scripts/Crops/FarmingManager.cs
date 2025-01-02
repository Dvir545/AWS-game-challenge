using System;
using System.Collections.Generic;
using Player;
using UI.GameUI;
using UI.WarningSign;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;
using Utils.Data;
using World;

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
        
        private readonly Vector2 _cropInstantiationOffset = new Vector2(.5f, 0.4f);

        public Dictionary<Vector2Int, CropBehaviour> Farms { get; private set; } =
            new Dictionary<Vector2Int, CropBehaviour>();
        public Dictionary<Vector2Int, PlantedCropInfo> PlantedCrops => GameData.Instance.plantedCrops;
        
        public bool IsFarming { get; private set; }

        private void Awake()
        {
            EventManager.Instance.StartListening(EventManager.CropHarvested, OnCropHarvest);
        }

        public void Init()
        {
            if (_canFarmTilemap == null)
            {
                _canFarmTilemap = GameObject.FindGameObjectWithTag("canFarmTilemap").GetComponent<Tilemap>();
                _farmTilemap = GameObject.FindGameObjectWithTag("farmTilemap").GetComponent<Tilemap>();
            }
            // add existing crops to Farms
            foreach (var tilePos in PlantedCrops.Keys)
            {
                PlantCrop(tilePos, (Crop)PlantedCrops[tilePos].cropType, PlantedCrops[tilePos].stage,
                    PlantedCrops[tilePos].plantProgress, PlantedCrops[tilePos].destroyProgress);
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

        public Tuple<bool, Vector2Int> IsStandingOnEdibleFarmTile(Transform t)
        {
            bool isStandingOnFarmTile;
            Vector2Int tilePos;
            (isStandingOnFarmTile, tilePos) = IsStandingOnFarmTile(t);
            if (isStandingOnFarmTile && Farms[tilePos].IsEdible())
            {
                return new Tuple<bool, Vector2Int>(true, tilePos);
            }
            else
            {
                return new Tuple<bool, Vector2Int>(false, tilePos);
            }
        }
        
        private Tuple<bool, Vector2Int> IsStandingOnFarmTile(Transform t)
        {
            Vector2Int tilePos = (Vector2Int)_canFarmTilemap.WorldToCell(t.position);
            return new Tuple<bool, Vector2Int>(_farmTilemap.GetTile((Vector3Int)tilePos) != null, tilePos);
        }

        private void PlantCrop(Vector2Int tilePos, Crop cropType, CropStage stage, float plantProgress=0f, float destroyProgress=0f)
        {
            _farmTilemap.SetTile((Vector3Int)tilePos, farmTile);
            Vector2 cropPos = tilePos + new Vector2(_cropInstantiationOffset.x, _cropInstantiationOffset.y);
            GameObject cropSpritePrefab = cropType switch
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
            Farms[tilePos].Init(tilePos, cropType, stage, plantProgress, destroyProgress);
        }

        private void PlantBestCrop(Vector2Int tilePos)
        {
            var bestAvailableCrop = cropManager.GetBestAvailableCrop();
            PlantCrop(tilePos, bestAvailableCrop, CropStage.Planted);
            cropManager.RemoveCrop(Farms[tilePos].GetCrop());
        }

        private void OnCropHarvest(object arg0)
        {
            if (arg0 is Transform cropTransform)
            {
                Vector2Int tilePos = (Vector2Int)_farmTilemap.WorldToCell(cropTransform.position);
                // Remove the tile and its progress
                _farmTilemap.SetTile((Vector3Int)tilePos, null);
                // Reward player with the crop sell price
                int amount = CropsData.Instance.GetSellPrice(Farms[tilePos].GetCrop());
                playerData.AddCash(amount);
                effectsManager.FloatingTextEffect(tilePos, 1, 1, amount.ToString() + "$", Constants.CashColor);
                Farms.Remove(tilePos);
                PlantedCrops.Remove(tilePos);
            }
        }

        private void DestroyCrop(Vector2Int tilePos)
        {
            SoundManager.Instance.FarmDestroyed();
            _farmTilemap.SetTile((Vector3Int)tilePos, null);
            WarningSignPool.Instance.ReleaseWarningSign(Farms[tilePos].transform);
            Destroy(Farms[tilePos].gameObject);
            Farms.Remove(tilePos);
            PlantedCrops.Remove(tilePos);
        }

        private void Farm()
        {
            if (DayNightManager.Instance.NightTime) return;
            bool isStandingOnFarmTile;
            Vector2Int tilePos;
            (isStandingOnFarmTile, tilePos) = IsStandingOnFarmTile(playerTransform);
            bool canFarm = _canFarmTilemap.GetTile((Vector3Int)tilePos) != null && cropManager.HasCrops();
            if (canFarm && !isStandingOnFarmTile)
            {
                // Start farming on this tile
                if (!PlantedCrops.ContainsKey(tilePos))
                {
                    PlantBestCrop(tilePos);
                }
                progressBarBehavior.StartWork(Farms[tilePos].GetPlantProgress());
            }

            // If we're on a tile that's being planted, increase its progress
            if (Farms.ContainsKey(tilePos) && Farms[tilePos].Stage == CropStage.Planted)
            {
                IncFarmPlantProgress(tilePos);
            }
            else  // check if there's an adjacent tile to plant
            {
                bool foundAdjacentFarm = false;
                foreach (Vector2Int adjacentTilePos in tilePos.GetAdjacentTiles(playerMovement.GetFacingDirection()))
                {
                    if (Farms.ContainsKey(adjacentTilePos) && Farms[adjacentTilePos].Stage == CropStage.Planted)
                    {
                        IncFarmPlantProgress(adjacentTilePos);
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


        private void IncFarmPlantProgress(Vector2Int tilePos)
        {
            var plantProgress = Farms[tilePos].GetPlantProgress();
            progressBarBehavior.StartWork(plantProgress);
            var isPlantingComplete = Farms[tilePos].AddToPlantProgress(playerData.GetProgressSpeedMultiplier * Time.deltaTime 
                                          / CropsData.Instance.GetPlantTime(Farms[tilePos].GetCrop()));
            progressBarBehavior.UpdateProgress(plantProgress);
                
            // Check if planting is complete
            if (isPlantingComplete)
            {
                progressBarBehavior.StopWork();
            }
        }
        
        // Returns true if the crop was destroyed
        public bool IncDestroyProgress(Vector2Int tilePos)
        {
            if (Farms.ContainsKey(tilePos))
            {
                Farms[tilePos].AddToDestroyProgress(Time.deltaTime / CropsData.Instance.GetDestroyTime(Farms[tilePos].GetCrop()));
                if (Farms[tilePos].GetDestroyProgress() >= 1f)
                {
                    DestroyCrop(tilePos);
                    return true;
                }
            }
            return false;
        }

        public void ResetCrops()
        {
            foreach (var tilePos in Farms.Keys)
            {
                _farmTilemap.SetTile((Vector3Int)tilePos, null);
                Destroy(Farms[tilePos].gameObject);
            }
            Farms.Clear();
        }

        public void StartNight()
        {
            // convert all Seed crops to Sprout
            foreach (var tilePos in Farms.Keys)
            {
                if (Farms[tilePos].Stage == CropStage.Seed)
                {
                    Farms[tilePos].SetStage(CropStage.Sprout);
                }
            }
        }

        public void HalfNight()
        {
            // convert all Sprout crops to Mature
            foreach (var tilePos in Farms.Keys)
            {
                if (Farms[tilePos].Stage == CropStage.Sprout)
                {
                    Farms[tilePos].SetStage(CropStage.Mature);
                }
            }
        }

        public void StartDay()
        {
            // convert all Mature crops to Ripe
            foreach (var tilePos in Farms.Keys)
            {
                if (Farms[tilePos].Stage == CropStage.Mature)
                {
                    Farms[tilePos].SetStage(CropStage.Ripe);
                }
            }
        }
    }
}