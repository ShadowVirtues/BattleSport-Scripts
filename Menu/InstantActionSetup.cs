using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;
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

    //===================================================
    [Header("Sound")]
    [SerializeField] private AudioSource music;     //Sources to play music and announcer sounds
    [SerializeField] private AudioSource announcer;
    
    [SerializeField] private GameObject blockInputPanel;    //"Panel" over the whole screen to block mouse input when needed

    void Start()
    {
        blockInputPanel.SetActive(false);   //Disable it in case if was active for some reason

        LoadPreviousSettings(); //Load previous set settings from PlayerPrefs

        periodTime.text = isPlayToScore ? playToScoreString : periodMinutesString;  //Set the label to respective value (isPlayToScore gets set in LoadPreviousSettings)
       
        music.PlayDelayed(0.1f);    //We have Duck Volume on music when announcer plays. Delay so for the split second of the scene music doesnt get full volume and then instantly down because of announcer ducking 
        announcer.Play();           //Play announcer that will duck the volume of music

        CustomInputModule.Instance.Enabled = true;      //Enable input from disabling it for 0.5 before switching the scene
        EventSystem.current.SetSelectedGameObject(arenaSelector.gameObject);    //Set arena selector as first selected in the start of the scene
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

    [Header("Selectors")]
    [SerializeField] private ArenaSelector arenaSelector;
    [SerializeField] private TankSelector  playerOneSelector;
    [SerializeField] private TankSelector  playerTwoSelector;           //All different selector references to get values from them when pressing GAME button, or when pressing BACK to save all the values
    [SerializeField] private ValueSelector numberOfPeriodsSelector;
    [SerializeField] private ValueSelector periodTimeSelector;
    [SerializeField] private ValueSelector shotClockSelector;
    
    public void GAMEButtonPress()   //This function is linked to the button "GAME" when pressing it
    {
        StartCoroutine(Game());        
    }

    private IEnumerator Game()
    {
        music.Stop();       //Stop the music and announcer from playing
        announcer.Stop();
        
        CustomInputModule.Instance.PlaySelect();           //Play 'select' sound
        disableInput();             //Disable input
        
        StartupController startup = FindObjectOfType<StartupController>();  //Find StartupController in our scene (In some menus it can be transfered over from previous scene, so we can't have a set reference to it in all cases)

        startup.arena = arenaSelector.Option;
        startup.PlayerOneTank = playerOneSelector.Option;
        startup.PlayerTwoTank = playerTwoSelector.Option;   //Set all its parameters from all selectors in the menu
        startup.NumberOfPeriods = numberOfPeriodsSelector.Option;
        startup.PeriodTime = periodTimeSelector.Option;
        startup.ShotClock = shotClockSelector.Option;
        startup.PlayerOneName = "PLAYER 1";
        startup.PlayerTwoName = "PLAYER 2"; //Instant Action is a quickest way to start the game, so no name entry and default names for players
        
        SavePreviousSettings();                             //Save the selectors states to PlayerPrefs

        yield return new WaitForSecondsRealtime(0.5f);      //0.5 sec delay before scene switching

        startup.GAMEButtonPress();                          //Launch the Startup Function on the side of GameController
    }

    void LoadPreviousSettings()     //Function loading previous state of the menu from PlayerPrefs
    {
        //Set index and value of every selector to what is saved to PlayerPrefs, second parameter is default value if there is no key in PlayerPrefs
        arenaSelector.SetIndex(PlayerPrefs.GetInt(InstAction_Arena, 1));            //First arena is in index 1
        playerOneSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayerOne, 0));
        playerTwoSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayerTwo, 0));        //All the others set to 0, if no key
        numberOfPeriodsSelector.SetIndex(PlayerPrefs.GetInt(InstAction_NumberPeriods, 3));        
        shotClockSelector.SetIndex(PlayerPrefs.GetInt(InstAction_ShotClock, 2));
        if (numberOfPeriodsSelector.GetIndex == 0)      //If "Number Of Periods" is set to 0, means we show "Play To Score"
        {
            isPlayToScore = true;
            periodTimeSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PlayToScore, 14));      //Get PlayToScore save and put it into periodTimeSelector (both "Period (Minutes)" and "Play To Score" are shown there)
            previousPeriodTime = PlayerPrefs.GetInt(InstAction_PeriodTime, 2);               //Set previousPeriodTime to what PeriodTime was before in case we switch to it
        }
        else
        {
            isPlayToScore = false;
            periodTimeSelector.SetIndex(PlayerPrefs.GetInt(InstAction_PeriodTime, 2));       //The opposite     
            previousPlayToScore = PlayerPrefs.GetInt(InstAction_PlayToScore, 14);
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
        StartCoroutine(Back());
    }

    private IEnumerator Back()
    {
        music.Stop();           //Stop music and announcer if they were playing
        announcer.Stop();
        
        CustomInputModule.Instance.PlaySelect();           //Play 'select' sound
        disableInput();         //Disable input
        SavePreviousSettings(); //Save the state of Instant Action Setup menu

        yield return new WaitForSecondsRealtime(0.5f);  //Delay 0.5 sec

        SceneManager.LoadScene("MainMenu"); //Load Main Menu scene
    }

    private void disableInput() //Function to disable player input after he clicked some button to go to next menu (since we have a small delay after pressing this button, when we don't want player to be able to select something else)
    {
        CustomInputModule.Instance.Enabled = false;       //MenuInputModule has variable that
        blockInputPanel.SetActive(true);                                            //Enable the panel in front of everything that blocks mouse input
    }




}
