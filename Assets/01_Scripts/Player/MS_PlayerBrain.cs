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
    
    [Header("References"), Space(10)]
    public GameObject cinemachineTargetHorizontal;
    public GameObject cinemachineTargetVertical;
    public GameObject buildModUI;
    //public GameObject freeBuildModUI;
    [Space(5)]
    public Rigidbody playerRigidbody;
    [Space(5)]
    public Collider playerCollider;
    [Space(15)]
    public MS_PlayerMovement playerMovement;
    
    [Header("Variables"), Space(5)]
    [Range(0,1)] public float sensibility;
    public float playerSpeed;
    public float currentVerticalRotation;
    public float limitVerticalCameraRotationMin;
    public float limitVerticalCameraRotationMax;
    public float height;
    public float jumpForce;
    [Space(5)] 
    public bool playerCanLookAround = true;
    public bool isGrounded;
    public bool canMove = true;
    public bool isMoving;
    
    [Header("Rewired"), Space(10)]
    public int playerID;
    public Player player;
    

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
        buildModUI.SetActive(MS_BuildMod_Manager.Instance.buildModOn);
    }

    void Update()
    {
        if (player.GetButtonDown("ToggleBuildMode")) Toggle_Build_Mod();
    }

    
    
    private void Toggle_Build_Mod()
    {
        MS_BuildMod_Manager.Instance.buildModOn = !MS_BuildMod_Manager.Instance.buildModOn;
        buildModUI.SetActive(MS_BuildMod_Manager.Instance.buildModOn);
        MS_BuildMod_Manager.Instance.Toggle_Preview();
    }
    
    private void Toggle_Free_Build()
    {
        MS_BuildMod_Manager.Instance.freeBuildOn = !MS_BuildMod_Manager.Instance.freeBuildOn;
        //freeBuildModUI.SetActive(freeBuildOn);
    }
    
    
}