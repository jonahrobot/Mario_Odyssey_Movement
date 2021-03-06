using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetScript : MonoBehaviour
{
    // Used this article to learn how to simulate this
    // https://levelup.gitconnected.com/implement-a-third-person-camera-just-like-mario-odyssey-in-unity-e21744911733 


    Transform PlayerMesh;
    PlayerStateMachineCore core;
    Camera mainCamera;

    float yPosition;
    bool DidLeaveGroundAction;
    Vector3 vel;

    private void Start()
    {
        mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
        core = GameObject.Find("Player").GetComponent<PlayerStateMachineCore>();
        PlayerMesh = GameObject.Find("CameraTargetLink").transform;
    }

    private void Update()
    {
        if (core.isGrounded == false && DidLeaveGroundAction == false)
        {
            DidLeaveGroundAction = true;
            yPosition = PlayerMesh.position.y;
        }

    }

    private void LateUpdate()
    {
        Vector3 characterViewPosition = mainCamera.WorldToViewportPoint(PlayerMesh.position);

        if (characterViewPosition.y > 0.95f || characterViewPosition.y < 0.3f)
        {
            yPosition = PlayerMesh.position.y;
        }else if (core.isGrounded)
        {
            DidLeaveGroundAction = false;
            yPosition = PlayerMesh.position.y;
        }
        var desiredPosition = new Vector3(PlayerMesh.position.x, yPosition, PlayerMesh.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref vel, 0.25f);
    }
}
