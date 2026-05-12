using UnityEngine;

public class MS_BuildMod_Manager : MonoBehaviour
{
    [Header("Variables"), Space(5)]
    [Range(0,50)] public float buildModRange;
    [Space(5)]
    public bool buildModOn = false;
    public bool freeBuildOn = true;
    
    [Header("Raycast Parameters"), Space(5)] 
    [SerializeField] private Vector3 hitLocation;
    [SerializeField] private Transform raycastOrigin;
    private RaycastHit customHit; 
    
    private void FixedUpdate()
    {
        if (buildModOn)
        {
            Physics.Raycast(raycastOrigin.position, raycastOrigin.rotation * Vector3.forward, out customHit, buildModRange);
            
            //Debug.DrawRay(raycastOrigin.position, raycastOrigin.rotation * Vector3.forward * buildModRange, Color.magenta);
            
            if (customHit.collider != null)
            {
                if (freeBuildOn)
                {
                    hitLocation = new Vector3(customHit.point.x, customHit.point.y, customHit.point.z);
                }
                else
                {
                    hitLocation = (new Vector3(Mathf.Round(customHit.point.x), Mathf.Round(customHit.point.y), Mathf.Round(customHit.point.z))) * MS_GameManager.Instance.gridSize;
                }
                
                OnDrawGizmos();
            }
        }
    }
    
    [Space(25)]
    [Header("=== Sphere debug parameters ==================================="), Space(10)]
    
    public Color color = Color.red;
    public float radius = 1.0f;

    // Dessine la sphère même si l'objet n'est pas sélectionné
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        // Dessine une sphère pleine filaire
        Gizmos.DrawWireSphere(hitLocation, radius);

        // Alternative pour une sphère uniquement visible quand l'objet est sélectionné :
        // Utiliser OnDrawGizmosSelected() à la place
    }
}
