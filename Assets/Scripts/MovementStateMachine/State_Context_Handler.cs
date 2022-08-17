using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Context_Handler : MonoBehaviour
{
    private InputMaster _inputController;
    private LayerMask _groundMask;
    private Transform _groundCheck;
    private Transform _camera;

    public bool IsCrouched;
    public bool IsJumping;
    public bool IsMoving;
    public bool IsGrounded;

    public Vector2 MovementInput;
    public Vector3 CameraRotation;
    public bool HasClicked;

    public bool HasHat = true;

    public bool DisableGroundCheck;

    public State_Context_Handler(LayerMask groundMask)
    {
        _groundMask = groundMask;

        _inputController = new InputMaster();
        _inputController.Enable();

        _groundCheck = GameObject.Find("GroundCheck").transform;

        _camera = GameObject.Find("Camera").transform;
    }

    public void UpdateInputs()
    {
        MovementInput = _inputController.Player.Movement.ReadValue<Vector2>().normalized;
   

        IsGrounded = Physics.CheckSphere(_groundCheck.position, 0.6f, _groundMask);

        if (DisableGroundCheck == true)
        {
            IsGrounded = false;
        }

        IsCrouched = _inputController.Player.Crouch.ReadValue<float>() == 1f;
        IsJumping = _inputController.Player.Jump.ReadValue<float>() == 1f;
     
        IsMoving = MovementInput.magnitude >= 0.2f;

        HasClicked = _inputController.Player.MouseClick.ReadValue<float>() == 1f;

        CameraRotation = _camera.eulerAngles;
    }


}
