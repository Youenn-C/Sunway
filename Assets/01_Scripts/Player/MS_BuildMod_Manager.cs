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

        //if (_prefabDataBase.Count > 0)
    }
/*
    private void Update()
    {
        if (!buildModOn) return;
    }
*/
    

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