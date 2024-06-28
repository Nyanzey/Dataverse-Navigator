using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverControlScript : MonoBehaviour
{
    private Vector3 lastPosition;
    private bool isMoving;
    private bool isFatherOfGroup;
    private XRRayInteractor rayCast;
    private float selectedCurrentSpeed;
    private GameObject RayInteractor;
    private GameObject ConnectionManager;

    public float selectedDefaultSpeed = 1f;
    public float selectedAcceleration = 1f;
    public float selectedMaxSpeed = 5f;
    public float movementThreshold = 0.001f;

    // For points only
    private int parentSpaceId;
    
    public void setParentSpaceId(int id) { parentSpaceId = id; }
    public int getParentSpaceId() { return parentSpaceId; }

    // Start is called before the first frame update
    void Start()
    {
        isFatherOfGroup = false;
        RayInteractor = GameObject.FindGameObjectWithTag("raycast");
        rayCast = RayInteractor.GetComponent<XRRayInteractor>();
        lastPosition = transform.position;
        isMoving = false;
        ConnectionManager = GameObject.Find("ConnectionManager");
    }

    // Update is called once per frame
    void Update()
    {
        if (isFatherOfGroup || tag == "SelectablePoint")
        {
            UpdateGroupSpeed();
        }
    }

    private void UpdateGroupSpeed()
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
            selectedCurrentSpeed = Mathf.Clamp(selectedCurrentSpeed + selectedAcceleration * Time.deltaTime, 0, selectedMaxSpeed);
            rayCast.translateSpeed = selectedCurrentSpeed;
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
        selectedCurrentSpeed = selectedDefaultSpeed;
        rayCast.translateSpeed = selectedCurrentSpeed;
    }

    public void LoadPanel()
    {
        ConnectionManager.GetComponent<ConnectionScript>().SendRequestImage(parentSpaceId, int.Parse(name));
        /* // Find the game object with the specified tag
        GameObject parentObj = GameObject.FindWithTag("LabelUI");

        // Check if the object was found
        if (parentObj != null)
        {
            // Find the child object by name (including inactive ones)
            Transform childTransform = FindChildRecursive(parentObj.transform, "Panel");

            // Check if the child object was found
            if (childTransform != null)
            {
                // Access the child object
                GameObject childObj = childTransform.gameObject;
                SetPanelValues(childObj);
                //updateLabelPos();
                childObj.SetActive(true);

                // Do something with the child object
                //Debug.Log("Found child object: " + childObj.name);
            }
            else
            {
                //Debug.Log("Child object not found.");
            }
        }
        else
        {
            //Debug.Log("Object with tag 'YourTag' not found.");
        } */
    }

    public void moveGroup() 
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("Selected");

        foreach (GameObject point in points)
        {
            if (ReferenceEquals(point, gameObject)) { continue; }
            point.transform.parent = gameObject.transform;
        }

        isFatherOfGroup = true;
    }

    public void deattachGroup()
    {
        GameObject[] points = GameObject.FindGameObjectsWithTag("Selected");

        foreach (GameObject point in points)
        {
            point.transform.parent = gameObject.transform.parent;
        }

        isFatherOfGroup = false;
    }
    
    public void UnloadPanel()
    {
        // Find the game object with the specified tag
        GameObject parentObj = GameObject.FindWithTag("LabelUI");

        // Check if the object was found
        if (parentObj != null)
        {
            // Find the child object by name (including inactive ones)
            Transform childTransform = FindChildRecursive(parentObj.transform, "Panel");

            // Check if the child object was found
            if (childTransform != null)
            {
                // Access the child object
                GameObject childObj = childTransform.gameObject;
                childObj.SetActive(false);

                // Do something with the child object
                //Debug.Log("Found child object: " + childObj.name);
            }
            else
            {
                //Debug.Log("Child object not found.");
            }
        }
        else
        {
            //Debug.Log("Object with tag 'YourTag' not found.");
        }
    }

    void SetPanelValues(GameObject panel)
    {
		Transform title = FindChildRecursive(panel.transform, "Title"); //FindChildRecursive(FindChildRecursive(FindChildRecursive(panel.transform, "TitleData"),"Viewport"),"Content");
        Transform image = FindChildRecursive(panel.transform, "Image");
        Transform coordinates = FindChildRecursive(panel.transform, "Coordinates"); //FindChildRecursive(FindChildRecursive(FindChildRecursive(panel.transform, "PointData"), "Viewport"), "Content");
        
        if (image == null) Debug.Log("image panel object not found.");
        if (title == null) Debug.Log("title panel object not found.");
        if (coordinates == null) Debug.Log("coordinates panel object not found.");

        // Get the TextMeshPro component attached to the GameObject
        TextMeshProUGUI titleText = title.GetComponent<TextMeshProUGUI>();
        titleText.text = name;

        float x = gameObject.transform.position.x;
        float y = gameObject.transform.position.y;
        float z = gameObject.transform.position.z;

        TextMeshProUGUI coordinatesText = coordinates.GetComponent<TextMeshProUGUI>();
        coordinatesText.text = string.Format("{0:.##} {1:.##} {2:.##}", x, y, z);
		
		// Debug.Log(GlobalVars.Data.FolderPath + "\\" + name);
		// Para cargar imagenes del disco
		// Puede que haya una fuga de memoria aca
		Texture2D tex = new Texture2D(400, 400);
		tex.LoadImage(System.IO.File.ReadAllBytes(Global.Data.FolderPath + "\\" + name));
		image.GetComponent<UnityEngine.UI.Image>().overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            // Recursively search through children
            Transform result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }

        // Child not found
        return null;
    }

}
