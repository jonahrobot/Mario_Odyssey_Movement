using UnityEngine;

public class Player_Running : Player_State
{
    PlayerStateMachineCore core;
    Player_Timers data;

    // Movement
    private const float Acceleration = 0.5f;
    private const float Deceleration = -0.125f;

    private float MaxSpeed = 20f;

    Vector3 Direction;

    float Velocity;
    float CurrentSpeed;
    float MaxSpeedOriginal;
    float turnSmoothVelocity;

    // Coyote Time

    bool AbleToJump;
    bool JumpBufferOn;
    bool TriggeredJumpBuffer;
    float JumpBufferTimer;

    public Player_Running(PlayerStateMachineCore core)
    {
        this.core = core;
        data = core.stateMemory;
    }

    public override void StartMethod()
    {
        core.ChangeAnimationState("Run", true);

        AbleToJump = data.GetBool("AbleToJump", false);
        CurrentSpeed = Mathf.Min(data.GetFloat("CurrentSpeed", 0f), MaxSpeed);

        MaxSpeedOriginal = MaxSpeed;
        core.speedDebug = CurrentSpeed;

        if (CurrentSpeed < MaxSpeed)
            core.ChangeAnimationSpeed(2);
    }

    public override void UpdateMethod()
    {
        Vector3 Direction = GetCurrentDirection();

        MaxSpeed = FindMaxSpeed(Direction);

        if (CurrentSpeed < MaxSpeed)
            Accelerate(Acceleration);

        if (CurrentSpeed > MaxSpeed)
            Accelerate(Deceleration);

        if (CurrentSpeed == MaxSpeed)
            Velocity = 0f;

        if (core.isPressingWSAD)
        {
            RotatePlayer();
            core.MovePlayer(Direction, CurrentSpeed);
        }

        if (core.isPressingSpace == false)
            AbleToJump = true;

        CoyoteTime();
    }

    private void Accelerate(float Acceleration)
    {
        Velocity += Acceleration * Time.deltaTime;

        if (Acceleration > 0)
        {
            CurrentSpeed = Mathf.Min(CurrentSpeed + Velocity, MaxSpeed);
            core.ChangeAnimationSpeed(2 - CurrentSpeed / MaxSpeed);
        }
        else
        {
            CurrentSpeed = Mathf.Max(CurrentSpeed + Velocity, MaxSpeed);
        }

        core.speedDebug = CurrentSpeed;
    }

    private Vector3 GetCurrentDirection()
    {
        float targetAngle = TargetAngleCameraRelative(core);

        Vector3 currentDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        Vector3 alignedDirection = AlignVectorToSlope(currentDirection, core.transform.position);

        Direction = currentDirection;
        return alignedDirection;
    }

    private void RotatePlayer()
    {
        float targetAngle = TargetAngleCameraRelative(core);

        float CurrentAngle = Mathf.SmoothDampAngle(core.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);

        core.transform.rotation = Quaternion.Euler(0f, CurrentAngle, 0f);
    }

    private float FindMaxSpeed(Vector3 CurrentVelocity)
    {
        var SlopeAngle = CurrentVelocity.y;
        bool GoingDownHill = SlopeAngle < -0.2f;
        bool GoingUpHill = SlopeAngle > 0.2f;

        if (GoingDownHill)
            return MaxSpeedOriginal + 12f;

        if (GoingUpHill)
            return MaxSpeedOriginal - 4f;

        return MaxSpeedOriginal; 
    }

    private void CoyoteTime()
    {
        bool JumpBufferStart = TriggeredJumpBuffer == false && core.isGrounded == false;

        if (JumpBufferStart)
        {
            TriggeredJumpBuffer = true;
            JumpBufferTimer = Time.time;
            JumpBufferOn = true;
        }

        bool JumpBufferOver = Time.time - JumpBufferTimer > 0.2f;

        if (TriggeredJumpBuffer == true && JumpBufferOver)
        {
            JumpBufferOn = false;
        }

        if (core.isGrounded)
        {
            TriggeredJumpBuffer = false;
            JumpBufferOn = false;
        }
    }

    public override float GetUpdateToGravity()
    {
        return 0;
    }

    public override void ExitMethod()
    {
        data.StoreBool("AbleToJump", AbleToJump);
        data.StoreFloat("CurrentSpeed", CurrentSpeed);
        data.StoreVector3("CurrentDirection", Direction);
        core.ChangeAnimationState("Run", false);
    }

    public override void CheckForStateSwap()
    {
        if (core.isPressingCrouch && core.isGrounded)
        {
            core.SwapState(new Player_Crouch(core));
            return;
        }
        if (core.isPressingSpace && AbleToJump == true && (core.isGrounded || JumpBufferOn))
        {
            AbleToJump = false;
            core.SwapState(new Player_Jumping(core));
            return;
        }

        if (core.isPressingCrouch && core.isGrounded == false)
        {
            core.SwapState(new Player_GroundPound(core));
            return;
        }

        if (core.onHat)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }
        if (core.hasClicked && core.getHasHat())
        {
            core.SwapState(new Player_Hat_Throw(core));
            return;
        }
    }
}
