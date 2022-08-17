using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial_part3 : MonoBehaviour
{
    //Handle third part of tutorial : Make the user move and think out loud when solving a puzzle

    public GameObject menu;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.gameObject.GetComponent<parent_property>().is_snapped == true)
        {
            GameObject.Find("Tuto_part3").SetActive(false);
            menu.SetActive(true);
        }
    }
}
