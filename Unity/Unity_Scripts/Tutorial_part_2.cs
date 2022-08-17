using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_part_2 : MonoBehaviour
{
    //Handle Second part of tutorial : learn to the user how to use sliders


    public GameObject slider1;
    public GameObject slider2;
    public GameObject menu;
    public GameObject video;
    public GameObject tuto3;
    // Start is called before the first frame update
    void OnEnable()
    {
        slider1.GetComponent<PinchSlider>().SliderValue = 0.5f;
        slider2.GetComponent<PinchSlider>().SliderValue = 0.5f;
        video.SetActive(true);
    }

    public void Confirm()
    {
        if (slider1.GetComponent<PinchSlider>().SliderValue==0.875f && slider2.GetComponent<PinchSlider>().SliderValue==0.25f)
        {
            GameObject.Find("Tuto_part2").SetActive(false);
            video.SetActive(false);
            tuto3.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
