using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Utils.Data;

namespace Player
{
    public class PetsManager: Singleton<PetsManager>
    {
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private GameObject petsParent;
        [SerializeField] private PlayerData playerData;
        
        private List<PetBehaviour> _slimes;


        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.AbilityUpgraded, ChangeSpeed);
        }

        public void Init()
        {
            _slimes = new List<PetBehaviour>();
            foreach (var pet in GameData.Instance.pets)
            {
                if ((Pet)pet.petType == Pet.Slime)
                {
                    AddSlimePet(pet.petIndex);
                }
            }
        }

        public void Reset()
        {
            foreach (var slime in _slimes)
            {
                Destroy(slime.gameObject);
            }
            _slimes.Clear();
        }

        private void ChangeSpeed(object arg0)
        {
            if (arg0 is (Upgrade upgrade, int level))
            {
                if (upgrade == Upgrade.Speed)
                {
                    UpdateSpeed();
                }
            }
        }
        
        public void AddPet(Pet pet, int index)
        {
            if (pet == Pet.Slime)
            {
                AddSlimePet(index);
            }
        }

        public void AddSlimePet(int index)
        {
            var slime = Instantiate(slimePrefab, transform);
            slime.transform.SetParent(petsParent.transform);
            slime.transform.position = transform.position;
            var petBehaviour = slime.GetComponent<PetBehaviour>();
            // set target to last slime in list, or player if list is empty
            Transform dest;
            if (_slimes.Count > 0)
            {
                dest = _slimes[^1].transform;
            }
            else
            {
                dest = playerMovement.transform;
            }
            petBehaviour.Init(Pet.Slime, index, playerMovement, playerData.SpeedMultiplier, dest);
            _slimes.Add(petBehaviour);
        }

        public void UpdateSpeed()
        {
            for (int i = 0; i < _slimes.Count; i++)
            {
                _slimes[i].UpdateSpeed();
            }
        }
    }
}