using System;
using UnityEngine;

namespace UI
{
    public class TutorialBehaviour: MonoBehaviour
    {
        private GameObject _window;
        [SerializeField] private GameObject mainMenu;

        private void Awake()
        {
            _window = transform.GetChild(0).gameObject;
        }

        public void OpenTutorial()
        {
            _window.SetActive(true);
            mainMenu.SetActive(false);
        }
    }
}