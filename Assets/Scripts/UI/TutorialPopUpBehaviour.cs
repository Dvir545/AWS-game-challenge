using UnityEngine;

namespace UI
{
    public class TutorialPopUpBehaviour : MonoBehaviour
    {
        public void ClosePopUp()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}
