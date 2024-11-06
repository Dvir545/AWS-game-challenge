using UnityEngine;

namespace Utils
{
    public class SceneManagerScript : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        
        public void LoadSceneWithArgs(string sceneName, object args)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            // pass args to the next scene
            
        }
    }
}
