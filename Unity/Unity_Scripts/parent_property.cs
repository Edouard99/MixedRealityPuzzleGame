using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parent_property : MonoBehaviour
{
    /*
     * This class is containing and updating state of the parent piece
     */

    public bool just_released=false;
    public bool just_picked = false;
    public bool is_snapped = false;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener((x) => {
            just_released = true;
            //Triggered when the player release the piece
        });
        this.gameObject.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener((x) => {
            just_picked = true;
            is_snapped = false;
            //Triggered when the player pick up the piece
        });
        this.gameObject.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener((x) => {
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            //Triggered when the player pick up the piece
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
