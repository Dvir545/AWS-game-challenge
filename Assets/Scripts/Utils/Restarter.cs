using UnityEngine;

namespace Utils
{
    public class Restarter : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
