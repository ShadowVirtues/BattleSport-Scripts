using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private Text[] scorePlayerOne;
    [SerializeField] private Text[] scorePlayerTwo; //There are multiple "fields" on the scoreboard holding the same value, that's why reference them in arrays
    [SerializeField] private Text[] PeriodTime;
    [SerializeField] private Text[] PlayerOneLabel;
    [SerializeField] private Text[] PlayerTwoLabel;
    [SerializeField] private RectTransform[] PeriodContainer;   //This is the area on the scoreboard where period circles are shown
    [SerializeField] private Text[] PlayToScore;                //Label in place of PeriodContainers showing "Play To ##" when this mode is set
    [SerializeField] private GameObject PeriodCirclePrefab;     //Circle prefab to instantiate in PeriodContainer when setting up the scene (Prefab is black circle, and childed to it red circle)

    private GameObject[,] periodCircles;        //Two-dim array of red period circles to enable them when periods start (two-dim, cuz there is a set of those on each of two sides of scoreboard, and also multiple period circles in both sets)

    void Awake()        //During the scene loading we "initialize" scoreboard before GameController initializes all scoreboard-related fields in its "Start" 
    {                   //(because GameController is first in Script Execution Order, but we need it to update scoreboard stuff AFTER scoreboard gets initialized)
        SetPlayerNames();   //When the scene starts, set player names   
        SetPeriods();       //Set all periods stuff
        UpdateScore();      //Set score to 0-0
    }

    public void SetPlayerNames()
    {
        for (int i = 0; i < 2; i++) //For both sides of scoreboard
        {
            PlayerOneLabel[i].text = GameController.Controller.PlayerOne.PlayerName;
            PlayerTwoLabel[i].text = GameController.Controller.PlayerTwo.PlayerName;
        }
    }

    public void SetPeriods()    //Manage periods on scoreboard
    {    
        if (GameController.Controller.isPlayToScore)    //If the game mode is "Play To Score" (when number of periods is 0)
        {
            for (int i = 0; i < 2; i++)
            {
                PlayToScore[i].gameObject.SetActive(true);  //Enable "Play To ##" label
                PlayToScore[i].text = $"PLAY TO {GameController.Controller.PeriodTime}";   //Set its text (the variable with score to reach is held in PeriodTime)
                PeriodContainer[i].gameObject.SetActive(false);                             //Disable period circles, cuz we don't need them in this mode
            }
            for (int i = 0; i < 4; i++)     //There are 4 period timers on scoreboard
            {
                PeriodTime[i].gameObject.SetActive(false);  //Also disable all period timers, since the game is not time-based
            }
        }
        else        //If its actually time-based game
        {
            int number = GameController.Controller.NumberOfPeriods; //Shorter reference

            periodCircles = new GameObject[2, number];  //Initialize the array of period circles (2 - two sides of scoreboard, number - number of periods to play, means number of circles on each side of scoreboard)

            for (int i = 0; i < 2; i++) //For both sides of scoreboard
            {
                for (int j = 0; j < number; j++)    //For number of periods, means period circles on scoreboard
                {
                    float x = PeriodContainer[i].rect.x;    //Get the left position of container rect
                    float w = PeriodContainer[i].rect.width;//Get the width of the container rect
                    //float pos = x + w * (1 + 2 * j) / (2 * number);
                    float pos = x + w * (j + 1) / (number + 1);     //Transform those into circle positions on scoreboard

                    GameObject circle = Instantiate(PeriodCirclePrefab, PeriodContainer[i]);    //Instantiate period circles, parented to their container
                    circle.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos, 0);//Set theis position in the container
                    periodCircles[i, j] = circle.transform.GetChild(0).gameObject;              //Fill into array the red circle, which is parented to black period circle in prefab (we enable them as new periods start)
                }
            }
        }
    
    }

    public void UpdateScore()   //Public function that runs when someone scores
    {
        for (int i = 0; i < 2; i++)
        {
            scorePlayerOne[i].text = GameController.Controller.PlayerOne.playerStats.Score.ToString();  //Get scores from playerStats
            scorePlayerTwo[i].text = GameController.Controller.PlayerTwo.playerStats.Score.ToString();
        }

    }

    public void UpdateTime()    //Public function that runs every second of the game form GameController (if it's time-based game)
    {
        int t = GameController.Controller.GameTime; //Shorter variable

        if (t < 60 * 10)    //The scoreboard only has space for "#:##" number, so max number to show on it can only be "9:59"
        {
            for (int i = 0; i < 4; i++) //4 fields with period time on ScoreBoard
            {
                PeriodTime[i].text = $"{t / 60}:{t % 60:D2}";   //Convert GameTime from seconds to Minutes:Seconds
            }
        }
        else    //If the game time is more than 9:59 (if user for whatever reason puts this time)
        {
            for (int i = 0; i < 4; i++) 
            {
                PeriodTime[i].text = "9:59";   //Set so the PeriodTime stays at 9:59, until the time goes less than that
            }
        }
        

    }

    public void NextPeriod()    //Function that enables the next period circle on each next period in the game
    {
        int periods = GameController.Controller.CurrentPeriod;  //Shorter reference

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < periods; j++)
            {               
                periodCircles[i, j].SetActive(true);    //Enable all circles within current amount of periods
            }
        }

    }





}
