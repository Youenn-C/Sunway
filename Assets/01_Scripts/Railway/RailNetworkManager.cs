using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

/// <summary>
/// Gestionnaire global optionnel. Utile pour :
/// 1. Enregistrer tous les rails et gares créés.
/// 2. Aider le joueur à connecter automatiquement les extrémités proches (Snap).
/// 3. Lancer des trains automatiquement sur le réseau.
/// </summary>
public class RailNetworkManager : MonoBehaviour
{
    public static RailNetworkManager Instance;
    
    public List<Station> allStations = new List<Station>();
    public List<SplineRail> allRails = new List<SplineRail>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Called by the Builder when a rail is finished.
    /// Tries to auto-connect the rail ends to nearby stations.
    /// </summary>
    public void RegisterRail(SplineRail newRail)
    {
        allRails.Add(newRail);
        TryAutoConnect(newRail);
    }

    void TryAutoConnect(SplineRail rail)
    {
        float snapDistance = 2.0f;
        Spline spline = rail.GetSpline();
        
        if (spline == null || spline.Count < 2) return;

        // --- DÉBUT (Index 0) ---
        // On demande au rail de nous donner la position monde du knot 0
        Vector3 startPos = rail.GetKnotWorldPosition(0);
        TryConnectPoint(rail, startPos, true, snapDistance);

        // --- FIN (Dernier Index) ---
        int lastIndex = spline.Count - 1;
        Vector3 endPos = rail.GetKnotWorldPosition(lastIndex);
        TryConnectPoint(rail, endPos, false, snapDistance);
    }

    void TryConnectPoint(SplineRail rail, Vector3 pointPos, bool isStart, float threshold)
    {
        foreach (var station in allStations)
        {
            if (Vector3.Distance(pointPos, station.transform.position) < threshold)
            {
                // Connexion trouvée !
                station.AddRailConnection(rail, isStart);
                Debug.Log($"Rail connecté à la gare {station.name}");
                break; // On s'arrête à la première gare trouvée
            }
        }
        
        // Ici, on pourrait aussi chercher d'autres rails pour faire des jonctions directes (aiguillages)
    }
}