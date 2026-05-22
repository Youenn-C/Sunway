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
        ConfigureExtrude();
    }

    private void ConfigureExtrude()
    {
        if (_splineExtrude == null) return;
        
        _splineExtrude.spline = 0;
        _splineExtrude.shape = ExtrudeShape.Circle;
        _splineExtrude.circleRadius = tubeRadius;
        _splineExtrude.circleSegments = radialSegments;
        _splineExtrude.capStart = true;
        _splineExtrude.capEnd = true;
        
        if (railMaterial != null)
            _splineExtrude.material = railMaterial;
    }

    /// <summary>
    /// INITIALISE UN SEGMENT À 2 POINTS.
    /// </summary>
    public void InitializeSegment(Vector3 startPoint, Vector3 endPoint, SplineRail previousRail)
    {
        if (_splineContainer == null) return;

        _splineContainer.AddSpline();
        Spline spline = _splineContainer.Splines[0];

        // Point A (Départ)
        _splineContainer.AddKnot(0, startPoint);
        int indexA = 0;

        // Point B (Arrivée)
        _splineContainer.AddKnot(0, endPoint);
        int indexB = 1;

        // --- TANGENTES (Fluidité) ---
        
        if (previousRail != null)
        {
            Vector3 prevTangentWorld = previousRail.GetEndTangentWorld();
            Vector3 prevTangentLocal = transform.InverseTransformDirection(prevTangentWorld);
            float3 tangentIn = new float3(prevTangentLocal.x, prevTangentLocal.y, prevTangentLocal.z);

            _splineContainer.SetTangentIn(0, indexA, -tangentIn);
            _splineContainer.SetTangentMode(0, indexA, SplineTangentMode.Continuous);
        }
        else
        {
            _splineContainer.SetTangentMode(0, indexA, SplineTangentMode.AutoSmooth);
        }

        _splineContainer.SetTangentMode(0, indexB, SplineTangentMode.AutoSmooth);

        if(_splineExtrude != null) _splineExtrude.Refresh();
    }

    // =============================================================================
    // MÉTHODES POUR LE TRAIN
    // =============================================================================

    public void EvaluateAtDistance(float distance, out Vector3 position, out Vector3 tangent)
    {
        Spline spline = GetSpline();
        
        if (spline == null || spline.Count < 2)
        {
            position = transform.position;
            tangent = transform.forward;
            return;
        }

        float3 posMath, tanMath, normMath;
        SplineUtility.Evaluate(spline, distance, out posMath, out tanMath, out normMath);

        position = new Vector3(posMath.x, posMath.y, posMath.z);
        tangent = new Vector3(tanMath.x, tanMath.y, tanMath.z);
    }

    public float GetLength()
    {
        Spline spline = GetSpline();
        return spline != null ? spline.GetLength() : 0f;
    }

    public Spline GetSpline()
    {
        if (_splineContainer == null || _splineContainer.Splines.Count == 0) return null;
        return _splineContainer.Splines[0];
    }

    public Vector3 GetEndPointWorld()
    {
        Spline spline = GetSpline();
        if (spline == null || spline.Count < 2) return transform.position;

        int lastIndex = spline.Count - 1;
        float3 pointLocal = _splineContainer.GetPoint(0, lastIndex);
        Vector3 pointLocalVec = new Vector3(pointLocal.x, pointLocal.y, pointLocal.z);
        
        return transform.TransformPoint(pointLocalVec);
    }
    
    // NOUVELLE MÉTHODE AJOUTÉE POUR LE MANAGER
    public Vector3 GetStartPointWorld()
    {
        Spline spline = GetSpline();
        if (spline == null || spline.Count < 1) return transform.position;

        float3 pointLocal = _splineContainer.GetPoint(0, 0);
        Vector3 pointLocalVec = new Vector3(pointLocal.x, pointLocal.y, pointLocal.z);
        
        return transform.TransformPoint(pointLocalVec);
    }

    public Vector3 GetEndTangentWorld()
    {
        Spline spline = GetSpline();
        if (spline == null || spline.Count < 2) return transform.forward;

        int lastIndex = spline.Count - 1;
        float3 tangentLocal = _splineContainer.GetTangentOut(0, lastIndex);
        Vector3 tangentLocalVec = new Vector3(tangentLocal.x, tangentLocal.y, tangentLocal.z);
        
        return transform.TransformDirection(tangentLocalVec);
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

    public void SetConnection(IRailConnectable connectable, bool isStart)
    {
        if (isStart) startConnection = connectable;
        else endConnection = connectable;
    }
}