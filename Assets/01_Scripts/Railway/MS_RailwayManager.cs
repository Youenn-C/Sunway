using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using UnityEngine.Splines;

public class MS_RailwayManager : MonoBehaviour
{
    [Header("References"), Space(5)]
    [SerializeField] private SplineContainer _splineContainer;
    [SerializeField] private int _splineNodesCount;
    [SerializeField] private Vector3 newPosition;

    private void Start()
    {
        if (_splineContainer != null)
        {
            _splineNodesCount = _splineContainer.Spline.Count;
        }
        
        StartCoroutine(Auto_Build());
    }
    
    public void AddPoint()
    {

        newPosition += new Vector3(0, 0, 15);
        var spline = _splineContainer.Splines[0];

        // Créez un nouveau nœud (BezierKnot)
        // La position doit être en float3 (Unity.Mathematics)
        BezierKnot newKnot = new BezierKnot
        {
            Position = newPosition, 
            Rotation = quaternion.identity, // Rotation par défaut
            //Scale = 1f // Échelle par défaut
        };

        // Ajoutez le nœud à la spline
        spline.Insert(spline.Count, newKnot);
        
        // Optionnel : Notifier que la spline a changé (souvent automatique, mais utile pour forcer le refresh)
        //_splineContainer.NotifySplineChanged();
    }

    IEnumerator Auto_Build()
    {
        while (true)
        {
            AddPoint();
        
            yield return new WaitForSeconds(2.5f);
        }
    }
}
