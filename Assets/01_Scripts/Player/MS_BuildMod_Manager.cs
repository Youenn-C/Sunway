using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;       // Nécessaire pour les Splines
using Unity.Mathematics;   // Nécessaire pour les conversions float3

public class MS_BuildMod_Manager : MonoBehaviour
{
    public static MS_BuildMod_Manager Instance;
    
    [Header("References"), Space(5)]
    [SerializeField] private List<GameObject> _prefabDataBase;
    [SerializeField] private GameObject _currentPrefabToInstantiate;
    private GameObject _previewLineObj;    // Un simple trait ou cylindre pour la prévisualisation
    [Space(5)]
    [SerializeField] private GameObject _prefabPreviewGameObject;
    [SerializeField] private MeshRenderer _prefabPreviewMeshRenderer;
    [SerializeField] private Image _feedbackImage;
    [Space(5)]
    [SerializeField] private Material _canBePlaceMaterial;   // Matériau Hologramme (Vert/Cyan)
    [SerializeField] private Material _cantBePlaceMaterial;  // Matériau Erreur (Rouge)
    [Space(5)]
    private SplineRail _activeSplineRail;         // Référence au script du rail en cours
    private SplineRail _previousRail;      // Le dernier rail posé (pour la connexion)
    
    [Header("Variables"), Space(5)]
    // -- Vector 3 ----------------------
    private Vector3 _railStartPoint;       // Point A (début du segment en cours)
    [Space(5)]
    // -- Float -------------------------
    [Range(0, 50)] public float buildModRange;
    private float _minSegmentLength = 1.5f;       // Distance min entre deux points pour éviter les bugs
    [Space(5)]
    // -- Int ---------------------------
    [SerializeField] private int _prefabDataBaseIndex;
    [Space(5)]
    // -- Bool --- ----------------------
    public bool buildModOn;
    public bool freeBuildOn;
    private bool _isDrawingRail = false;          // Est-on en train de tracer un rail ?
    private bool _canBePlace;
    
    [Header("Raycast Parameters"), Space(5)] 
    [SerializeField] private Vector3 hitLocation;
    [SerializeField] private Transform raycastOrigin;
    private RaycastHit customHit;
    
    [Space(25)]
    [Header("=== Debug ==="), Space(10)]
    public Color color = Color.red;
    public float radius = 1.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (_prefabPreviewGameObject != null)
            _prefabPreviewMeshRenderer = _prefabPreviewGameObject.GetComponent<MeshRenderer>();

