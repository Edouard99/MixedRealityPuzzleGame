using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial_part1 : MonoBehaviour
{
    //Handle first part of tutorial :  learn to the user how to manipulate a piece


    public Vector3 start_position = new Vector3(1,0,1.5f);
    public Quaternion start_rotation = Quaternion.identity;
    public int number_sucess;
    public GameObject menu;
    public GameObject text_tuto;
    public GameObject video0;
    public GameObject video1;
    public GameObject video2;
    public GameObject video3;
    public GameObject video4;
    public GameObject tuto_part2;



    void OnEnable()
    {
        this.gameObject.transform.position = start_position;
        this.gameObject.transform.rotation = start_rotation;
        number_sucess = 0;
        text_tuto.GetComponent<Text>().text = "Move the piece with one hand";
        video0.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //Handle Video and instructions displays at each success of the user
        
        if (number_sucess < 10)
        {
            if (this.gameObject.GetComponent<parent_property>().is_snapped == true)
            {
                number_sucess++;
                switch (number_sucess)
                {
                    case 1:
                        text_tuto.GetComponent<Text>().text = "Once again with one hand";
                        video0.GetComponent<AudioSource>().Play();
                        break;
                    case 2:
                        text_tuto.GetComponent<Text>().text = "Move the piece with two hands";
                        video0.SetActive(false);
                        video1.SetActive(true);
                        break;
                    case 3:
                        text_tuto.GetComponent<Text>().text = "Once again with two hands";
                        video1.GetComponent<AudioSource>().Play();
                        break;
                    case 4:
                        text_tuto.GetComponent<Text>().text = "Move the piece with one hand from far position. The piece is selected when the ray is continuous, when the end of ray point is on the piece";
                        video1.SetActive(false);
                        video2.SetActive(true);
                        break;
                    case 5:
                        text_tuto.GetComponent<Text>().text = "Once again with one hand from far position. The piece is selected when the ray is continuous, when the end of ray point is on the piece";
                        video2.GetComponent<AudioSource>().Play();
                        break;
                    case 6:
                        text_tuto.GetComponent<Text>().text = "Move the piece with two hands from far position. The piece is selected when the ray is continuous, when the end of ray point is on the piece";
                        video2.SetActive(false);
                        video3.SetActive(true);
                        break;
                    case 7:
                        text_tuto.GetComponent<Text>().text = "Once again with two hands from far position. The piece is selected when the ray is continuous, when the end of ray point is on the piece";
                        video3.GetComponent<AudioSource>().Play();
                        break;
                    case 8:
                        text_tuto.GetComponent<Text>().text = "Move and rotate the Piece. You can move around the piece for better rotation angle";
                        video3.SetActive(false);
                        video4.SetActive(true);
                        break;
                    case 9:
                        text_tuto.GetComponent<Text>().text = "Once again move and rotate the piece";
                        video4.GetComponent<AudioSource>().Play();
                        break;
                }
                this.gameObject.transform.position = start_position;
                this.gameObject.transform.rotation = start_rotation;
                this.gameObject.GetComponent<parent_property>().is_snapped = false;
                this.gameObject.transform.Rotate(90 * number_sucess, 0, 0);
                GameObject.Find("aim_piece_tuto").gameObject.transform.rotation = start_rotation;
                GameObject.Find("aim_piece_tuto").gameObject.transform.Rotate(90 * number_sucess, 0, 0);
                if (number_sucess==8 || number_sucess == 9)
                {
                    GameObject.Find("aim_piece_tuto").gameObject.transform.Rotate(90 * number_sucess, 45, 0);
                }
            }
        }
        else
        {
            GameObject.Find("Tuto_part1").SetActive(false);
            video4.SetActive(false);
            tuto_part2.SetActive(true);
        }


        
    }
}
