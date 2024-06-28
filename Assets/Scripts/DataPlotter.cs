using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class DataPlotter : MonoBehaviour
{
    public float plotScale = 10;

    // The prefab for the data points that will be instantiated
    public GameObject PointPrefab;

    // Object which will contain instantiated prefabs in hiearchy
    public GameObject PointHolder;

    // Use this for initialization
    void Start()
    {
        
    }

    public void EnclosePoints()
    {
        PointHolder.GetComponent<PointHolderScript>().CreateTransparentCube();
    }

    bool updatePointHolderReference(int id)
    {
        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("PointHolder");

        foreach (GameObject obj in taggedObjects)
        {
            if (obj.GetComponent<PointHolderScript>().getSpaceId()==id)
            {
                PointHolder = obj;
                return true;
            }
        }
        return false;
    }

    public void Plot(float[][] points, int pointHolderId)
    {

        Utils utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();

        if (updatePointHolderReference(pointHolderId))
        {
            Debug.Log("Found space to delete");
            utilsScript.deleteClusteringSpace(pointHolderId);
        }

        PointHolder = utilsScript.createClusteringSpace(pointHolderId);
        if (!PointHolder) 
        {
            Debug.Log("Couldnt create space to plot");
            return;
        }

        if (points.Length == 0)
        {
            Debug.Log("Dataset esta vacio");
            return;
        }

        //ClearPoints();
        PointHolder.GetComponent<PointHolderScript>().clearPoints();
        Debug.Log(points.Length);
        var pmax = (float[]) points[0].Clone();
        var pmin = (float[]) points[0].Clone();
        foreach (var pnt in points)
        {
            for (var i = 0; i < 3; i++)
            {
                pmax[i] = Math.Max(pmax[i], pnt[i]);
                pmin[i] = Math.Min(pmin[i], pnt[i]);
            }
        }

        //Loop through Pointlist
        for (var i = 0; i < points.Length; i++)
        {
            Debug.Log("plotting: " + i);
            // Get value in poinList at ith "row", in "column" Name, normalize
            float x = (points[i][0] - pmin[0]) / (pmax[0] - pmin[0]);
            float y = (points[i][1] - pmin[1]) / (pmax[1] - pmin[1]);
            float z = (points[i][2] - pmin[2]) / (pmax[2] - pmin[2]);

            // Instantiate as gameobject variable so that it can be manipulated within loop
            GameObject dataPoint = Instantiate(
                    PointPrefab,
                    new Vector3(x, y, z) * plotScale,
                    Quaternion.identity);

            // Make child of PointHolder object, to keep points within container in hiearchy
            dataPoint.transform.parent = PointHolder.transform;

            // Set parent space id
            dataPoint.GetComponent<HoverControlScript>().setParentSpaceId(pointHolderId);

            // Assigns name to the prefab
            dataPoint.transform.name = i.ToString();

            // Gets material color and sets it to a new RGB color we define
            dataPoint.GetComponent<Renderer>().material.color = Color.white;
        }
    }

    public void UpdatePointSize(float scale)
    {
        GameObject[] selectablePoints = GameObject.FindGameObjectsWithTag("SelectablePoint");
        GameObject[] selectedPoints = GameObject.FindGameObjectsWithTag("Selected");

        foreach (GameObject point in selectablePoints)
        {
            point.transform.localScale = new Vector3(scale, scale, scale);
        }

        foreach (GameObject point in selectedPoints)
        {
            point.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void ClearPoints()
    {
        GameObject[] selectablePoints = GameObject.FindGameObjectsWithTag("SelectablePoint");
        GameObject[] selectedPoints = GameObject.FindGameObjectsWithTag("Selected");

        foreach (GameObject point in selectablePoints)
        {
            Destroy(point);
        }

        foreach (GameObject point in selectedPoints)
        {
            Destroy(point);
        }
    }
}