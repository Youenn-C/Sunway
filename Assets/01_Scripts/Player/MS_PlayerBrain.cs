using System;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.Rendering.HighDefinition;

public class MS_PlayerBrain : MonoBehaviour
{
    public static MS_PlayerBrain Instance;
    
    [Header("References"), Space(5)]
    public GameObject cinemachineTargetHorizontal;
    public GameObject cinemachineTargetVertical;
    public GameObject buildModUI;
    [Space(5)]
    public Rigidbody playerRigidbody;
    public Collider playerCollider;
    [Space(5)]
    public MS_PlayerMovement playerMovement;
    
    [Header("Variables"), Space(5)]
    [Range(0,1)] public float sensibility;
    public float playerSpeed;
    public float currentVerticalRotation;
    public float limitVerticalCameraRotationMin;
    public float limitVerticalCameraRotationMax;
    public float height;
    public float jumpForce;
    public float buildModRange;
    [Space(5)] 
    public bool playerCanLookAround = true;
    public bool canMove = true;
    public bool isMoving;
    public bool isGrounded;
    public bool buildModOn = false;
    public bool freeBuildOn = true;
    
    [Header("Rewired"), Space(5)]
    public int playerID;
    public Player player;
    
    [Header("Raycast Parameters"), Space(5)] 
    [SerializeField] private Vector3 hitLocation;
    [SerializeField] private Transform raycastOrigin;
    private RaycastHit customHit; 
    
    

    void Awake()
    {
        // Instance creation
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        // Get player from Rewired
        player = ReInput.players.GetPlayer(playerID);
    }

    private void Start()
    {
        buildModUI.SetActive(buildModOn);
    }

    void Update()
    {
        if (player.GetButtonDown("ToggleBuildMode")) Toggle_Build_Mod();
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
                
                OnDrawGizmos();
            }
        }
    }
    
    private void Toggle_Build_Mod()
    {
        buildModOn = !buildModOn;
        buildModUI.SetActive(buildModOn);
    }
    
    private void Toggle_Free_Build()
    {
        freeBuildOn = !freeBuildOn;
        //buildModUI.SetActive(buildModOn);
    }
    
    [Space(50)]
    [Header("Sphere debug parameters"), Space(5)]
    
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