using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TowerBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _underConstruction;  // The object that will be shown when the tower is under construction
    [SerializeField] private GameObject _floor1;  // The object that will be shown when the tower is at level 1
    [SerializeField] private GameObject _floor2;  // The object that will be shown when the tower is at level 2
    [SerializeField] private GameObject _floor3;  // The object that will be shown when the tower is at level 3

    private Tower _towerType;  // The type of the tower
    private int _currentLevel = 0;  // The current level of the tower
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        // attack the enemy
    }
}
