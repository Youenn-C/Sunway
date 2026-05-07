using System.Collections.Generic;
using UnityEngine;
using Rewired;
using TMPro;
using Unity.Cinemachine;

public class MS_PlayerBrain : MonoBehaviour
{
    public static MS_PlayerBrain Instance;
    
    [Header("References"), Space(5)]
    public GameObject cinemachineTargetHorizontal;
    public GameObject cinemachineTargetVertical;
    [Space(5)]
    public Rigidbody playerRigidbody;
    public Collider playerCollider;
    [Space(5)]
    public MS_PlayerMovement playerMovement;
    
    [Header("Variables"), Space(5)]
    public float playerSpeed;
    [Range(0,1)] public float sensibility;
    public float currentVerticalRotation;
    public float limitVerticalCameraRotationMin;
    public float limitVerticalCameraRotationMax;
    public float height;
    public float jumpForce;
    [Space(5)] 
    public bool playerCanLookAround = true;
    public bool canMove = true;
    public bool isMoving;
    public bool isGrounded;
    
    [Header("Rewired"), Space(5)]
    public int playerID;
    public Player player;
    
    //[Header("Inventory"), Space(5)]
    //public MyDictionary<string, int> minecraftInventory;
    

    void Awake()
    {
        // Instance creation
        if (Instance == null) Instance = this;
        else Destroy(this);
        
        // Get player from Rewired
        player = ReInput.players.GetPlayer(playerID);
    }

    
    void Update()
    {
        // Get input for MC_Inventory
        //if (player.GetButtonDown("Minecraft_Inventory")) Open_And_Close_Minecraft_Inventory();
    }


    //private void Open_And_Close_Minecraft_Inventory()
    //{
    //    minecraftInventoryIsOpen = !minecraftInventoryIsOpen;
    //    
    //    if (minecraftInventoryIsOpen)
    //    {
    //        mcFullInventoryUI.SetActive(true);
    //        mcCurrentsSlotsInventoryUI.SetActive(false);
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;
    //    }
    //    else
    //    {
    //        mcFullInventoryUI.SetActive(false);
    //        mcCurrentsSlotsInventoryUI.SetActive(true);
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;
    //    }
    //}
}
