using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace UI
{
    public class TutorialBehaviour: Singleton<TutorialBehaviour>
    {
        private GameObject _window;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject[] tutorialPages;
        [SerializeField] private GameObject controlsMobile;
        [SerializeField] private GameObject controlsPC;

        public bool wasOpened;

        private void Awake()
        {
            _window = transform.GetChild(0).gameObject;
            if (MobileKeyboardManager.Instance.IsMobileDevice())
            {
                controlsMobile.SetActive(true);
                controlsPC.SetActive(false);
            }
            else
            {
                controlsMobile.SetActive(false);
                controlsPC.SetActive(true);
            }
        }

        public void OpenTutorial()
        {
            if (_window == null)
                Awake();
            _window.SetActive(true);
            mainMenu.SetActive(false);
            tutorialPages[0].SetActive(true);
            wasOpened = true;
        }
        
        public void NextPage()
        {
            for (int i = 0; i < tutorialPages.Length; i++)
            {
                if (tutorialPages[i].activeSelf)
                {
                    tutorialPages[i].SetActive(false);
                    if (i == tutorialPages.Length - 1)
                        tutorialPages[0].SetActive(true);
                    else
                        tutorialPages[i + 1].SetActive(true);
                    return;
                }
            }
        }
        
        public void PrevPage()
        {
            for (int i = 0; i < tutorialPages.Length; i++)
            {
                if (tutorialPages[i].activeSelf)
                {
                    tutorialPages[i].SetActive(false);
                    if (i == 0)
                        tutorialPages[tutorialPages.Length - 1].SetActive(true);
                    else
                        tutorialPages[i - 1].SetActive(true);
                    return;
                }
            }
        }
    }
}