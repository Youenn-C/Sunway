using System;
using UnityEngine;

public class MS_GameManager : MonoBehaviour
{
    public static MS_GameManager Instance;
    
    [Header("Variables"), Space(5)]
    public float gridSize = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
