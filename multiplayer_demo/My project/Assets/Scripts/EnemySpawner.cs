using System;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    private GameObject _enemyInstance;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        SpawnEnemy();
    }

    private void Update()
    {
        if (!IsServer) return;
        if (Input.GetKeyDown(KeyCode.K))
        {
            Destroy(_enemyInstance);
            // alternatively can use NetworkObject.Despawn, this will keep the object at the server
        }
    }

    private void SpawnEnemy()
    {
        _enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        NetworkObject networkObject = _enemyInstance.GetComponent<NetworkObject>();
        
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("Enemy prefab is missing NetworkObject component!");
        }
    }
}