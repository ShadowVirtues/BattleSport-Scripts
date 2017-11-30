using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TODO INJECT NUMBERS INTO TEXTURES

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private Text[] scorePlayerOne;
    [SerializeField] private Text[] scorePlayerTwo; //There are multiple "fields" on th scoreboard holding the same value, that's why reference them in arrays
    [SerializeField] private Text[] PeriodTime;
    
    void Start()
    {
        //TODO Set player names
        UpdateScore();      //When the scene starts, set scoreboard values to starting ones
        UpdateTime();
    }

    public void UpdateScore()   //Public function that runs when someone scores
    {
        for (int i = 0; i < 2; i++)
        {
            scorePlayerOne[i].text = GameController.Controller.PlayerOne.playerStats.Score.ToString();
            scorePlayerTwo[i].text = GameController.Controller.PlayerTwo.playerStats.Score.ToString();
        }

    }

    public void UpdateTime()    //Public function that runs every second of the game form GameController
    {
        int t = GameController.Controller.GameTime; //Shorter variable

        for (int i = 0; i < 4; i++) //4 fields with period time on ScoreBoard
        {
            PeriodTime[i].text = $"{t / 60}:{t % 60:D2}";   //Convert GameTime from seconds to Minutes:Seconds
        }

    }

    //TODO public void NextPeriod





}
