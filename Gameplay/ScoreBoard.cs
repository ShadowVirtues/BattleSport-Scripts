using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




//Period positions: 






public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private Text[] scorePlayerOne;
    [SerializeField] private Text[] scorePlayerTwo; //There are multiple "fields" on th scoreboard holding the same value, that's why reference them in arrays
    [SerializeField] private Text[] PeriodTime;
    [SerializeField] private Text[] PlayerOneLabel;
    [SerializeField] private Text[] PlayerTwoLabel;
    [SerializeField] private RectTransform[] PeriodContainer;
    [SerializeField] private Text[] PlayToScore;
    [SerializeField] private GameObject PeriodCirclePrefab;

    private GameObject[,] periodCircles;


    void Awake()
    {
        SetPlayerNames();   //COMM
        SetPeriods();
        UpdateScore();      //When the scene starts, set scoreboard values to starting ones
        UpdateTime();
    }

    public void SetPlayerNames()
    {
        for (int i = 0; i < 2; i++)
        {
            PlayerOneLabel[i].text = GameController.Controller.PlayerOne.PlayerName;
            PlayerTwoLabel[i].text = GameController.Controller.PlayerTwo.PlayerName;
        }


    }

    public void SetPeriods()
    {
        int number = GameController.Controller.NumberOfPeriods;

        if (number == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                PlayToScore[i].gameObject.SetActive(true);
                PlayToScore[i].text = $"PLAY TO {GameController.Controller.PeriodTime / 60}";
                PeriodContainer[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < 4; i++)
            {
                PeriodTime[i].gameObject.SetActive(false);
            }
        }
        else
        {
            periodCircles = new GameObject[2, number];

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < number; j++)
                {
                    float x = PeriodContainer[i].rect.x;
                    float w = PeriodContainer[i].rect.width;
                    //float pos = x + w * (1 + 2 * j) / (2 * number);
                    float pos = x + w * (j + 1) / (number + 1);

                    GameObject circle = Instantiate(PeriodCirclePrefab, PeriodContainer[i]);    //TODO POSITION
                    circle.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos, 0);
                    periodCircles[i, j] = circle.transform.GetChild(0).gameObject;
                }
            }
        }
    
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

    public void NextPeriod()
    {
        int periods = GameController.Controller.CurrentPeriod;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < periods; j++)
            {
                
                periodCircles[i, j].SetActive(true);
            }
        }

    }





}
