using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines; // <--- CORRECTION ICI

public class MS_BuildMod_Manager : MonoBehaviour
{
    public static MS_BuildMod_Manager Instance;
    
    [Header("References")]
    [SerializeField] private List<GameObject> _prefabDataBase;
    [SerializeField] private GameObject _currentPrefabToInstantiate;
    
    [Space(5)]
    [SerializeField] private GameObject _prefabPreviewGameObject;
    [SerializeField] private MeshRenderer _prefabPreviewMeshRenderer;
    [SerializeField] private Image _feedbackImage;
    
    [Space(5)]
    [SerializeField] private Material _canBePlaceMaterial;
    [SerializeField] private Material _cantBePlaceMaterial;
    
    [Header("Variables")]
    [Range(0, 50)] public float buildModRange;
    [Space(5)]
    [SerializeField] private int _prefabDataBaseIndex;
    [Space(5)]
    public bool buildModOn;
    public bool freeBuildOn;
    
    private bool _isDrawingRail = false;
    private Vector3 _railStartPoint;
    private SplineRail _previousRail;
    private GameObject _previewLineObj;
    
    private float _minSegmentLength = 1.5f;
    private bool _canBePlace;
    
    [Header("Raycast")]
    [SerializeField] private Vector3 hitLocation;
    [SerializeField] private Transform raycastOrigin;
    private RaycastHit customHit;

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
            UpdateCurrentPrefab();
    }

    private void Update()
    {
        if (!buildModOn) return;

        HandlePrefabSelection();

        bool isRail = _currentPrefabToInstantiate.GetComponent<SplineContainer>() != null;

        if (isRail)
        {
            HandleRailConstruction();
        }
        else
        {
            HandleStaticBuildingConstruction();
        }

        if (!isRail && _prefabPreviewGameObject != null && _prefabPreviewMeshRenderer != null)
        {
            _prefabPreviewMeshRenderer.material = _canBePlace ? _canBePlaceMaterial : _cantBePlaceMaterial;
            _prefabPreviewGameObject.transform.position = hitLocation;
        }
    }

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
            if (_previewLineObj == null) CreatePreviewLine(hitLocation, hitLocation);
            else UpdatePreviewLine(hitLocation, hitLocation);

            if (Input.GetKeyDown(KeyCode.Mouse0) && _canBePlace)
            {
                _railStartPoint = hitLocation;
                _isDrawingRail = true;
                CreatePreviewLine(_railStartPoint, _railStartPoint);
                Debug.Log("Rail : Point A sélectionné");
            }
        }
        else
        {
            UpdatePreviewLine(_railStartPoint, hitLocation);

            float dist = Vector3.Distance(_railStartPoint, hitLocation);
            _canBePlace = dist >= _minSegmentLength;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (_canBePlace)
                {
                    CreateRailSegment(_railStartPoint, hitLocation);
                    _railStartPoint = hitLocation;
                    Debug.Log("Rail : Segment validé");
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape))
            {
                StopDrawingRail();
            }
        }
    }

    void CreateRailSegment(Vector3 start, Vector3 end)
    {
        GameObject newRailObj = Instantiate(_currentPrefabToInstantiate, start, Quaternion.identity);
        SplineRail newRailScript = newRailObj.GetComponent<SplineRail>();

        if (newRailScript == null)
        {
            Debug.LogError("Erreur : Prefab Rail sans composant SplineRail !");
            Destroy(newRailObj);
            return;
        }

        newRailScript.InitializeSegment(start, end, _previousRail);

        if (_previousRail != null)
        {
            _previousRail.SetConnection(newRailScript, false);
            newRailScript.SetConnection(_previousRail, true);
        }
        else
        {
            if (RailNetworkManager.Instance != null)
                RailNetworkManager.Instance.TryConnectStartToStation(newRailScript, start);
        }

        _previousRail = newRailScript;
    }

    void StopDrawingRail()
    {
        if (_previousRail != null && RailNetworkManager.Instance != null)
        {
            Vector3 endPoint = _previousRail.GetEndPointWorld();
            RailNetworkManager.Instance.TryConnectEndToStation(_previousRail, endPoint);
        }

        _isDrawingRail = false;
        _previousRail = null;
        DestroyPreviewLine();
        Debug.Log("Rail : Terminé.");
    }

    void CancelRailConstruction()
    {
        _isDrawingRail = false;
        _previousRail = null;
        DestroyPreviewLine();
    }

    void CreatePreviewLine(Vector3 start, Vector3 end)
    {
        if (_previewLineObj != null) Destroy(_previewLineObj);
        _previewLineObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _previewLineObj.GetComponent<Renderer>().material = _canBePlaceMaterial;
        _previewLineObj.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
        UpdatePreviewLine(start, end);
    }

    void UpdatePreviewLine(Vector3 start, Vector3 end)
    {
        if (_previewLineObj == null) return;

        Vector3 mid = (start + end) * 0.5f;
        float dist = Vector3.Distance(start, end);
        
        _previewLineObj.transform.position = mid;
        if (dist > 0.1f) _previewLineObj.transform.LookAt(end);
        
        _previewLineObj.transform.localScale = new Vector3(0.2f, dist * 0.5f, 0.2f);
        
        _previewLineObj.GetComponent<Renderer>().material = _canBePlace ? _canBePlaceMaterial : _cantBePlaceMaterial;
    }

    void DestroyPreviewLine()
    {
        if (_previewLineObj != null) Destroy(_previewLineObj);
        _previewLineObj = null;
    }

    void HandleStaticBuildingConstruction()
    {
        if (_isDrawingRail) return;

        if (customHit.collider != null)
        {
            hitLocation = freeBuildOn ? customHit.point : SnapToGrid(customHit.point);
            _canBePlace = true;
        }
        else _canBePlace = false;

        if (_prefabPreviewGameObject != null)
        {
            _prefabPreviewGameObject.transform.position = hitLocation;

            if (Input.GetKey(KeyCode.A)) _prefabPreviewGameObject.transform.Rotate(0, 2, 0);
            if (Input.GetKey(KeyCode.E)) _prefabPreviewGameObject.transform.Rotate(0, -2, 0);

            if (Input.GetKeyDown(KeyCode.Mouse0) && _canBePlace)
            {
                Instantiate(_currentPrefabToInstantiate, _prefabPreviewGameObject.transform.position, _prefabPreviewGameObject.transform.rotation);
            }
        }
    }

    void HandlePrefabSelection()
    {
        if (_isDrawingRail) return;

        if (Input.mouseScrollDelta.y > 0)
        {
            _prefabDataBaseIndex = (_prefabDataBaseIndex - 1 + _prefabDataBase.Count) % _prefabDataBase.Count;
            UpdateCurrentPrefab();
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            _prefabDataBaseIndex = (_prefabDataBaseIndex + 1) % _prefabDataBase.Count;
            UpdateCurrentPrefab();
        }
    }

    void UpdateCurrentPrefab()
    {
        _currentPrefabToInstantiate = _prefabDataBase[_prefabDataBaseIndex];
        Debug.Log("Sélection : " + _currentPrefabToInstantiate.name);
        
        MS_Building b = _currentPrefabToInstantiate.GetComponent<MS_Building>();
        if (b != null && _feedbackImage != null) _feedbackImage.sprite = b._buildingSprite;
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        if (MS_GameManager.Instance == null) return pos;
        float gs = MS_GameManager.Instance.gridSize;
        return new Vector3(
            Mathf.Round(pos.x / gs) * gs,
            Mathf.Round(pos.y / gs) * gs,
            Mathf.Round(pos.z / gs) * gs
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