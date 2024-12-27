using UnityEngine;

namespace Utils
{
    public class FullScreenHelper: MonoBehaviour
    {
        private bool _isFullScreen;

        private void Awake()
        {
            _isFullScreen = Screen.fullScreen;
            if (_isFullScreen)
                WebGLSupport.WebGLWindow.SwitchFullscreen();
        }

        private void Update()
        {
            if (_isFullScreen != Screen.fullScreen)
            {
                WebGLSupport.WebGLWindow.SwitchFullscreen();
                _isFullScreen = Screen.fullScreen;
            }
        }
    }
}