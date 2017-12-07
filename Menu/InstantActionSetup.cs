using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class InstantActionSetup : MonoBehaviour
{   
    [SerializeField] private Text periodTime;   //Reference to selector label with "Period (Minutes)" to change it to "Play To Score" and back if switching between 0 periods
    //periodTimeSelector is used for both "Period (Minutes)" values and "Play To Score"

    private const string playToScoreString = "PLAY TO\r\nSCORE";        //What to change "periodTime" label to when switching
    private const string periodMinutesString = "PERIOD\r\n(MINUTES)";

    private int previousPeriodTime = 0;         //Variables to hold the previous value of PeriodTime after switching to PlayToScore, to be able to save them and restore if switching back
    private int previousPlayToScore = 0;                                                                                                       

    private const string InstAction_Arena = "InstAction_Arena";
    private const string InstAction_PlayerOne = "InstAction_PlayerOne";
    private const string InstAction_PlayerTwo = "InstAction_PlayerTwo";
    private const string InstAction_NumberPeriods = "InstAction_NumberPeriods";     //PlayerPrefs keys to load previous settings from and save to
    private const string InstAction_ShotClock = "InstAction_ShotClock";
    private const string InstAction_PlayToScore = "InstAction_PlayToScore";
    private const string InstAction_PeriodTime = "InstAction_PeriodTime";

    private bool isPlayToScore = false;     //Bool indicating if PlayToScore is on screen right now, instead of Period(Minutes)

    void Start()
    {
        LoadPreviousSettings(); //Load previous set settings from PlayerPrefs

        periodTime.text = isPlayToScore ? playToScoreString : periodMinutesString;  //Set the label to respective value (isPlayToScore gets set in LoadPreviousSettings)
    }
    
    public void ScoreBased()    //This function is linked to Left and Right buttons on "Number Of Periods" selector and gets called whenever user presses them (or corresponding left-right keys)
    {
        if (numberOfPeriodsSelector.GetIndex == 0)              //If set number of periods is 0
        {
            previousPeriodTime = periodTimeSelector.GetIndex;   //Save previous "Period (Minutes)" value
            periodTime.text = playToScoreString;                //Set label to "Play To Score"
            periodTimeSelector.SetIndex(previousPlayToScore);   //Set value of periodTime to what was "Play To Score" previously
            isPlayToScore = true;                               //Set the flag that it's now "Play To Score"
        }
        else if (isPlayToScore)                                 //If before switching number of periods it was "Play To Score"
        {
            previousPlayToScore = periodTimeSelector.GetIndex;  //Set stuff in the same manner back
            periodTime.text = periodMinutesString;            
            periodTimeSelector.SetIndex(previousPeriodTime);
            isPlayToScore = false;
        }       
    }

    [SerializeField] private ArenaSelector arenaSelector;
    [SerializeField] private TankSelector  playerOneSelector;
    [SerializeField] private TankSelector  playerTwoSelector;           //All different selector references to get values from them when pressing GAME button, or when pressing BACK to save all the values
    [SerializeField] private ValueSelector numberOfPeriodsSelector;
    [SerializeField] private ValueSelector periodTimeSelector;
    [SerializeField] private ValueSelector shotClockSelector;
    
    public void GAMEButtonPress()   //This function is linked to the button "GAME" when pressing it
    {
        //Find StartupController in our scene (In some menus it can be transfered over from previous scene, so we can't have a set reference to it in all cases)
        StartupController startup = GameObject.Find(nameof(StartupController)).GetComponent<StartupController>();   

        startup.arena = arenaSelector.Option;
        startup.PlayerOneTank = playerOneSelector.Option;
        startup.PlayerTwoTank = playerTwoSelector.Option;   //Set all its parameters from all selectors in the menu
        startup.NumberOfPeriods = numberOfPeriodsSelector.Option;
        startup.PeriodTime = periodTimeSelector.Option;
        startup.ShotClock = shotClockSelector.Option;
        startup.PlayerOneName = "PLAYER 1";
        startup.PlayerTwoName = "PLAYER 2"; //Instant Action is a quickest way to start the game, so no name entry and default names for players


        SavePreviousSettings();                             //Save the selectors states to PlayerPrefs

        startup.GAMEButtonPress();                          //Launch the Startup Function on the side of GameController
        
    }

    void LoadPreviousSettings()     //Function loading previous state of the menu from PlayerPrefs
    {
        //Set index and value of every selector to what is saved to PlayerPrefs, second parameter is default value if there is no key in PlayerPrefs
        arenaSelector.SetIndex(PlayerPrefs.GetInt(InstAction_Arena, 1));            //First arena is in index 1
        playerOneSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayerOne, 0));
        playerTwoSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayerTwo, 0));        //All the others set to 0, if no key
        numberOfPeriodsSelector.SetIndex(PlayerPrefs.GetInt(InstAction_NumberPeriods, 0));        
        shotClockSelector.SetIndex(PlayerPrefs.GetInt(InstAction_ShotClock, 0));
        if (numberOfPeriodsSelector.GetIndex == 0)      //If "Number Of Periods" is set to 0, means we show "Play To Score"
        {
            isPlayToScore = true;
            periodTimeSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayToScore, 0));      //Get PlayToScore save and put it into periodTimeSelector (both "Period (Minutes)" and "Play To Score" are shown there)
            previousPeriodTime = PlayerPrefs.GetInt(InstAction_PeriodTime, 0);               //Set previousPeriodTime to what PeriodTime was before in case we switch to it
        }
        else
        {
            isPlayToScore = false;
            periodTimeSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PeriodTime, 0));       //The opposite     
            previousPlayToScore = PlayerPrefs.GetInt(InstAction_PlayToScore, 0);
        }

        
    }

    void SavePreviousSettings()     //Function gets called when going back to main menu, or starting the game to save the current state of the menu
    {
        PlayerPrefs.SetInt(InstAction_Arena, arenaSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_PlayerOne, playerOneSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_PlayerTwo, playerTwoSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_NumberPeriods, numberOfPeriodsSelector.GetIndex);
        PlayerPrefs.SetInt(InstAction_ShotClock, shotClockSelector.GetIndex);
        if (isPlayToScore)
        {
            PlayerPrefs.SetInt(InstAction_PlayToScore, periodTimeSelector.GetIndex);    //Similar to loading
            PlayerPrefs.SetInt(InstAction_PeriodTime, previousPeriodTime);
        }
        else
        {
            PlayerPrefs.SetInt(InstAction_PlayToScore, previousPlayToScore);
            PlayerPrefs.SetInt(InstAction_PeriodTime, periodTimeSelector.GetIndex);
        }

    }


    public void BACKButtonPress()       //Going back to the main menu button
    {
        SavePreviousSettings(); //Save the state of Instant Action Setup menu

        SceneManager.LoadScene("MainMenu"); //Load Main Menu scene
        
    }








}
