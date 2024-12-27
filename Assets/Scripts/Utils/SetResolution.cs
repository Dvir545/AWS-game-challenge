using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetResolution : MonoBehaviour
{
    private const float Width = 1920f;
    private const float Height = 1080f;
    
    private const float LandscapeRatio =  Width / Height;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Resolution, width: " + Screen.width + ", height: " + Screen.height);

        // Get the real ratio
        float ratio = (float)Screen.width / (float)Screen.height;
        Debug.Log("Ratio: " + ratio);

        // Cammera settings to landscape
        if (ratio >= LandscapeRatio)
        {
            Camera.main.orthographicSize = Height/ 200f;
        }
        else
        {
            float scaledHeight = Width / ratio;
            Camera.main.orthographicSize = scaledHeight / 200f;
        }
    }
}