using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MS_BuildMod_Manager : MonoBehaviour
{
    public static MS_BuildMod_Manager Instance;
    
    [Header("References"), Space(5)]
    [SerializeField] private List<GameObject> _prefabDataBase;
    [SerializeField] private GameObject _currentPrefabToInstantiate;
    [Space(5)]
    [SerializeField] private GameObject _prefabPreviewGameObject;
    [SerializeField] private MeshRenderer _prefabPreviewMeshRenderer;
    [SerializeField] private Image _feedbackImage;
    [Space(5)]
    [SerializeField] private Material _canBePlaceMaterial;
    [SerializeField] private Material _cantBePlaceMaterial;
    
    [Header("Variables"), Space(5)]
    [Range(0,50)] public float buildModRange;
    [Space(5)]
    [SerializeField] private int _prefabDataBaseIndex;
    [Space(5)]
    public bool buildModOn;
    public bool freeBuildOn;
    private bool _canBePlace;
    
    [Header("Raycast Parameters"), Space(5)] 
    [SerializeField] private Vector3 hitLocation;
    [SerializeField] private Transform raycastOrigin;
    private RaycastHit customHit;
    
    [Space(25)]
    [Header("=== Sphere debug parameters ==================================="), Space(10)]
    public Color color = Color.red;
    public float radius = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        _prefabPreviewMeshRenderer = _prefabPreviewGameObject.GetComponent<MeshRenderer>();

        
        _currentPrefabToInstantiate = _prefabDataBase[_prefabDataBaseIndex];
        Debug.Log(" | Item : " + _currentPrefabToInstantiate.name + " | " + "Index : " + _prefabDataBaseIndex);
        
        
        MS_Building currentMsBuilding = _currentPrefabToInstantiate.GetComponent<MS_Building>();
        if (currentMsBuilding != null) _feedbackImage.sprite = currentMsBuilding._buildingSprite;
    }

    private void Update()
    {
        if (buildModOn)
        {
            // #########################################################################################################
            // -- SELECT BUILDING --------------------------------------------------------------------------------------
            
            if (Input.mouseScrollDelta.y > 0)
            {
                // On décrémente l'index pour revenir en arrière dans la liste
                _prefabDataBaseIndex--;

                // Si l'index devient inférieur à 0, on boucle au dernier élément
                if (_prefabDataBaseIndex < 0)
                {
                    _prefabDataBaseIndex = _prefabDataBase.Count - 1;
                }
    
                _currentPrefabToInstantiate = _prefabDataBase[_prefabDataBaseIndex];
                Debug.Log(" | Item : " + _currentPrefabToInstantiate.name + " | " + "Index : " + _prefabDataBaseIndex);
                
                MS_Building currentMsBuilding = _currentPrefabToInstantiate.GetComponent<MS_Building>();
                if (currentMsBuilding != null) _feedbackImage.sprite = currentMsBuilding._buildingSprite;
            }
            
            else if (Input.mouseScrollDelta.y < 0)
            {
                // On incrémente l'index pour avancer dans la liste
                _prefabDataBaseIndex++;

                // Si l'index dépasse ou égale la longueur, on boucle au premier élément (0)
                if (_prefabDataBaseIndex >= _prefabDataBase.Count)
                {
                    _prefabDataBaseIndex = 0;
                }
    
                _currentPrefabToInstantiate = _prefabDataBase[_prefabDataBaseIndex];
                Debug.Log(" | Item : " + _currentPrefabToInstantiate.name + " | " + "Index : " + _prefabDataBaseIndex);
                
                MS_Building currentMsBuilding = _currentPrefabToInstantiate.GetComponent<MS_Building>();
                if (currentMsBuilding != null) _feedbackImage.sprite = currentMsBuilding._buildingSprite;   
            }
            
            // #########################################################################################################
            // -- UPDATE MESHE PREVIEW ---------------------------------------------------------------------------------
            
            
            
            
            
            
            
            // #########################################################################################################
            // -- CHECK IF THE PLAYER CAN PLACE BUILDING ---------------------------------------------------------------
            
            if (_canBePlace)
            {
                _prefabPreviewMeshRenderer.material = _canBePlaceMaterial;
            }
            else _prefabPreviewMeshRenderer.material = _cantBePlaceMaterial;

            _prefabPreviewGameObject.transform.position = hitLocation;

            // #########################################################################################################
            // -- ROTATE PREVIEW ---------------------------------------------------------------------------------------
            
            if (Input.GetKey(KeyCode.A))
            {
                _prefabPreviewGameObject.transform.Rotate(0,2,0);
            }
            if (Input.GetKey(KeyCode.E))
            {
                _prefabPreviewGameObject.transform.Rotate(0,-2,0);
            }

            // #########################################################################################################
            // -- PLACE BUILDING ---------------------------------------------------------------------------------------
            
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Instantiate(_currentPrefabToInstantiate, _prefabPreviewGameObject.transform.position, _prefabPreviewGameObject.transform.rotation);
            }
        }
    }

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
                
                //OnDrawGizmos();
            }
        }
    }
    
    /*
    // Dessine la sphère même si l'objet n'est pas sélectionné
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        // Dessine une sphère pleine filaire
        Gizmos.DrawWireSphere(hitLocation, radius);

        // Alternative pour une sphère uniquement visible quand l'objet est sélectionné :
        // Utiliser OnDrawGizmosSelected() à la place
    }
    */
    
    public void Toggle_Preview()
    {
        _prefabPreviewGameObject.SetActive(!_prefabPreviewGameObject.activeSelf);
    }
}
