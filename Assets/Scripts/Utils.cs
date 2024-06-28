using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;
public class Utils : MonoBehaviour
{
    //Support messages in progress
    public string[] supMessages = {}; 

    public GameObject groupSelector;
    public GameObject clusteringSpacePrefab;
    public GameObject tutorialCanvas;
    public GameObject tutorialCanvasText;
    public GameObject supportButton;
    public GameObject spaceAnchor;
    private bool canvasActive;
    public Color outlineColor;
    public float outlineWidth;
    private bool sphereInScene = false;

    private Dictionary<int, List<GameObject>> currentSelectedPoints = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, Vector3> selectedCenters = new Dictionary<int, Vector3>();

    //Prefab ids
    int clusteringSpaceCount = 0;
    int currentSpaceIdToTeleport = 0;
    Dictionary<int, Vector3> clusteringSpacesIds = new Dictionary<int, Vector3>();
    Vector3 spacesOffset;
    public Vector3 clusteringGapSize = new Vector3(5f, 0, 0);

    private void Start()
    {
        supMessages = new string[] {
        "<Base>\nMovement:\nLeft Joystick\nRotation:\nRight Joystick\nSelection:\nRight Trigger\nOpen menu:\nMenu button",
        "<Selection in progress>\nScale selector:\nButtons X and Y\nGrab selector:\nRight Trigger",
        "<Points selected>\nExpand:\nButton X\nCondense:\nButton Y\nClear selection:\nOpen menu"};
        UpdateCanvas(0);
        spacesOffset = new Vector3(0, 0, 0);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void UpdateCanvas(int mode)
    {
        tutorialCanvasText.GetComponent<TextMeshProUGUI>().text = supMessages[mode];
    }

    public void ToggleCanvas()
    {
        canvasActive = !canvasActive;
        if (canvasActive) { supportButton.GetComponent<TextMeshProUGUI>().text = "Desactivar apoyo"; }
        else { supportButton.GetComponent<TextMeshProUGUI>().text = "Activar apoyo"; }
        tutorialCanvas.SetActive(canvasActive);
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public int getClosestSpaceIdToPlayer()
    {
        if (clusteringSpacesIds.Count == 0) { return 0; }
        Vector3 playerPosition = GameObject.FindGameObjectWithTag("originrig").transform.position;

        int closestKey = 0;
        float closestDistance = float.MaxValue;

        foreach (KeyValuePair<int, Vector3> kvp in clusteringSpacesIds)
        {
            float distance = Vector3.Distance(kvp.Value, playerPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestKey = kvp.Key;
            }
        }

        return closestKey;
    }

    public void spawnGroupSelector()
    {
        GameObject sphereSelector = GameObject.FindGameObjectWithTag("sphereSelector");

        if (!sphereSelector)
        {
            setSphereInScene(true);
            float distanceFromCamera = 5f;
            float rightOffset = 3f;

            // Calculate the spawn position in front of the camera
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * distanceFromCamera + Vector3.right * rightOffset;

            // Instantiate the prefab at the calculated position
            Instantiate(groupSelector, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.Log("A sphere selector is already present");
        }
        
    }

    //Ids start from 0 (returns the id of the recently created space)
    public GameObject createClusteringSpace(int id)
    {
        Vector3 instatiatePosition = spaceAnchor.transform.position + spacesOffset;
        GameObject newSpace = Instantiate(clusteringSpacePrefab, instatiatePosition, Quaternion.identity);
        newSpace.GetComponent<PointHolderScript>().setSpaceId(id);
        clusteringSpaceCount++;
        Debug.Log("Created space with id: " + id);
        return newSpace;
    }

    public void registerClusteringSpace(int id, Vector3 position)
    {
        clusteringSpacesIds.Add(id, position);
    }

    public Vector3 getClusteringSpacePosition(int id)
    {
        if (clusteringSpacesIds.ContainsKey(id))
            return clusteringSpacesIds[id];

        return new Vector3(0, 0, 0);
    }

    public bool spaceIdRegistered(int id)
    {
        return clusteringSpacesIds.ContainsKey(id);
    }

    public Vector3 getNextSpacePositionToTeleport()
    {
        if (clusteringSpacesIds.Count > 0)
        {
            int nextKey = new List<int>(clusteringSpacesIds.Keys)[currentSpaceIdToTeleport % clusteringSpacesIds.Count];
            Vector3 res = getClusteringSpacePosition(nextKey);
            currentSpaceIdToTeleport++;
            return res;
        }
        return new Vector3(0, 0, 0);
    }

    public void deleteClusteringSpace(int id)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("PointHolder");
        GameObject PointHolder = null;

        foreach (GameObject obj in taggedObjects)
        {
            if (obj.GetComponent<PointHolderScript>().getSpaceId() == id)
            {
                PointHolder = obj;
                break;
            }
        }

        if (!PointHolder) { Debug.Log("No point holder with ID:" + id + " was found when deleting."); return; }

        Vector3 reverseOffset = PointHolder.GetComponent<PointHolderScript>().getWidth() + clusteringGapSize;
        Vector3 deletedPosition = PointHolder.transform.position;
        //Check if it actually works like a pointer
        Destroy(PointHolder);

        taggedObjects = GameObject.FindGameObjectsWithTag("PointHolder");
        foreach (GameObject obj in taggedObjects)
        {
            if (obj.transform.position.x > deletedPosition.x)
            {
                obj.transform.position -= reverseOffset;
                clusteringSpacesIds[obj.GetComponent<PointHolderScript>().getSpaceId()] = obj.transform.position;
            }
        }
        spacesOffset -= reverseOffset;
        clusteringSpacesIds.Remove(id);
    }

    public void updateLabels(int spaceId, int[] labels, float[][] colors)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("PointHolder");
        GameObject PointHolder = null;

        foreach (GameObject obj in taggedObjects)
        {
            if (obj.GetComponent<PointHolderScript>().getSpaceId() == spaceId)
            {
                PointHolder = obj;
                break;
            }
        }
        if (!PointHolder) { Debug.Log("No point holder with ID:" + spaceId + " was found when updating selection"); return; }

        int idx;
        foreach (Transform child in PointHolder.transform)
        {
            if (child.gameObject.tag == "SelectablePoint" || child.gameObject.tag == "Selected")
            {
                idx =  int.Parse(child.gameObject.name);
            }
            else
            {
                continue;
            }

            if (labels[idx] == -1)
            {
                child.gameObject.GetComponent<Renderer>().material.color = Color.white;
                continue; 
            }
            child.gameObject.GetComponent<Renderer>().material.color = new Color(colors[labels[idx]][0], colors[labels[idx]][1], colors[labels[idx]][2]);
        }
    }

    public void deselectPoint(int spaceId, int index)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("PointHolder");
        GameObject PointHolder = null;

        foreach (GameObject obj in taggedObjects)
        {
            if (obj.GetComponent<PointHolderScript>().getSpaceId() == spaceId)
            {
                PointHolder = obj;
                break;
            }
        }
        if (!PointHolder) { Debug.Log("No point holder with ID:" + spaceId + " was found when updating selection"); return; }

        foreach (Transform child in PointHolder.transform)
        {
            // Check if the child's name is "target"
            if (child.name == index.ToString())
            {
                disableOutline(child.gameObject);
                break;
            }
        }

    }

