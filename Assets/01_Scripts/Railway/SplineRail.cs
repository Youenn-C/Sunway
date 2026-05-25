using UnityEngine;
using UnityEngine.Splines; // <--- CORRECTION ICI
using Unity.Mathematics;   // Toujours nécessaire pour float3
using System.Collections.Generic;

/// <summary>
/// Représente un TRONÇON de rail unique (une spline à 2 points).
/// </summary>
[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(SplineExtrude))]

public class SplineRail : MonoBehaviour, IRailConnectable
{
    [Header("Connexions")]
    public IRailConnectable startConnection;
    public IRailConnectable endConnection;

    [Header("Configuration Visuelle")]
    public float tubeRadius = 0.5f;
    [Range(3, 32)] public int radialSegments = 12;
    public Material railMaterial;

    private SplineContainer _splineContainer;
    private SplineExtrude _splineExtrude;

    private void Awake()
    {
        _splineContainer = GetComponent<SplineContainer>();
        _splineExtrude = GetComponent<SplineExtrude>();
    }

    // =============================================================================
    // INTERFACE
    // =============================================================================

    public List<SplineRail> GetConnectedRails()
    {
        List<SplineRail> connected = new List<SplineRail>();
        if (endConnection is SplineRail next) connected.Add(next);
        if (startConnection is SplineRail prev) connected.Add(prev);
        return connected;
    }

    public Vector3 GetPosition() => transform.position;
}