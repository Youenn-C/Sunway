using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Une Gare est un nœud du réseau. Elle peut connecter plusieurs tronçons de rails.
/// </summary>
public class Station : MonoBehaviour, IRailConnectable
{
    [Header("Connexions")]
    // Liste manuelle ou automatique des rails connectés.
    // Pour simplifier, on peut les remplir via un éditeur ou un script de construction.
    public List<SplineRail> connectedRails = new List<SplineRail>();

    public List<SplineRail> GetConnectedRails()
    {
        return connectedRails;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    // Méthode utilitaire pour ajouter une connexion proprement
    public void AddRailConnection(SplineRail rail, bool isRailStartConnectedToStation)
    {
        if (!connectedRails.Contains(rail))
        {
            connectedRails.Add(rail);
            // On informe aussi le rail qu'il est connecté à cette gare
            rail.SetConnection(this, isRailStartConnectedToStation);
        }
    }
}