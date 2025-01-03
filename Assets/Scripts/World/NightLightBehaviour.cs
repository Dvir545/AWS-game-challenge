using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utils;
using World;

public class NightLightBehaviour : MonoBehaviour
{
    private Light2D _light;
    private float _intensity;
    private Light2D _globalLight2D;
    
    private void Awake()
    {
        _light = GetComponent<Light2D>();
        _intensity = _light.intensity;
        _light.intensity = 0f;
        _globalLight2D = GameObject.FindWithTag("GlobalLight").GetComponent<Light2D>();
    }

    private void Update()
    {
        if (!GameStarter.Instance.GameStarted)
        {
            return;
        }
        if (!DayNightManager.Instance.DayTime && !DayNightManager.Instance.NightTime)
        {
            _light.intensity = 1 - _globalLight2D.intensity;
        }
        else if (DayNightManager.Instance.NightTime && Math.Abs(_light.intensity - _intensity) > 0.01f)
        {
            _light.intensity = _intensity;
        }
        else if (DayNightManager.Instance.DayTime && _light.intensity != 0f)
        {
            _light.intensity = 0f;
        }
    }
}
