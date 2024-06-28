using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class GroupSelectorScript : MonoBehaviour
{
    public float radiusIncreaseRate;
    Utils utilsScript;
    private GameObject ConnectionManager;

    //Variables to handle movement
    private Vector3 lastPosition;
    private bool isMoving;
    private XRRayInteractor rayCast;
    private float currentSpeed;
    private GameObject RayInteractor;

    public float defaultSpeed = 1f;
    public float acceleration = 1f;
    public float maxSpeed = 5f;
    public float movementThreshold = 0.001f;

    void Start()
    {
        ConnectionManager =  GameObject.Find("ConnectionManager");
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();
        RayInteractor = GameObject.FindGameObjectWithTag("raycast");
        rayCast = RayInteractor.GetComponent<XRRayInteractor>();
        lastPosition = transform.position;
        isMoving = false;
    }
    void Update()
    {
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        float distanceMoved = Vector3.Distance(lastPosition, transform.position);
        // Check if the distance moved is greater than the threshold
        if (distanceMoved > movementThreshold)
        {
            if (!isMoving)
            {
                isMoving = true;
            }
            // Apply acceleration to current velocity
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0, maxSpeed);
            rayCast.translateSpeed = currentSpeed;
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                OnStopMoving();
            }
        }

        // Update lastPosition for the next frame
        lastPosition = transform.position;
    }

    void OnStopMoving()
    {
        // Reset speed when movement stops
        currentSpeed = defaultSpeed;
        rayCast.translateSpeed = currentSpeed;
    }

    //Clear selection on selected objects
    public void Deselect()
    {
        // To make sure ConnectionManager is not null (when deselecting, there may not be a selector sphere in scene and thus not executing start function)
        ConnectionManager = GameObject.Find("ConnectionManager");
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();

        utilsScript.clearSelectedArray(utilsScript.getClosestSpaceIdToPlayer());
        ConnectionManager.GetComponent<ConnectionScript>().SendClearSelection(utilsScript.getClosestSpaceIdToPlayer());
    }

    //Select objects colliding with sphere selector
    public void Select()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x/2f);
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();

        var indexes = new List<int>();
        int spaceId = 0;
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("SelectablePoint"))
            {
                collider.gameObject.tag = "Selected";
                indexes.Add(int.Parse(collider.gameObject.name));
                utilsScript.enableOutline(collider.gameObject);
                spaceId = collider.gameObject.GetComponent<HoverControlScript>().getParentSpaceId();
            }
        }

        // Enviar seleccion al controlador
        ConnectionManager.GetComponent<ConnectionScript>().SendSelection(spaceId, indexes);

        utilsScript.setSphereInScene(false);
        utilsScript.updateSelectedArray(utilsScript.getClosestSpaceIdToPlayer());
        utilsScript.UpdateCanvas(0);
        Destroy(gameObject);
    }

    public void scaleArea(float sign)
    {
        transform.localScale += new Vector3(1f, 1f, 1f)*radiusIncreaseRate*sign;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SelectablePoint"))
        {
            utilsScript.enableOutline(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SelectablePoint"))
        {
            utilsScript.disableOutline(other.gameObject);
        }
    }

}
