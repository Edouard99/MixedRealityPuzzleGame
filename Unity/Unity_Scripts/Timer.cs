using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Microsoft.MixedReality.Toolkit.UI;

public class Timer : MonoBehaviour
{
    //Handle time during a session
    [HideInInspector] public bool uploadasked=false;
    [HideInInspector] public float timeassessment=0f;

    public float timeRemaining = 30;
    public bool timerIsRunning = false;
    public Text TimeText;
    public GameObject MainMenu;
    public GameObject Level;
    public float timeBreak = 0.0f;
    public bool hasAssess = false;
    public Text BannerText;

    private void DisplayTime(float timeToDisplay)
    {
        //Display the time during the session on the Holographic display
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        TimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void DisplayBreak(float timeToDisplay)
    {
        //Display the time during break time on the Holographic display
        timeToDisplay += 1;
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        BannerText.text = string.Format("Break Time ! Time remaining {0:00}:{1:00}", minutes, seconds) + ". Please Complete the assessment during the break time";
    }
    private void Start()
    {
        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            //if the player is playing a puzzle
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else if(timeRemaining<0 & hasAssess==false)
            {
                //Handle assessment when time is up (while the player has not assessed the session is not ended)
                BannerText.text = "Time has run out! Please Complete the assessment to end the session ...";
                GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().assessemnt_button.SetActive(true);
                GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().assessment_canvas.SetActive(true);
                foreach (GameObject piece in GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().pieces_List)
                {
                    Destroy(piece);
                }
                foreach (GameObject aimcube in GameObject.FindGameObjectsWithTag("aim_cube"))
                {
                    Destroy(aimcube);
                }
                GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().break_menu.SetActive(false);
            }
            else
            {
                //When the player assess the data are send to the azure storage and the session is finished
                BannerText.text = "Returning to Menu in 10 seconds please wait...";
                timeRemaining = 0;
                foreach (GameObject piece in GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().pieces_List)
                {
                    Destroy(piece);
                }
                foreach (GameObject aimcube in GameObject.FindGameObjectsWithTag("aim_cube"))
                {
                    Destroy(aimcube);
                }
                timerIsRunning = false;
                if (uploadasked == false)
                {
                    GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().mode = GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().mode;
                    GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().sessionDateTime = GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().sessionDateTime;
                    //GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().final_upload = true;
                    GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().uploadJSON("_matrix_");
                    GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().uploadJSON("_tracking_");
                    GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().uploadJSON("_player_");
                    GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().uploadJSON("_assessment_");
                    uploadasked = true;
                    timeassessment = Time.time;
                }
                if (uploadasked == true)
                {
                    BannerText.text = "Returning to Menu in 10 seconds please wait...";
                    if (Time.time > timeassessment + 10)
                    {
                        uploadasked = false;
                        hasAssess = false;
                        MainMenu.SetActive(true);
                        Level.SetActive(false);
                    }
                }
                
                //hasAssess = false;
                //MainMenu.SetActive(true);
                //Level.SetActive(false);
            }
        }
        else
        {
            //If the player is in break time
            if (timeBreak > 0)
            {
                timeBreak -= Time.deltaTime;
                DisplayBreak(timeBreak);
            }
            else if (timeBreak<0 & hasAssess==false)
            {
                //If the player has not assessed
                BannerText.text = "Please Complete the assessment to continue ...";
            }
            else
            {
                //When the player has assessed
                timerIsRunning = true;
                GameObject.Find("Empty_Big_Cube").GetComponent<Grid_big_cube_update>().doBreak = false;
                hasAssess = false;
            }
        }
    }
}
