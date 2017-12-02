using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class InstantActionSetup : MonoBehaviour
{   
    [SerializeField] private Text periodTime;
 
    private int previousPeriodTime = 0;         
    private int previousPlayToScore = 0;                                                                                                       

    private const string playToScoreString = "PLAY TO\r\nSCORE";
    private const string periodMinutesString = "PERIOD\r\n(MINUTES)";

    private const string InstAction_Arena = "InstAction_Arena";
    private const string InstAction_PlayerOne = "InstAction_PlayerOne";
    private const string InstAction_PlayerTwo = "InstAction_PlayerTwo";
    private const string InstAction_NumberPeriods = "InstAction_NumberPeriods";
    private const string InstAction_ShotClock = "InstAction_ShotClock";
    private const string InstAction_PlayToScore = "InstAction_PlayToScore";
    private const string InstAction_PeriodTime = "InstAction_PeriodTime";

    void Start()
    {        
        LoadPreviousSettings();

        if (isPlayToScore)
        {           
            periodTime.text = playToScoreString;           
        }
        else
        {            
            periodTime.text = periodMinutesString;          
        }

    }

    private bool isPlayToScore = false;

    public void ScoreBased()
    {
        if (numberOfPeriodsSelector.GetIndex == 0)
        {
            previousPeriodTime = periodTimeSelector.GetIndex;
            periodTime.text = playToScoreString;            
            periodTimeSelector.SetIndex(previousPlayToScore);
            isPlayToScore = true;
        }
        else if (isPlayToScore)
        {

            previousPlayToScore = periodTimeSelector.GetIndex; 
            periodTime.text = periodMinutesString;            
            periodTimeSelector.SetIndex(previousPeriodTime);
            isPlayToScore = false;
        }
        print(isPlayToScore);
    }



    [SerializeField] private ArenaSelector arenaSelector;
    [SerializeField] private TankSelector  playerOneSelector;
    [SerializeField] private TankSelector  playerTwoSelector;
    [SerializeField] private ValueSelector numberOfPeriodsSelector;
    [SerializeField] private ValueSelector periodTimeSelector;
    [SerializeField] private ValueSelector shotClockSelector;
    
    public void GAMEButtonPress()
    {
        StartupController startup = GameObject.Find(nameof(StartupController)).GetComponent<StartupController>();

        startup.arena = arenaSelector.Option;
        startup.PlayerOneTank = playerOneSelector.Option;
        startup.PlayerTwoTank = playerTwoSelector.Option;
        startup.NumberOfPeriods = numberOfPeriodsSelector.Option;
        startup.PeriodTime = periodTimeSelector.Option;
        startup.ShotClock = shotClockSelector.Option;

        SavePreviousSettings();

        startup.GAMEButtonPress();
        
    }

    void LoadPreviousSettings()
    {
        arenaSelector.SetIndex(PlayerPrefs.GetInt(InstAction_Arena, 1));
        playerOneSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayerOne, 0));
        playerTwoSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayerTwo, 0));
        numberOfPeriodsSelector.SetIndex(PlayerPrefs.GetInt(InstAction_NumberPeriods, 0));        
        shotClockSelector.SetIndex(PlayerPrefs.GetInt(InstAction_ShotClock, 0));
        if (numberOfPeriodsSelector.GetIndex == 0)
        {
            isPlayToScore = true;
            periodTimeSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayToScore, 0));            
            previousPeriodTime = PlayerPrefs.GetInt(InstAction_PeriodTime, 0);
        }
        else
        {
            isPlayToScore = false;
            periodTimeSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PeriodTime, 0));           
            previousPlayToScore = PlayerPrefs.GetInt(InstAction_PlayToScore, 0);
        }

        
    }

    void SavePreviousSettings()
    {
        PlayerPrefs.SetInt(InstAction_Arena, arenaSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_PlayerOne, playerOneSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_PlayerTwo, playerTwoSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_NumberPeriods, numberOfPeriodsSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_ShotClock, shotClockSelector.GetIndex);
        if (isPlayToScore)
        {
            PlayerPrefs.SetInt(InstAction_PlayToScore, periodTimeSelector.GetIndex);
            PlayerPrefs.SetInt(InstAction_PeriodTime, previousPeriodTime);
        }
        else
        {
            PlayerPrefs.SetInt(InstAction_PlayToScore, previousPlayToScore);
            PlayerPrefs.SetInt(InstAction_PeriodTime, periodTimeSelector.GetIndex);
        }

    }


    public void BACKButtonPress()
    {
        SavePreviousSettings();

        SceneManager.LoadScene("MainMenu");




    }








}
