using UnityEngine;

namespace UI
{
    public class PopupScreenBehaviour: MonoBehaviour
    {
        [SerializeField] private GameObject toDisable;
        
        public void OpenPopup()
        {
            gameObject.SetActive(true);
            if (toDisable != null)
            {
                toDisable.SetActive(false);
            }
        }
        
        public void ClosePopup()
        {
            gameObject.SetActive(false);
            if (toDisable != null)
            {
                toDisable.SetActive(true);
            }
        }
        
    }
}