using Unity.Cinemachine;
using UnityEngine;

namespace Utils
{
    public class CameraShake : MonoBehaviour
    {
        private CinemachineCamera _camera;
        private CinemachineBasicMultiChannelPerlin _noise;
        private float _shakeTimer;
        private float _shakeTimerTotal;
        private float _startingAmplitude;
        private float _startingFrequency;
        
        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
            _noise = _camera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
            
            EventManager.Instance.StartListening(EventManager.PlayerGotHit, OnPlayerGotHit);
            EventManager.Instance.StartListening(EventManager.PlayerHitEnemy, OnPlayerHitEnemy);
        }

        private void OnPlayerHitEnemy(object arg0)
        {
            ShakeCamera(.5f, 4f, 5f);
        }

        private void OnPlayerGotHit(object arg0)
        {
            ShakeCamera(.5f, 6f, 10f);
        }

        private void Update()
        {
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;
                
                float currentAmplitude = Mathf.Lerp(_startingAmplitude, 0f, 1 - (_shakeTimer / _shakeTimerTotal));
                float currentFrequency = Mathf.Lerp(_startingFrequency, 0f, 1 - (_shakeTimer / _shakeTimerTotal));
                
                _noise.AmplitudeGain = currentAmplitude;
                _noise.FrequencyGain = currentFrequency;
            }
        }

        private void ShakeCamera(float duration, float amplitude, float frequency)
        {
            _shakeTimer = duration;
            _shakeTimerTotal = duration;
            _startingAmplitude = amplitude;
            _startingFrequency = frequency;
            
            _noise.AmplitudeGain = amplitude;
            _noise.FrequencyGain = frequency;
        }
    }
}