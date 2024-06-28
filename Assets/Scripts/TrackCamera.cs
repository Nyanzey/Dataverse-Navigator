using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCamera : MonoBehaviour
{
    public Camera mainCamera;
    public float distance = 10;
    public bool updatePerFrame;
    public float xOffset = 0;
    public float yOffset = 0;
    

    void Start()
    {
        
    }

    void OnEnable()
    {
        UpdateTracking();
    }

    void Update()
    {
        if (updatePerFrame)
        {
            UpdateTracking();
        }
    }

    void UpdateTracking()
    {
        Vector3 forwardDirection = mainCamera.transform.forward;
        forwardDirection += xOffset * mainCamera.transform.right;
        forwardDirection += yOffset * mainCamera.transform.up;
        
        Vector3 cameraPosition = mainCamera.transform.position;

        this.transform.position = cameraPosition + distance * forwardDirection;
        this.transform.rotation = Quaternion.LookRotation(forwardDirection);
    }
}
