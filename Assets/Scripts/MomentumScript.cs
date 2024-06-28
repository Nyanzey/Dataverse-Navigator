using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MomentumScript : MonoBehaviour
{
    public ContinuousMoveProviderBase moveProvider;

    // Variables for controlling speed and acceleration
    public float acceleration = 1f;
    public float maxSpeed = 5f;
    public float defaultSpeed = 2f;
    private float currentSpeed = 0f;
    private bool isMoving = false;

    void Start()
    {
        if (moveProvider == null)
        {
            Debug.LogError("ContinuousMoveProviderBase component not found.");
        }
        currentSpeed = defaultSpeed;
    }

    void Update()
    {
        if (moveProvider != null)
        {
            // Check if the input for movement is active (customize based on your input system)
            bool inputActive = Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0;

            if (inputActive)
            {
                if (!isMoving)
                {
                    isMoving = true;
                    OnStartMoving();
                }

                // Increase the speed with acceleration, clamping it to the max speed
                currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0, maxSpeed);
                moveProvider.moveSpeed = currentSpeed;
            }
            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    OnStopMoving();
                }
            }
        }
    }

    void OnStartMoving()
    {
        Debug.Log("Started moving");
    }

    void OnStopMoving()
    {
        Debug.Log("Stopped moving");
        // Reset speed when movement stops
        currentSpeed = defaultSpeed;
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = currentSpeed;
        }
    }
}
