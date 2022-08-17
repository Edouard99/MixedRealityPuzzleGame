using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Snap : MonoBehaviour
{
    // Handle Snapping interaction for each piece


    //private Vector3 gridSize = new Vector3(1,1,1);
    public List<GameObject> piece_cubes= new List<GameObject>();
    public List<GameObject> piece_cubes_collider = new List<GameObject>();
    //public float xpos;
    public List<GameObject> aim_grid_cubes = new List<GameObject>();
    public List<float> distance = new List<float>();
    public List<GameObject> closest_go = new List<GameObject>();
    public List<GameObject> closest_go_collider = new List<GameObject>();
    public float threshold = 0.04f;
    public bool is_snapped;
    /*public Vector3 lrota;
    public float rota;
    public float euler;
    public float leuler;
    public Quaternion all_rota;
    public Quaternion rota_corrected;*/
    //public GameObject parent_emul;
    //public Vector3 obj_pos;
    //public Vector3 vect_target;
    //public Vector3 parent_prepos;
    //public Vector3 parent_target;
    //public Vector3 parent_postpos;


    public float FindDistanceClosestObject(GameObject[] cubes,GameObject cubeReference)
    {
        //Find the closest object to cubeReference and return the distance between them
        GameObject closest = null;
        float distance = Mathf.Infinity;
        foreach (GameObject go in cubes)
        {
            Vector3 diff = go.transform.position - cubeReference.transform.position;
            float curDistance = (float)Math.Sqrt(diff.sqrMagnitude);
            if (curDistance< distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return distance;
    }

    public GameObject FindClosestObject(GameObject[] cubes, GameObject cubeReference)
    {
        //Find the closest object to cubeReference and return the nearest object
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 diff_vect = new Vector3(0,0,0);
        foreach (GameObject go in cubes)
        {
            Vector3 diff = go.transform.position - cubeReference.transform.position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
                diff_vect = diff;
            }
        }
        return closest;
    }



    // Start is called before the first frame update
    void Start()
    {
        //Initialization
        for (int a=0; a < transform.childCount; a++)
        {
            if (this.gameObject.transform.GetChild(a).gameObject.tag == "small_cube")
            {
                piece_cubes.Add(this.gameObject.transform.GetChild(a).gameObject);
                distance.Add(0);
            }

        }
        is_snapped = false;

    }

    public void freeze()
    {
        //Freeze a gameObject with rigidbody properties
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Calculate closest location where the object can be snapped. If the maximum distance between this location and the piece is below the threshhold, the piece is moved to the location and rotated
        if (this.gameObject.GetComponent<parent_property>().just_released == true)
        {
            closest_go = new List<GameObject>();
            int a = 0;
            foreach (GameObject cube in piece_cubes)
            {
                distance[a] = FindDistanceClosestObject(GameObject.FindGameObjectsWithTag("aim_cube"), cube);
                closest_go.Add(FindClosestObject(GameObject.FindGameObjectsWithTag("aim_cube"), cube));
                a++;
            }
            if (distance.Max() < threshold)
            {
                Vector3 vec = transform.eulerAngles;
                vec.x = Mathf.Round(vec.x / 90) * 90;
                vec.y = Mathf.Round(vec.y / 90) * 90;
                vec.z = Mathf.Round(vec.z / 90) * 90;
                transform.eulerAngles = vec;
                Vector3 target = closest_go[0].transform.position - piece_cubes[0].transform.position;
                transform.position = transform.position + target;
                freeze();
                this.gameObject.GetComponent<parent_property>().just_released = false;
                this.gameObject.GetComponent<parent_property>().is_snapped = true;
            }
            else
            {
                freeze();
                this.gameObject.GetComponent<parent_property>().just_released = false;
            }
        }
    }

}
