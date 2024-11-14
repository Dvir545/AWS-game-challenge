using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyMovement : NetworkBehaviour
{
    [SerializeField] private float radius = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    private Vector3 centerPoint;
    private float angle;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            centerPoint = transform.position;
            angle = 0f;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            // Calculate new position using parametric equations of a circle
            angle += rotationSpeed * Time.deltaTime;
            float newX = centerPoint.x + radius * Mathf.Cos(angle);
            float newY = centerPoint.y;
            float newZ = centerPoint.z + radius * Mathf.Sin(angle);
            
            transform.position = new Vector3(newX, newY, newZ);
        }
    }
}

