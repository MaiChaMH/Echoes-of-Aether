using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScript : MonoBehaviour
{
    private Transform CameraP;
    // Start is called before the first frame update
    void Start()
    {
        CameraP = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = transform.position;
        newPosition.x = CameraP.position.x;
        newPosition.y = CameraP.position.y;
        transform.position = newPosition;
    }
}
