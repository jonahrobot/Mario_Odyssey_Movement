using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetScript : MonoBehaviour
{
    // Used this article to learn how to simulate this
    // https://levelup.gitconnected.com/implement-a-third-person-camera-just-like-mario-odyssey-in-unity-e21744911733 
    

    Transform PlayerMesh;

    private void Start()
    {
        PlayerMesh = GameObject.Find("CameraTargetLink").transform;
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(PlayerMesh.position.x, PlayerMesh.position.y, PlayerMesh.position.z);
    }
}
