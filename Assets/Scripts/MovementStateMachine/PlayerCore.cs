using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    // Player Conditions
    bool isGrounded;
    bool isMoving;
    bool isTryingToJump;
    bool isPressingCrouch;

    // Tracking
    public  Vector2 movementInput;
    private Player_State currentState;

    // Component Referencing
    private InputMaster InputController;
    public  Transform GroundCheck;
    public  LayerMask GroundMask;

    private void Awake()
    { 
        InputController = new InputMaster();
        InputController.Enable();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        movementInput = InputController.Player.Movement.ReadValue<Vector2>().normalized;

        isMoving = movementInput.magnitude >= 0.2f;
        isTryingToJump = InputController.Player.Jump.ReadValue<float>() == 1f;
        isPressingCrouch = InputController.Player.Crouch.ReadValue<float>() == 1f;
        isGrounded = Physics.CheckSphere(GroundCheck.position, 0.4f, GroundMask);

        if(isMoving && !isTryingToJump && !isPressingCrouch && isGrounded)
        {
          //  currentState = new Player_Running(this);
        }
        
    }
}
