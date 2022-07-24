using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Context_Handler : MonoBehaviour
{
    private InputMaster.PlayerActions _inputController;
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

    public bool HasHat;

    private bool DisableGroundCheck;

    public State_Context_Handler(LayerMask groundMask)
    {
        _groundMask = groundMask;

        _inputController = new InputMaster().Player;

        _groundCheck = GameObject.Find("GroundCheck").transform;

        _camera = Camera.main.transform;
    }

    private void Update()
    {
        MovementInput = _inputController.Movement.ReadValue<Vector2>().normalized;

        IsGrounded = Physics.CheckSphere(_groundCheck.position, 0.6f, _groundMask);
        if (DisableGroundCheck == true)
        {
            IsGrounded = false;
        }

        IsCrouched = _inputController.Crouch.ReadValue<float>() == 1f;
        IsJumping = _inputController.Jump.ReadValue<float>() == 1f;
        IsMoving = MovementInput.magnitude >= 0.2f;

        HasClicked = _inputController.MouseClick.ReadValue<float>() == 1f;

        CameraRotation = _camera.eulerAngles;
    }


}
