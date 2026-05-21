using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Représente un TRONÇON de rail unique (une spline à 2 points).
/// Dans cette architecture, chaque clic du joueur crée une NOUVELLE instance de ce script.
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

    /// <summary>
    /// Configure le générateur de mesh (Tube 3D).
    /// </summary>
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
    /// INITIALISATION D'UN SEGMENT À 2 POINTS.
    /// Appelé par le BuildManager pour créer un tronçon entre startPoint et endPoint.
    /// Gère automatiquement le lissage (Smooth) pour se connecter au segment précédent.
    /// </summary>
    public void InitializeSegment(Vector3 startPoint, Vector3 endPoint, SplineRail previousRail)
    {
        if (_splineContainer == null) return;

        // 1. Créer une nouvelle spline vide
        _splineContainer.AddSpline();
        Spline spline = _splineContainer.Splines[0];

        // 2. Ajouter le Point A (Départ)
        // On utilise AddKnot qui ajoute à la fin de la liste
        int indexA = spline.Count; 
        _splineContainer.AddKnot(0, startPoint);

        // 3. Ajouter le Point B (Arrivée)
        int indexB = spline.Count;
        _splineContainer.AddKnot(0, endPoint);

        // 4. CONFIGURATION DES TANGENTES (Le secret de la fluidité)
        
        // --- Pour le Point A (Départ) ---
        // Si on a un rail précédent, on aligne la tangente de départ avec la fin du précédent.
        if (previousRail != null)
        {
            Vector3 prevEndPos = previousRail.GetEndPointWorld();
            Vector3 prevTangent = previousRail.GetEndTangentWorld();
            
            // On définit la tangente "Entrante" du point A pour qu'elle soit opposée à la tangente de sortie du précédent
            // Cela assure une continuité parfaite (C1 continuity)
            _splineContainer.SetTangent(0, indexA, -prevTangent, SplineTangentMode.Continuous);
        }
        else
        {
            // Pas de précédent (début de ligne), on laisse Unity calculer ou on met une tangente par défaut (vers le point B)
            _splineContainer.SetTangentMode(0, indexA, SplineTangentMode.AutoSmooth);
        }

        // --- Pour le Point B (Arrivée) ---
        // On met en AutoSmooth pour que le prochain segment (s'il y en a un) puisse s'y connecter facilement,
        // ou pour que ce segment ait une courbe naturelle si le joueur tourne.
        _splineContainer.SetTangentMode(0, indexB, SplineTangentMode.AutoSmooth);

        // 5. Forcer la régénération du mesh
        _splineExtrude.Refresh();
    }

    // =============================================================================
    // MÉTHODES PUBLIQUES POUR LE TRAIN & LE RÉSEAU
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

        // Utilisation de SplineUtility avec conversion float3 -> Vector3
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
        if (_splineContainer.Splines.Count == 0) return null;
        return _splineContainer.Splines[0];
    }

    /// <summary>
    /// Retourne la position mondiale du point de FIN (Index 1 pour un segment à 2 points).
    /// </summary>
    public Vector3 GetEndPointWorld()
    {
        Spline spline = GetSpline();
        if (spline == null || spline.Count < 2) return transform.position;

        // Le point de fin est le dernier knot (Index = Count - 1)
        int lastIndex = spline.Count - 1;
        
        // Récupération via le Container (float3)
        float3 pointLocal = _splineContainer.GetPoint(0, lastIndex);
        Vector3 pointLocalVec = new Vector3(pointLocal.x, pointLocal.y, pointLocal.z);
        
        return transform.TransformPoint(pointLocalVec);
    }

    /// <summary>
    /// Retourne la tangente mondiale au point de FIN.
    /// Utile pour aligner le prochain segment.
    /// </summary>
    public Vector3 GetEndTangentWorld()
    {
        Spline spline = GetSpline();
        if (spline == null || spline.Count < 2) return transform.forward;

        int lastIndex = spline.Count - 1;
        
        // Récupération de la tangente sortante (TangentOut)
        float3 tangentLocal = _splineContainer.GetTangentOut(0, lastIndex);
        Vector3 tangentLocalVec = new Vector3(tangentLocal.x, tangentLocal.y, tangentLocal.z);
        
        // Transformation de la direction (pas de position, donc pas de TransformPoint, mais TransformDirection)
        return transform.TransformDirection(tangentLocalVec);
    }

    // =============================================================================
    // IMPLÉMENTATION INTERFACE IRailConnectable
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