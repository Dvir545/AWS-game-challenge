using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPopUpBehaviour : MonoBehaviour
{
    public void ClosePopUp()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
