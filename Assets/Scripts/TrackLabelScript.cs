using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackLabelScript : MonoBehaviour
{
    private Camera mainCamera;
    public float distanceFromCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        updatePos();
    }


    public void updatePos()
    {
        // Get the direction from the canvas to the camera
        Vector3 directionToCamera = transform.position - mainCamera.transform.position;

        // Rotate the canvas to face the camerawss
        transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

        // Optionally, adjust canvas position to maintain a certain distance from the camera // Adjust this value to set the desired distance from the camera
        transform.position = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera + Vector3.left*2f;
    }

}
