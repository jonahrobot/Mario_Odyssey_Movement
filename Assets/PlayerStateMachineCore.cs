using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Core of Players State Machine

public class PlayerStateMachineCore : MonoBehaviour
{

    private InputMaster InputController;
    private Player_State Current_Player_State;
    private State_Types State;

    enum State_Types
    {
        Idle,
        Run,
        Jump
    }

    private void Awake()
    {
        InputController = new InputMaster();
        InputController.Enable();

        State = State_Types.Idle;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void switchState(Player_State newState)
    {
        Current_Player_State.ExitMethod();
        Current_Player_State = newState;
        Current_Player_State.StartMethod();
    }

    private void Update()
    {
        var i_Movement = InputController.Player.Movement.ReadValue<Vector2>().normalized;
        var i_Jump = InputController.Player.Jump.ReadValue<float>();
        var i_GroundPound = InputController.Player.GroundPound.ReadValue<float>();

        // Player is trying to move 
        if (i_Movement.magnitude >= 0.1f && (State == State_Types.Idle))
        {
            State = State_Types.Run;
            switchState(new Player_Running());
        }
        
        // Player is trying to jump
        if(i_Jump == 1f && (State == State_Types.Idle || State == State_Types.Run))
        {
            State = State_Types.Jump;
            switchState(new Player_Jumping());
        }

        Current_Player_State.UpdateMethod();

    }
}
