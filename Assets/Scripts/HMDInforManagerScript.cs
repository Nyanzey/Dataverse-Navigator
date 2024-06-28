using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HMDInforManagerScript : MonoBehaviour
{
    public GameObject VRSimulator;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Is Device Active: " + XRSettings.isDeviceActive);
        Debug.Log("Device name is: " + XRSettings.loadedDeviceName);

        if (!XRSettings.isDeviceActive)
        {
            Instantiate(VRSimulator);
            Debug.Log("No headset detected, instantiating simulator.");
        }
        else
        {
            Debug.Log("Detected headset with name (" + XRSettings.loadedDeviceName + ")");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
