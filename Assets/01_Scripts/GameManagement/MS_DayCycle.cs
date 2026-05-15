using System;
using Unity.Collections;
using UnityEngine;

public class MS_DayCycle : MonoBehaviour
{
    [Header("References"), Space(5)]
    [SerializeField] private Light _directionalLight;
    
    [Header("General Settings"),Space(5)]
    [SerializeField] private float _dayDuration;
    [SerializeField] private float _currentTime;
    private float _convertDuration;
    [Space(5)]
    [SerializeField] private bool _isDay;
    [SerializeField] private bool _isSunrise;
    [SerializeField] private bool _isSunset;
    [SerializeField] private bool _isNight;

    [Header("Day Settings"), Space(5)]
    [SerializeField] private int _dayLightIntensity;
    [SerializeField] private int _dayLightTemperature;
    [SerializeField] private int _dayLightIndirectMultiplier;
    
    [Header("Sunrise Settings"), Space(5)]
    [SerializeField] private int _sunriseLightIntensity;
    [SerializeField] private int _sunriseLightTemperature;
    [SerializeField] private int _sunriseLightIndirectMultiplier;
    
    [Header("Sunset Settings"), Space(5)]
    [SerializeField] private int _sunsetLightIntensity;
    [SerializeField] private int _sunsetLightTemperature;
    [SerializeField] private int _sunsetLightIndirectMultiplier;
    
    [Header("Night Settings"),Space(5)]
    [SerializeField] private int _nightLightIntensity;
    [SerializeField] private int _nightLightTemperature;
    [SerializeField] private int _nightLightIndirectMultiplier;

    private void Start()
    {
        _convertDuration =  _dayDuration * 60;
    }

    private void Update()
    {
        if (_currentTime < _convertDuration)
        {
            _convertDuration += Time.deltaTime;
            
            
            
        }
    }
}
