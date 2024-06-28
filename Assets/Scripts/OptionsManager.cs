using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    string[] clusteringMethods = { "DBScan", "KMeans", "KMedoids" }, // Add Hierarchical Clustering
             reductionMethods = { "UMAP", "PCA", "TSNE" }; // Add LDA
    public GameObject[] clusteringParameters = new GameObject[3],
                        reductionParameters = new GameObject[3];
    public TMP_Dropdown clusteringSelector, reductionSelector;

    [Header("UMAP Parameters values")]
    public TMP_Text nNeighborsValue;
    public TMP_Text spreadValue;

    [Header("Visualization Parameters values")]
    public TMP_Text pointSizeValue;

    public GameObject pointGenerator;

    static int clusteringMethod = 0, reductionMethod = 0;
    private int UMAPnNeighbors;
    private float UMAPspread;
    private float pointsSize;
    public GameObject MenuCanvas;

    void Start()
    {
        clusteringSelector.value = clusteringMethod;
        reductionSelector.value = reductionMethod;
        RefreshOptions();
    }

    public void UpdateClusteringMethod()
    {
        clusteringMethod = clusteringSelector.value;
        RefreshOptions();
    }

    public void UpdateReductionMethod()
    {
        reductionMethod = reductionSelector.value;
        RefreshOptions();
    }

    private void RefreshOptions()
    {
        for (int i = 0; i < 3; i++)
        {
            clusteringParameters[i].SetActive(i == clusteringMethod);
            reductionParameters[i].SetActive(i == reductionMethod);
        }
    }

    public void ApplyChanges()
    {
        UMAPnNeighbors = int.Parse(nNeighborsValue.text);
        UMAPspread = float.Parse(spreadValue.text);
        pointsSize = float.Parse(pointSizeValue.text);

        DataPlotter plotterScript = pointGenerator.GetComponent<DataPlotter>();

        /*plotterScript.clearPoints();
        Global.ApplyUmap(UMAPnNeighbors);
        plotterScript.plot();*/
        plotterScript.UpdatePointSize(pointsSize);

        // Debug.Log("nNeighbors: " + UMAPnNeighbors + ", spread: " + UMAPspread + ", pointsSize: " + pointsSize);
        gameObject.SetActive(false);
        MenuCanvas.SetActive(false);        
    }

    
}
