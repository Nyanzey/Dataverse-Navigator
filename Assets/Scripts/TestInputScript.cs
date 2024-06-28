using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInputScript : MonoBehaviour
{
    Utils utilsScript;


    // Start is called before the first frame update
    void Start()
    {
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();
    }

    // Update is called once per frame
    void Update()
    {
        // Call the MoveObjects function with a velocity value
        if (Input.GetKeyDown(KeyCode.J))
        {
            Vector3 newPos = utilsScript.getNextSpacePositionToTeleport();
            GameObject VRRig = GameObject.FindGameObjectWithTag("originrig");
            if (VRRig)
            {
                Debug.Log("Found VR Rig");
                VRRig.transform.position = newPos;
            }
            Debug.Log("New Pos: " + newPos);
            
        }
        if (Input.GetKey(KeyCode.K))
        {
            utilsScript.compressSelected(3f, false, utilsScript.getClosestSpaceIdToPlayer()); // Move inwards with velocity 5
        }
    }
}
