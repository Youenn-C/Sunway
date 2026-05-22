using UnityEngine;
using System.Collections.Generic;

public class Station : MonoBehaviour, IRailConnectable
{
    public List<SplineRail> connectedRails = new List<SplineRail>();

    public List<SplineRail> GetConnectedRails() => connectedRails;
    public Vector3 GetPosition() => transform.position;

    public void AddRailConnection(SplineRail rail, bool isRailStartConnectedToStation)
    {
        if (!connectedRails.Contains(rail))
        {
            connectedRails.Add(rail);
            rail.SetConnection(this, isRailStartConnectedToStation);
            Debug.Log($"Connexion : Gare {this.name} <-> Rail {rail.name}");
        }
    }
}