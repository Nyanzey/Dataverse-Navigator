using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointHolderScript : MonoBehaviour
{
    public Color borderColor = Color.green;
    public Color shadowColor = Color.black;
    public float borderWidth = 0.05f;
    public Material cubeMat;
    public Material borderMat;
    Bounds bounds;
    public Vector3 initialPosition;
    public Vector3 offsetPosition;

    Utils utilsScript;

    //For clustering spaces
    int spaceId;

    void Start()
    {
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();
    }

    public void setSpaceId(int id)
    { 
        spaceId = id;
        Debug.Log("Im point holder with id: " + spaceId);
    }
    public int getSpaceId() { return spaceId; }

    public void CreateTransparentCube()
    {
        utilsScript = GameObject.FindGameObjectWithTag("menu").GetComponent<Utils>();
        bounds = new Bounds(transform.GetChild(0).position, Vector3.zero);
        foreach (Transform point in transform)
        {
            if (point.gameObject.tag == "SelectablePoint")
            {
                bounds.Encapsulate(point.position);
            }
        }

        /*
        // Create the cube object
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = bounds.center;
        cube.transform.localScale = bounds.size;

        // Create a material for the transparent cube
        Material transparentMaterial = cubeMat;
        transparentMaterial.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a); // Adjust the alpha for transparency

        // Apply the material to the cube
        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.material = transparentMaterial;

        cube.transform.parent = transform;
        cube.layer = 3;

        // Create the border
        CreateCubeBorders(cube.transform, bounds, borderColor, borderWidth);*/

        // Create the cube object
        //transform.position = bounds.center;
        //transform.localScale = bounds.size;
        TransformParentOnly(gameObject, bounds.size, bounds.center);
        Debug.Log("bounds size: " + bounds.size);

        // Create a material for the transparent cube
        Material transparentMaterial = cubeMat;
        transparentMaterial.color = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a); // Adjust the alpha for transparency

        // Apply the material to the cube
        Renderer renderer = gameObject.GetComponent<Renderer>();
        renderer.material = transparentMaterial;

        //transform.parent = transform;
        gameObject.layer = 3;

        // Create the border
        CreateCubeBorders(transform, bounds, borderColor, borderWidth);

        transform.position = utilsScript.getPosition();
        utilsScript.registerClusteringSpace(spaceId, transform.position);
        utilsScript.updateOffset(new Vector3(bounds.size.x, 0, 0));
    }

    public void ClearBorders()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "Cube")
            {
                Destroy(child.gameObject);
            }
        }
    }

    public Vector3 getWidth()
    {
        return new Vector3(bounds.size.x, 0, 0);
    }

    public void TransformParentOnly(GameObject parent, Vector3 newScale, Vector3 newPos)
    {
        // Store the original parent
        Transform[] children = new Transform[parent.transform.childCount];
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            children[i] = parent.transform.GetChild(i);
        }

        // Detach children
        foreach (Transform child in children)
        {
            child.SetParent(null);
        }

        // Transform the parent
        parent.transform.localScale = newScale;
        parent.transform.position = newPos;

        // Reattach children
        foreach (Transform child in children)
        {
            child.SetParent(parent.transform);
        }
    }

    void CreateCubeBorders(Transform parentCube, Bounds bounds, Color color, float width)
    {
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;
        Vector3 halfSize = size / 2;

        // Create borders along the edges of the cube
        CreateBorder(parentCube, new Vector3(halfSize.x, halfSize.y, 0) + center, new Vector3(width, width, size.z), color);
        CreateBorder(parentCube, new Vector3(halfSize.x, -halfSize.y, 0) + center, new Vector3(width, width, size.z), color);
        CreateBorder(parentCube, new Vector3(-halfSize.x, halfSize.y, 0) + center, new Vector3(width, width, size.z), color);
        CreateBorder(parentCube, new Vector3(-halfSize.x, -halfSize.y, 0) + center, new Vector3(width, width, size.z), color);

        CreateBorder(parentCube, new Vector3(halfSize.x, 0, halfSize.z) + center, new Vector3(width, size.y, width), color);
        CreateBorder(parentCube, new Vector3(halfSize.x, 0, -halfSize.z) + center, new Vector3(width, size.y, width), color);
        CreateBorder(parentCube, new Vector3(-halfSize.x, 0, halfSize.z) + center, new Vector3(width, size.y, width), color);
        CreateBorder(parentCube, new Vector3(-halfSize.x, 0, -halfSize.z) + center, new Vector3(width, size.y, width), color);

        CreateBorder(parentCube, new Vector3(0, halfSize.y, halfSize.z) + center, new Vector3(size.x, width, width), color);
        CreateBorder(parentCube, new Vector3(0, halfSize.y, -halfSize.z) + center, new Vector3(size.x, width, width), color);
        CreateBorder(parentCube, new Vector3(0, -halfSize.y, halfSize.z) + center, new Vector3(size.x, width, width), color);
        CreateBorder(parentCube, new Vector3(0, -halfSize.y, -halfSize.z) + center, new Vector3(size.x, width, width), color);
    }

    void CreateBorder(Transform parentCube, Vector3 position, Vector3 scale, Color color)
    {
        GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
        border.transform.position = position;
        border.transform.localScale = scale;

        Material borderMaterial = borderMat;
        borderMaterial.color = color;
        Renderer borderRenderer = border.GetComponent<Renderer>();
        borderRenderer.material = borderMaterial;

        // Optionally disable the collider for the border cube
        Destroy(border.GetComponent<Collider>());
        border.transform.parent = parentCube;
        border.layer = 3;
    }

    public void clearPoints()
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.tag == "SelectablePoint" || child.gameObject.tag == "Selected")
                {
                    Destroy(child.gameObject);
                }
            }
        }
        else
        {
            Debug.Log("No children found for space with ID: " + spaceId);
        }
    }

}

