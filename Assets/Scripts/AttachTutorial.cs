using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachTutorial : MonoBehaviour
{
    // Offset from the camera
    public Vector3 positionOffset = new Vector3(0, 0, 2);
    public Vector3 rotationOffset = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    void Start()
    {
        // Attach the canvas to the camera
        transform.SetParent(Camera.main.transform);

        // Set the local position and rotation to the offset values
        transform.localPosition = positionOffset;
        transform.localRotation = Quaternion.Euler(rotationOffset);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
