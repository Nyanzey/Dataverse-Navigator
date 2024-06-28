using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCanvas : MonoBehaviour
{
    public Camera mainCamera;
    public float distance;

    void Start()
    {

    }

    void OnDisable()
    {

    }

    void OnEnable()
    {
        Vector3 forwardDirection = mainCamera.transform.forward;
        Vector3 cameraPosition = mainCamera.transform.position;

        this.transform.position = cameraPosition + distance * forwardDirection;
        this.transform.rotation = Quaternion.LookRotation(forwardDirection);
    }

    void Update()
    {
        
    }
}