    public Vector3 getPosition()
    {
        return spaceAnchor.transform.position + spacesOffset;
    }

    public void updateOffset(Vector3 val)
    {
        spacesOffset += (val + clusteringGapSize);
    }

    public void updateSelectedArray(int spaceId)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("PointHolder");
        GameObject PointHolder = null;

        foreach (GameObject obj in taggedObjects)
        {
            if (obj.GetComponent<PointHolderScript>().getSpaceId() == spaceId)
            {
                PointHolder = obj;
                break;
            }
        }
        if (!PointHolder) { Debug.Log("No point holder with ID:" + spaceId + " was found when updating selection"); return; }

        if (!currentSelectedPoints.ContainsKey(spaceId))
        {
            currentSelectedPoints[spaceId] = new List<GameObject>();
        }

        foreach (Transform child in PointHolder.transform)
        {
            if (child.CompareTag("Selected"))
            {
                currentSelectedPoints[spaceId].Add(child.gameObject);
            }
        }

        selectedCenters[spaceId] = Vector3.zero;
        foreach (GameObject obj in currentSelectedPoints[spaceId])
        {
            selectedCenters[spaceId] += obj.transform.position;
        }
        selectedCenters[spaceId] /= currentSelectedPoints[spaceId].Count;
    }

    public void clearSelectedArray(int spaceId)
    {
        if (currentSelectedPoints.ContainsKey(spaceId))
        {
            foreach (GameObject obj in currentSelectedPoints[spaceId])
            {
                obj.tag = "SelectablePoint";
                disableOutline(obj);
            }

            currentSelectedPoints[spaceId] = new List<GameObject>();
        }
    }

    public void compressSelected(float speed, bool outwards, int spaceId)
    {
        if (currentSelectedPoints[spaceId].Count == 0)
        {
            Debug.LogWarning("No game objects selected to move outwards!");
            return;
        }

        // Move each object outwards from the center
        foreach (GameObject obj in currentSelectedPoints[spaceId])
        {
            Vector3 direction = (obj.transform.position - selectedCenters[spaceId]).normalized;
            if (!outwards) { direction = -direction; }
            obj.transform.position += direction * speed * Time.deltaTime;
        }
    }

    public void setSphereInScene(bool state){sphereInScene = state;}
    public bool getSphereInScene() { return sphereInScene; }

    public void enableOutline(GameObject obj)
    {
        if (obj.GetComponent<Outline>() != null)
        {
            obj.GetComponent<Outline>().enabled = true;
        }
        else
        {
            Outline outline = obj.AddComponent<Outline>();
            outline.enabled = true;
            obj.GetComponent<Outline>().OutlineColor = outlineColor;
            obj.GetComponent<Outline>().OutlineWidth = outlineWidth;
        }
    }

    public void disableOutline(GameObject obj)
    {
        if (obj && obj.GetComponent<Outline>())
        {
            obj.GetComponent<Outline>().enabled = false;
        }
        else
        {
            Debug.Log("Failed disabling outline (null object)");
        }
    }
}
