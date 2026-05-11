using System;
using UnityEngine;
using UnityEngine.Splines;

public class MS_RailwayManager : MonoBehaviour
{
    [Header("References"), Space(5)]
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private int _splineNodesCount;

    private void Start()
    {
        if (_spline != null)
        {
            _splineNodesCount = _spline.Spline.Count;
        }
    }
}
