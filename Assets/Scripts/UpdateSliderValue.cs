using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSliderValue : MonoBehaviour
{
    [Header("Configuration")]
    public Slider slider;
    public TMP_Text value;
    public bool isInt = true;
    // Start is called before the first frame update
    void Start()
    {
        UpdateTextValue();
    }

    public void UpdateTextValue()
    {
        if (isInt)
            value.text = String.Format("{0:F0}", slider.value);
        else
            value.text = String.Format("{0:F2}", slider.value);
    }
}
