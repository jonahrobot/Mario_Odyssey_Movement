using UnityEngine;

public class Player_Running : Player_State
{
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

    public Player_Running(PlayerStateMachineCore core) : base(core)
    {
    }

    public override void StartMethod()
    {
        AnimationController.ChangeAnimationState("Run", true);

        AbleToJump = data.GetBool("AbleToJump", false);
        CurrentSpeed = Mathf.Min(data.GetFloat("CurrentSpeed", 0f), MaxSpeed);

        MaxSpeedOriginal = MaxSpeed;
        core.SpeedDebug = CurrentSpeed;

        if (CurrentSpeed < MaxSpeed)
            AnimationController.ChangeAnimationSpeed(2);
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

        if (StateContext.IsMoving)
        {
            RotatePlayer();
            core.MovePlayer(Direction, CurrentSpeed);
        }

        if (StateContext.IsJumping == false)
            AbleToJump = true;

        CoyoteTime();
    }

    private void Accelerate(float Acceleration)
    {
        Velocity += Acceleration * Time.deltaTime;

        if (Acceleration > 0)
        {
            CurrentSpeed = Mathf.Min(CurrentSpeed + Velocity, MaxSpeed);
            AnimationController.ChangeAnimationSpeed(2 - CurrentSpeed / MaxSpeed);
        }
        else
        {
            CurrentSpeed = Mathf.Max(CurrentSpeed + Velocity, MaxSpeed);
        }

        core.SpeedDebug = CurrentSpeed;
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
        bool JumpBufferStart = TriggeredJumpBuffer == false && StateContext.IsGrounded == false;

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

        if (StateContext.IsGrounded)
        {
            TriggeredJumpBuffer = false;
            JumpBufferOn = false;
        }
    }


    public override void ExitMethod()
    {
        data.StoreBool("AbleToJump", AbleToJump);
        data.StoreFloat("CurrentSpeed", CurrentSpeed);
        data.StoreVector3("CurrentDirection", Direction);
        AnimationController.ChangeAnimationState("Run", false);
    }

    public override void CheckStateSwaps()
    {
        if (StateContext.IsCrouched && StateContext.IsGrounded)
        {
            core.SwapState(new Player_Crouch(core));
            return;
        }
        if (StateContext.IsJumping && AbleToJump == true && (StateContext.IsGrounded || JumpBufferOn))
        {
            AbleToJump = false;
            core.SwapState(new Player_Jumping(core));
            return;
        }

        if (StateContext.IsCrouched && StateContext.IsGrounded == false)
        {
            core.SwapState(new Player_GroundPound(core));
            return;
        }

        if (core.CollidingWithHat)
        {
            core.SwapState(new Player_Jumping(core));
            return;
        }

        if (StateContext.HasClicked && StateContext.HasHat)
        {
            core.SwapState(new Player_Hat_Throw(core));
            return;
        }
    }
}
