using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetScript : MonoBehaviour
{
    // Used this article to learn how to simulate this
    // https://levelup.gitconnected.com/implement-a-third-person-camera-just-like-mario-odyssey-in-unity-e21744911733 


    Transform PlayerMesh;
    State_Context_Handler _stateContext;
    Camera mainCamera;

    float yPosition;
    bool DidLeaveGroundAction;
    Vector3 vel;

    bool StartTrackingCamera = false;

    private void Start()
    {
        mainCamera = GameObject.Find("Camera").GetComponent<Camera>();

        _stateContext = GameObject.Find("Player").GetComponent<PlayerStateMachineCore>().StateContext;
        PlayerMesh = GameObject.Find("CameraTargetLink").transform;
    }

    private void Update()
    {
        if (_stateContext.IsGrounded == false && DidLeaveGroundAction == false)
        {
            DidLeaveGroundAction = true;
        }

    }

    private void LateUpdate()
    {
        // Only follow player if on ground
        if (_stateContext.IsGrounded)
        {
            // If not recovering from Jump, follow exact
            if (DidLeaveGroundAction == false)
            {
                transform.position = PlayerMesh.position;
            }
            else
            {
                // Recovering from jump
                transform.position = Vector3.SmoothDamp(transform.position, PlayerMesh.position, ref vel, 0.125f);
                if (Vector3.Distance(transform.position, PlayerMesh.position) < 0.025)
                {
                    DidLeaveGroundAction = false;
                }
            }
        }
   
        // Check if moving through air
        if(_stateContext.IsGrounded == false && _stateContext.IsMoving)
        {
            // Should follow Lightly
            var desiredPosition = new Vector3(PlayerMesh.position.x, transform.position.y, PlayerMesh.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref vel, 0.25f);
            if (Vector3.Distance(transform.position, PlayerMesh.position) < 0.025)
            {
                DidLeaveGroundAction = false;
            }
        }
      
    }
}
