using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    //Handle actions of menu's button

    public GameObject MainMenu;
    public GameObject Image;
    public GameObject Level;
    public GameObject Tuto_part1;
    public GameObject empty_Big_Cube;
    Grid_big_cube_update selection;
    Timer timer;

    // Start is called before the first frame update
    void Start()
    {
        MainMenuButton();
    }

    public void TutorialButton()
    {
        MainMenu.SetActive(false);
        Tuto_part1.SetActive(true);
    }

    public void EasyButton()
    {
        MainMenu.SetActive(false);
        selection = empty_Big_Cube.GetComponent<Grid_big_cube_update>();
        selection.puzzle_lvl = "easy";
        selection.mode = "easy";
        selection.nextPuzzle = true;
        selection.timelvl = 3 * 60;
        Level.SetActive(true);
        timer = GameObject.Find("Time").GetComponent<Timer>();
        timer.timeRemaining = 60*3;     //Time for the session
        timer.timerIsRunning = true;
    }

    public void MediumButton()
    {
        MainMenu.SetActive(false);
        selection = empty_Big_Cube.GetComponent<Grid_big_cube_update>();
        selection.puzzle_lvl = "medium";
        selection.mode = "medium";
        selection.nextPuzzle = true;
        Level.SetActive(true);
        timer = GameObject.Find("Time").GetComponent<Timer>();
        timer.timeRemaining = 60*7;     //Time for the session
        timer.timerIsRunning = true;
    }

    public void HardButton()
    {
        MainMenu.SetActive(false);
        selection = empty_Big_Cube.GetComponent<Grid_big_cube_update>();
        selection.puzzle_lvl = "hard";
        selection.mode = "hard";
        selection.nextPuzzle = true;
        Level.SetActive(true);
        timer = GameObject.Find("Time").GetComponent<Timer>();
        timer.timeRemaining = 60*8;     //Time for the session
        timer.timerIsRunning = true;
    }
    public void VeryHardButton()
    {
        //This mode is not used but the idea was to use a 4x4x4 grid instead of a 3x3x3
        MainMenu.SetActive(false);
        selection = empty_Big_Cube.GetComponent<Grid_big_cube_update>();
        selection.puzzle_lvl = "veryhard";
        selection.mode = "veryhard";
        selection.nextPuzzle = true;
        Level.SetActive(true);
        timer = GameObject.Find("Time").GetComponent<Timer>();
        timer.timeRemaining = 60 * 8;     //Time for the session
        timer.timerIsRunning = true;
    }

    public void QuitButton()
    {
       Application.Quit();
    }

    public void MainMenuButton()
    {
        MainMenu.SetActive(true);
        Level.SetActive(false);
    }
}