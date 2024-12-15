using System;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Towers
{
    public class TowerBaseBehaviour: MonoBehaviour
    {
        private TowerBuild _towerBuild;

        private void Awake()
        {
            _towerBuild = transform.parent.GetComponent<TowerBuild>();
        }

        private void OnBecameInvisible()
        {
            _towerBuild.ShowWarningSign();
        }

        private void OnBecameVisible()
        {
            _towerBuild.HideWarningSign();
        }
    }
}