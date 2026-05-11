using UnityEngine;

public class MS_PlayerMovement : MonoBehaviour
{
    [Header("Variables"), Space(5)]
    [SerializeField] private float _forwardMovement;
    [SerializeField] private float _lateralMovement;
    [Space(5)]
    [SerializeField] private bool _triggerJump;
    
    void Start()
    {
        
    }

    void Update()
    {
        // #############################################################################################################
        // -- PLAYER MOVEMENT ------------------------------------------------------------------------------------------
        
        // Mises à jour des directions basées sur les boutons
        _forwardMovement = MS_PlayerBrain.Instance.player.GetAxis("Forward_Movement");
        _lateralMovement = MS_PlayerBrain.Instance.player.GetAxis("Lateral_Movement");

        // Déterminer si le joueur bouge
        if (MS_PlayerBrain.Instance.canMove) MS_PlayerBrain.Instance.isMoving = _forwardMovement > 0 || _lateralMovement > 0;
        
        // #############################################################################################################
        // -- ROTATION CAMERA ------------------------------------------------------------------------------------------

        if (MS_PlayerBrain.Instance.playerCanLookAround)
        {
            float mouseMovementX = MS_PlayerBrain.Instance.player.GetAxis("Mouse_Movement_X");
            float mouseMovementY = MS_PlayerBrain.Instance.player.GetAxis("Mouse_Movement_Y");
            float sensitivity = MS_PlayerBrain.Instance.sensibility;

            // -- CALCUL ROTATION VERTICALE (Axe X) --------------------------------------------------------------------
            // On accumule l'input brut, puis on clamp le résultat total
            MS_PlayerBrain.Instance.currentVerticalRotation += (mouseMovementY * sensitivity);
            MS_PlayerBrain.Instance.currentVerticalRotation = Mathf.Clamp(MS_PlayerBrain.Instance.currentVerticalRotation, MS_PlayerBrain.Instance.limitVerticalCameraRotationMin, MS_PlayerBrain.Instance.limitVerticalCameraRotationMax);

            // -- APPLICATION ROTATION VERTICALE -----------------------------------------------------------------------
            // On récupère la rotation actuelle pour préserver les axes Y et Z, puis on injecte notre angle X clampé
            Vector3 currentVerticalEuler = MS_PlayerBrain.Instance.cinemachineTargetVertical.transform.localEulerAngles;
            MS_PlayerBrain.Instance.cinemachineTargetVertical.transform.localEulerAngles = new Vector3(MS_PlayerBrain.Instance.currentVerticalRotation, currentVerticalEuler.y, currentVerticalEuler.z);

            // -- APPLICATION ROTATION HORIZONTALE (Axe Y) -------------------------------------------------------------
            // Rotation directe sur l'axe Y. Pas besoin de stocker l'état précédent pour le corps horizontal.
            MS_PlayerBrain.Instance.cinemachineTargetHorizontal.transform.localEulerAngles += new Vector3(0f, mouseMovementX * sensitivity, 0f);
        }
        
        // #############################################################################################################
        // -- JUMP SYSTEM ----------------------------------------------------------------------------------------------
        
        CheckIfPlayerIsGrounded();
        
        if (MS_PlayerBrain.Instance.player.GetButtonDown("Jump") && MS_PlayerBrain.Instance.isGrounded)
        {
            _triggerJump = true;
        }
    }
    
    void FixedUpdate()
    {
        if (MS_PlayerBrain.Instance.canMove)
        {
            Vector3 direction = Vector3.zero;
            
            if (_forwardMovement > 0) direction += transform.forward;
            if (_lateralMovement > 0) direction += transform.right;
            if (_forwardMovement < 0) direction -= transform.forward;
            if (_lateralMovement < 0) direction -= transform.right;
    
            direction.Normalize();
    
            Vector3 velocity = direction * MS_PlayerBrain.Instance.playerSpeed;
            velocity.y = MS_PlayerBrain.Instance.playerRigidbody.linearVelocity.y;
    
            MS_PlayerBrain.Instance.playerRigidbody.linearVelocity = velocity;
        }

        if (_triggerJump)
        {
            _triggerJump = false;
            MS_PlayerBrain.Instance.playerRigidbody.AddForce(Vector3.up * MS_PlayerBrain.Instance.jumpForce, ForceMode.Impulse);
        }
    }
    
    void CheckIfPlayerIsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, MS_PlayerBrain.Instance.height))
        {
            MS_PlayerBrain.Instance.isGrounded = true;
        }
        else
        {
            MS_PlayerBrain.Instance.isGrounded = false;
        }
    }
}