        if (_prefabDataBase.Count > 0)
        {
            UpdateCurrentPrefab();
        }
    }

    private void Update()
    {
        if (!buildModOn) return;

        // 1. SÉLECTION DU PREFAB (Molette)
        HandlePrefabSelection();

        // 2. LOGIQUE DE CONSTRUCTION (Rail vs Bâtiment)
        // On vérifie si le prefab actuel contient un SplineContainer (donc c'est un Rail)
        bool isRail = _currentPrefabToInstantiate.GetComponent<SplineContainer>() != null;

        if (isRail)
        {
            HandleRailConstruction();
        }
        else
        {
            HandleStaticBuildingConstruction();
        }

        // 3. FEEDBACK VISUEL (Couleur)
        if (_prefabPreviewGameObject != null && _prefabPreviewMeshRenderer != null)
        {
            // Si on dessine un rail, la couleur est gérée dans HandleRailConstruction (via le matériau du rail)
            // Sinon, on gère la couleur du preview statique
            if (!isRail || !_isDrawingRail)
            {
                _prefabPreviewMeshRenderer.material = _canBePlace ? _canBePlaceMaterial : _cantBePlaceMaterial;
                _prefabPreviewGameObject.transform.position = hitLocation;
            }
        }
    }

    // =============================================================================
    // LOGIQUE RAIL (SPLINE)
    // =============================================================================

    void HandleRailConstruction()
    {
        if (customHit.collider != null)
        {
            hitLocation = freeBuildOn ? customHit.point : SnapToGrid(customHit.point);
            _canBePlace = true;
        }
        else
        {
            _canBePlace = false;
            return;
        }

        if (!_isDrawingRail)
        {
            // --- ÉTAPE 1 : ATTENTE DU PREMIER CLIC (POINT A) ---
            // On affiche un simple repère ou le début du prefab
            if (_previewLineObj != null) _previewLineObj.transform.position = hitLocation;

            if (Input.GetKeyDown(KeyCode.Mouse0) && _canBePlace)
            {
                _railStartPoint = hitLocation;
                _isDrawingRail = true;
                Debug.Log("Point A sélectionné : " + _railStartPoint);
                
                // Optionnel : Activer une ligne de prévisualisation qui suit la souris
                CreatePreviewLine(_railStartPoint);
            }
        }
        else
        {
            // --- ÉTAPE 2 : PRÉVISUALISATION VERS LE POINT B ---
            UpdatePreviewLine(hitLocation);

            // Vérification distance min
            float dist = Vector3.Distance(_railStartPoint, hitLocation);
            _canBePlace = dist >= _minSegmentLength;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (_canBePlace)
                {
                    // --- VALIDATION : CRÉATION DU SEGMENT ---
                    CreateRailSegment(_railStartPoint, hitLocation);
                    
                    // Le point B devient le nouveau Point A pour le prochain segment
                    _railStartPoint = hitLocation;
                    Debug.Log("Segment créé. Nouveau Point A : " + _railStartPoint);
                }
            }

            // Clic Droit : Terminer la ligne
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                StopDrawingRail();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelRailConstruction();
            }
        }
    }

    void CreateRailSegment(Vector3 start, Vector3 end)
    {
        // 1. Instancier le prefab
        GameObject newRailObj = Instantiate(_currentPrefabToInstantiate, start, Quaternion.identity);
        SplineRail newRailScript = newRailObj.GetComponent<SplineRail>();

        if (newRailScript == null)
        {
            Debug.LogError("Prefab rail invalide !");
            Destroy(newRailObj);
            return;
        }

        // 2. Initialiser le segment avec les 2 points et le rail précédent
        newRailScript.InitializeSegment(start, end, _previousRail);

        // 3. Gérer les connexions
        // Si c'est le premier segment, on cherche une gare au départ
        if (_previousRail == null)
        {
            if (RailNetworkManager.Instance != null)
                RailNetworkManager.Instance.TryConnectStartToStation(newRailScript, start);
        }
        else
        {
            // Connexion manuelle avec le rail précédent
            _previousRail.SetConnection(newRailScript, false); // Fin du précédent connectée à...
            newRailScript.SetConnection(_previousRail, true);  // ...Début du nouveau
        }

        // 4. Mettre à jour la référence "Dernier Rail"
        _previousRail = newRailScript;

        // 5. Chercher une gare à l'arrivée (Optionnel, ou laissé pour la fin de la ligne)
        // Pour l'instant, on laisse le joueur continuer. S'il arrête, on scanne la fin.
    }

    void StopDrawingRail()
    {
        // Si on s'arrête, on vérifie si la fin touche une gare
        if (_previousRail != null && RailNetworkManager.Instance != null)
        {
            Vector3 endPoint = _previousRail.GetEndPointWorld();
            RailNetworkManager.Instance.TryConnectEndToStation(_previousRail, endPoint);
        }

        _isDrawingRail = false;
        _previousRail = null; // On reset la chaîne (ou on la garde si on veut faire des branches plus tard)
        DestroyPreviewLine();
        Debug.Log("Ligne de rail terminée.");
    }

    void CancelRailConstruction()
    {
        _isDrawingRail = false;
        _previousRail = null;
        _railStartPoint = Vector3.zero;
        DestroyPreviewLine();
        Debug.Log("Construction annulée.");
    }

    // --- MÉTHODES DE PRÉVISUALISATION (Simple Ligne ou Cylindre) ---
    void CreatePreviewLine(Vector3 start)
    {
        // Crée un simple cylindre ou un LineRenderer pour montrer le trajet
        _previewLineObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _previewLineObj.transform.localScale = new Vector3(0.1f, 100f, 0.1f);
        _previewLineObj.GetComponent<Renderer>().material = _canBePlaceMaterial;
        // Positionnement géré dans UpdatePreviewLine
    }

    void UpdatePreviewLine(Vector3 end)
    {
        if (_previewLineObj == null) return;
        
        Vector3 midPoint = (_railStartPoint + end) * 0.5f;
        _previewLineObj.transform.position = midPoint;
        _previewLineObj.transform.LookAt(end);
        
        float dist = Vector3.Distance(_railStartPoint, end);
        _previewLineObj.transform.localScale = new Vector3(0.2f, dist * 0.5f, 0.2f);
        
        // Couleur selon validité
        _previewLineObj.GetComponent<Renderer>().material = _canBePlace ? _canBePlaceMaterial : _cantBePlaceMaterial;
    }

    void DestroyPreviewLine()
    {
        if (_previewLineObj != null) Destroy(_previewLineObj);
        _previewLineObj = null;
    }

    // =============================================================================
    // LOGIQUE BÂTIMENT (STATIQUE - Ton code original adapté)
    // =============================================================================

    void HandleStaticBuildingConstruction()
    {
        // Si on était en train de dessiner un rail, on ignore (sécurité)
        if (_isDrawingRail) return;

        if (customHit.collider != null)
        {
            hitLocation = freeBuildOn ? customHit.point : SnapToGrid(customHit.point);
            _canBePlace = true;
        }
        else
        {
            _canBePlace = false;
        }

        if (_prefabPreviewGameObject != null)
        {
            _prefabPreviewGameObject.transform.position = hitLocation;

            // Rotation (Touches A / E)
            if (Input.GetKey(KeyCode.A))
                _prefabPreviewGameObject.transform.Rotate(0, 2, 0);
            if (Input.GetKey(KeyCode.E))
                _prefabPreviewGameObject.transform.Rotate(0, -2, 0);

            // Placement
            if (Input.GetKeyDown(KeyCode.Mouse0) && _canBePlace)
            {
                Instantiate(_currentPrefabToInstantiate, _prefabPreviewGameObject.transform.position, _prefabPreviewGameObject.transform.rotation);
            }
        }
    }

    // =============================================================================
    // UTILITAIRES
    // =============================================================================

    void HandlePrefabSelection()
    {
        // Interdire le changement de prefab si on est en train de dessiner un rail
        if (_isDrawingRail) return;

        if (Input.mouseScrollDelta.y > 0)
        {
            _prefabDataBaseIndex--;
            if (_prefabDataBaseIndex < 0) _prefabDataBaseIndex = _prefabDataBase.Count - 1;
            UpdateCurrentPrefab();
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            _prefabDataBaseIndex++;
            if (_prefabDataBaseIndex >= _prefabDataBase.Count) _prefabDataBaseIndex = 0;
            UpdateCurrentPrefab();
        }
    }

    void UpdateCurrentPrefab()
    {
        _currentPrefabToInstantiate = _prefabDataBase[_prefabDataBaseIndex];
        Debug.Log(" | Item : " + _currentPrefabToInstantiate.name + " | Index : " + _prefabDataBaseIndex);

        MS_Building currentMsBuilding = _currentPrefabToInstantiate.GetComponent<MS_Building>();
        if (currentMsBuilding != null && _feedbackImage != null)
        {
            _feedbackImage.sprite = currentMsBuilding._buildingSprite;
        }
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        if (MS_GameManager.Instance == null) return pos;
        return new Vector3(
            Mathf.Round(pos.x / MS_GameManager.Instance.gridSize) * MS_GameManager.Instance.gridSize,
            Mathf.Round(pos.y / MS_GameManager.Instance.gridSize) * MS_GameManager.Instance.gridSize,
            Mathf.Round(pos.z / MS_GameManager.Instance.gridSize) * MS_GameManager.Instance.gridSize
        );
    }

    private void FixedUpdate()
    {
        if (buildModOn)
        {
            Physics.Raycast(raycastOrigin.position, raycastOrigin.rotation * Vector3.forward, out customHit, buildModRange);
        }
    }

    public void Toggle_Preview()
    {
        if (_prefabPreviewGameObject != null)
            _prefabPreviewGameObject.SetActive(!_prefabPreviewGameObject.activeSelf);
    }
}