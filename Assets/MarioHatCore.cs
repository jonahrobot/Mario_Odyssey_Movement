using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarioHatCore : MonoBehaviour
{
    PlayerStateMachineCore core;
    GameObject parent;
    GameObject arm;
    BoxCollider collider;

    private Vector3 target;
    private float velocity;
    private enum throwState{IDLE,THROW,RETURN,TRANSITION,ONARM};
    private throwState currentState = throwState.IDLE;

    private float defaultVelocity = 20f;
    private float maxVelocity = 50f;
    private float acceleration = 5f;
    private float hangTime = 1f;
    private Vector3 targetSize;
    private bool Heatseaking;


    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
        parent = GameObject.Find("MarioHatLink");
        core = GameObject.Find("Player").GetComponent<PlayerStateMachineCore>();
        arm = GameObject.Find("MarioHatLinkArm");
    }

    public void linkToArm()
    {
        currentState = throwState.ONARM;
    }

    public void startThrow(Vector3 direction)
    {
        collider.enabled = true;
        target = transform.position + direction * 14f;
        Heatseaking = false;
        velocity = defaultVelocity;

        targetSize = transform.localScale * 1.5f;

        currentState = throwState.THROW;
    }

    private void startReturn()
    {
        target = parent.transform.position;
        Heatseaking = true;
        velocity = defaultVelocity;
        collider.enabled = false;

        targetSize = transform.localScale / 1.5f;

        currentState = throwState.RETURN;
    }

    private void LateUpdate()
    {
        if (currentState == throwState.IDLE)
        {
            transform.position = parent.transform.position;
            transform.rotation = parent.transform.rotation;
            return;
        }

        if (currentState == throwState.ONARM)
        {
            transform.position = arm.transform.position;
            return;
        }

        transform.Rotate(new Vector3(0, 3, 0));
    }

    private void Update()
    {
        if (currentState != throwState.IDLE && currentState != throwState.ONARM)
        {
            if (Heatseaking)
            {
                target = parent.transform.position;
            }

            transform.position = Vector3.MoveTowards(transform.position, target, velocity * Time.deltaTime);
            velocity += acceleration * 60f * Time.deltaTime;
            velocity = Mathf.Min(velocity, maxVelocity);

            transform.localScale = Vector3.Lerp(transform.localScale, targetSize, 0.125f);

            float distanceToTarget = (transform.position - target).magnitude;
            bool reachedTarget = distanceToTarget < 1f;

            if (reachedTarget && currentState == throwState.THROW)
            {
                Invoke(nameof(startReturn), hangTime);
                currentState = throwState.TRANSITION;
            }

            if (reachedTarget && currentState == throwState.RETURN)
            {
                currentState = throwState.IDLE;
                core.setHasHat(true);
            }
        }
    }
}
