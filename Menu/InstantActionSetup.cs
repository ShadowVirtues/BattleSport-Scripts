using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InstantActionSetup : MonoBehaviour
{
    [SerializeField] private Text numberOfPeriods;
    [SerializeField] private Text periodTime;
    [SerializeField] private Text periodTimeValue;
    

    private int previousPeriodTime = 3;         //TODO Consider that "prevoiusPeriodTime" from the start gets set to "1", which is what gets set by default in Awake of ValueSelector
    private int previousPlayToScore = 5;         //TODO get those from 'save' container 

    private const string playToScoreString = "PLAY TO\r\nSCORE";
    private const string periodMinutesString = "PERIOD\r\n(MINUTES)";

    void Start()
    {
        ScoreBased();
    }


    public void ScoreBased()
    {
        if (Int32.Parse(numberOfPeriods.text) == 0)
        {
            previousPeriodTime = Int32.Parse(periodTimeValue.text);
            periodTime.text = playToScoreString;
            //periodTimeValue.text = previousPlayToScore.ToString();
            periodTimeSelector.ChangeIndex(previousPlayToScore);
        }
        else if (periodTime.text == playToScoreString)
        {

            previousPlayToScore = Int32.Parse(periodTimeValue.text);
            periodTime.text = periodMinutesString;
            //periodTimeValue.text = previousPeriodTime.ToString();
            periodTimeSelector.ChangeIndex(previousPeriodTime);
        }

    }



    [SerializeField] private ArenaSelector arenaSelector;
    [SerializeField] private TankSelector playerOneSelector;
    [SerializeField] private TankSelector playerTwoSelector;
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
        
        startup.GAMEButtonPress();
        
    }










}
