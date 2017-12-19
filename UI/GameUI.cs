using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image GameFader;     //Black image across the whole screen that fades the game to black, showing UI over that black screen (we access it in the GameController when the initial countdown ends),
    [SerializeField] private Image UIFader; //Same black image, but used to fade actual UI, when GameFader has faded the game (another approach could be fading individual panels on UI, for now using fader)

    public GameObject CountdownPanel;   //The panel that shows when starting the game (gets destroyed in the end of countdown, since we don't need it anymore for the play time)
    public Text Countdown;              //Countdown text to change it every second in GameController until the game starts

    [SerializeField] private GameObject periodPanel;    //Panel with all Periods (to enable it)
    [SerializeField] private GameObject periodCirclePrefab; //The prefab to instantiate
    [SerializeField] private RectTransform periodContainer; //Container for period circles in PeriodsUI to instantiate period circles there when initializing the scene

    [SerializeField] private GameObject endPanel;    //End game panel with "Replay Game", "Main Menu" options
    [SerializeField] private GameObject replayButton;//Button to select when came to this menu

    [SerializeField] private Text PlayerOneName;
    [SerializeField] private Text PlayerTwoName;    //Player names in Periods UI
    [SerializeField] private Text PlayerOneScore;   
    [SerializeField] private Text PlayerTwoScore;   //Player scores in Periods UI
    
    [SerializeField] private RectTransform pauseMenu;   //Pause menu panel. RectTransform, because we change the position of the menu, depending on which player paused the game
    
    private GameObject[] periodCircles;     //Red period circles reference array to enable them each period

    //TODO Starting Sequence (TUGUSH)
    
    void Start()
    {
        number = GameController.Controller.NumberOfPeriods; //Shorter reference

        if (number == 0) return;     //We don't have period UI if the game is score-based (periods = 0) (if there is one period, there is still a chance for overtime)

        PlayerOneName.text = GameController.Controller.PlayerOne.PlayerName;    //Set player names in Period UI
        PlayerTwoName.text = GameController.Controller.PlayerTwo.PlayerName;
               
        periodCircles = new GameObject[number]; //Initialize array of red period circles

        for (int j = 0; j < number; j++)    //For all periods
        {
            float x = periodContainer.rect.x;    //Get the left position of period container rect
            float w = periodContainer.rect.width;//Get the width of the container rect
            float pos = x + w * (j + 1) / (number + 1);     //Transform those into circle positions on UI         
            GameObject circle = Instantiate(periodCirclePrefab, periodContainer);   //Instantiate period circles, parented to the container
            circle.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos, 0);    //Set theirs position in the container
            periodCircles[j] = circle.transform.GetChild(0).gameObject; //Fill into array the red circle, which is parented to black period circle in prefab (we enable them as new periods start)
            circle.transform.GetChild(1).GetComponent<Text>().text = (j + 1).ToString();    //Set text below the circle to the corresponding period number
        }

        periodCircles[0].SetActive(true);   //Set active first red period circle
    }

    public void PauseMenu()  //Function invoked from GameController
    {        
        periodPanel.SetActive(false);   //Disable all the stuff that could be enabled for some reason
        GameFader.color = Color.clear;
        UIFader.color = Color.clear;
        endPanel.SetActive(false);
        pauseMenu.gameObject.SetActive(true);   //Enable pause menu panel
    }

    public void QuitMatch()     //Launch end-period sequence with the GameStats message
    {
        StartCoroutine(EndGameSequence("THIS GAME WAS ENDED PREMATURELY"));
    }
    
    private int period;     //Current period number (when Periods UI gets shown, this is the number of 'next' period //Shorter references from GameController
    private int number;     //Number of periods

    public void EndPeriod()     //Function that gets called from GameController when period countdown comes to 0
    {
        period = ++GameController.Controller.CurrentPeriod; //Increment a period in GameController and assign it to this variable

        gameObject.SetActive(true); //Enable GameUI object (it gets disabled in the end of period sequence of countdown sequence)

        if (period <= GameController.Controller.NumberOfPeriods)    //If we are still in playable number of periods
        {
            StartCoroutine(EndPeriodSequence());    //Start Period sequence
        }
        else
        {
            int P1Score = GameController.Controller.PlayerOne.playerStats.Score;    //Shorter references
            int P2Score = GameController.Controller.PlayerTwo.playerStats.Score;

            if (P1Score == P2Score) //If the game is tied, set the overtime
            {
                GameController.Controller.isPlayToScore = true; //Make is so we now play to score
                GameController.Controller.PeriodTime = P1Score + 1; //Of the one above player's current scores
                
                StartCoroutine(EndPeriodSequence(true));    //Start the sequence with "overtime = true"
            }
            else if (P1Score > P2Score) //Means if player 1 won
            {                           //Start the sequence with the message on GameStats that this player won
                StartCoroutine(EndGameSequence($"{GameController.Controller.PlayerOne.PlayerName} VANQUISHED {GameController.Controller.PlayerTwo.PlayerName}!"));
            }
            else if (P2Score > P1Score) //Means if player 2 won
            {                           //Do similar stuff but for player 2
                StartCoroutine(EndGameSequence($"{GameController.Controller.PlayerTwo.PlayerName} VANQUISHED {GameController.Controller.PlayerOne.PlayerName}!"));
            }

            
        }
        
    }

    private float time = 1.5f;  //Fade time of everything in period UI and also in end game UI

    IEnumerator EndPeriodSequence(bool overtime = false)    //If parameter is "true", means we are setting the next period to be overtime
    {
        GameController.Controller.Pause();      //Pause the game
       
        GameController.audioManager.PeriodHorn(); //Play period horn
        UIFader.color = Color.clear;            //Make sure UIFader is transparent (so we don't get black screen)
        GameFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true);   //Fade out the game (SetUpdate makes tween use unscaled time, since the game is paused with Time.timeScale = 0)

        int P1Score = GameController.Controller.PlayerOne.playerStats.Score;    //Shorter references
        int P2Score = GameController.Controller.PlayerTwo.playerStats.Score;

        PlayerOneScore.text = P1Score.ToString();   //Set player scores on Periods UI
        PlayerTwoScore.text = P2Score.ToString();       

        yield return new WaitForSecondsRealtime(time);      //We have timeScale set to 0, so need to use unscaled time to wait

        UIFader.color = Color.black;    //After fading out the game, set UI fader to black, so by further fading it out, it fades in Periods UI
        periodPanel.SetActive(true);    //Enable Periods UI
        UIFader.DOFade(0, time).SetUpdate(UpdateType.Normal, true); //Fade out UIFader so Periods UI fades in

        yield return new WaitForSecondsRealtime(time);      //Wait until it fades in

        GameController.Controller.SetEverythingBack(overtime);      //Run a big function to reset the whole scene to initial state (and set the overtime if it is the case)

        if (overtime == false)  //If there is no overtime in next period
        {
            if (P1Score == P2Score) //But if player scores are tied, play the sound "Tie game, [number of periods]", specific to current period number
            {                  
                GameController.announcer.PeriodSoundTied(period - number + 3);      //The function parameter is how you get corresponding audioClip from the array depending on current period
            }
        }
        else    //If there is overtime
        {            
            GameController.announcer.OvertimeSound();
        }
        
        while (InputManager.GetButtonDown("Start", PlayerID.One) == false && InputManager.GetButtonDown("Start", PlayerID.Two) == false)    //Until some user presses 'Start', wait
        {                                               
            yield return null;
        }

        if (overtime == false)  //Only if there is no overtime
        {
            if (P1Score != P2Score) //If players are not tied, say "Next period" after player presses the button
            {
                if (number == 4 && period == 4)                         //My numbering system of End Period lines worked perfectly except for this case, so hack it here                    
                    GameController.announcer.PeriodSound(period - 1);
                else
                    GameController.announcer.PeriodSound(period - 2);                    
            }

            for (int i = 0; i < period; i++)    //After pressing the button activate next red circle
            {
                periodCircles[i].SetActive(true);
            }
        }
    
        UIFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true); //Fade in UIFader so Periods UI fades out

        yield return new WaitForSecondsRealtime(time);  //Wait until it fades

        System.GC.Collect();    //Collect all garbage before the game starts

        GameFader.color = Color.clear;  //Instantly make GameFader transparent
        periodPanel.SetActive(false);   //Disable periods panel
        gameObject.SetActive(false);    //And disable the whole UI
       
        GameController.Controller.UnPause();  //Unpause the game and players proceed to play
    }

    [SerializeField] private GameObject gameStatsPrefab;    //Prefab GameStats panel that gets intstantiated when the game ends, filling all its fields
    private GameObject statsPanel;                          //Object where instantiate the prefab into

    IEnumerator EndGameSequence(string cause)   //Parameter means what caused the game to end (some player winning, or exiting from menu)
    {
        if (GameController.audioManager.music.isPlaying == false) GameController.audioManager.music.UnPause();  //If we are ending game from the pause, unpause the music that gets paused in pause menu

        //We use 'custom' pausing the game here, without pausing the music
        Time.timeScale = 0;     //Set timescale to 0       

        GameController.audioManager.FinalHorn(); //Play end game horn
        UIFader.color = Color.clear;            //Make sure UIFader is transparent (so we don't get black screen)
        GameFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true);   //Fade out the game (SetUpdate makes tween use unscaled time, since the game is paused with Time.timeScale = 0)
        
        yield return new WaitForSecondsRealtime(time);      //We have timeScale set to 0, so need to use unscaled time to wait

        UIFader.color = Color.black;        //Set UIFader to black to fade in GameStats UI

        statsPanel = Instantiate(gameStatsPrefab, transform);   //Instantiate GameStats panel into this GameUI object
        statsPanel.transform.SetSiblingIndex(transform.childCount - 3);                //Set so it's not in front of UIFader (object spawn in the end of hierarchy by default)
        statsPanel.GetComponent<GameStats>().gameResultString = cause;  //Sets the string in GameStats to output it to GameStats field

        GameController.Controller.SetEverythingBack(replay : true); //Set everything back here during big delay in case user will decide to replay the game
        pauseMenu.gameObject.SetActive(false);   //Disable pause menu panel if it is enabled

        yield return new WaitForSecondsRealtime(time * 2);  //Wait for some more time
      
        UIFader.DOFade(0, time).SetUpdate(UpdateType.Normal, true); //Fade out UIFader so GameStats UI fades in

        yield return new WaitForSecondsRealtime(time);      //Wait until it fades

        while (InputManager.GetButtonDown("Start", PlayerID.One) == false && InputManager.GetButtonDown("Start", PlayerID.Two) == false && Input.GetMouseButtonDown(0) == false)    //Until some user presses 'Start', wait
        {
            yield return null;
        }

        UIFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true); //Fade in UIFader so GameStats UI fades out

        yield return new WaitForSecondsRealtime(time);  //Wait until it fades

        Destroy(statsPanel);    //Destroy stats panel, if we replay the game, we will make a new one
        
        endPanel.SetActive(true);   //Enable end-panel

        UIFader.DOFade(0, time).SetUpdate(UpdateType.Normal, true); //Fade it in

        yield return new WaitForSecondsRealtime(time);  //Wait until it fades

        pauseMenu.GetComponent<PauseMenu>().eventSystem.enabled = true; //Enable EventSystem when the menu fully fades so players can control it
        CustomInputModule.Instance.PlayerOne = true;
        CustomInputModule.Instance.PlayerTwo = true;  //Set so both players can control this menu
        Cursor.lockState = CursorLockMode.None;       //COMM
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(replayButton);                    //Select Replay button to enable button navigation
        
        //Everything further gets handled with pressing buttons in the menu
    }
    
    public void MainMenu()  //Function tied to "Return to main menu" button on the end-panel
    {
        StartCoroutine(Menu());
    }

    private IEnumerator Menu()
    {
        GameController.audioManager.music.Stop();           //Stop music from playing

        CustomInputModule.Instance.PlaySelect(); //Play 'select' sound
        EventSystem.current.enabled = false;        //Disable input

        yield return new WaitForSecondsRealtime(0.5f);  //Delay 0.5 sec

        SceneManager.LoadScene("MainMenu"); //Load Main Menu scene
    }

    public void ReplayGame()    //Function tied to "Replay game" button on the end-panel
    {
        StartCoroutine(ResetOnReplayGame());
    }

    private IEnumerator ResetOnReplayGame()
    {
        GameController.audioManager.music.Stop();           //Stop music from playing

        CustomInputModule.Instance.PlaySelect();    //Play 'select' sound
        EventSystem.current.enabled = false;        //Disable input
        Cursor.lockState = CursorLockMode.Locked;       //COMM
        Cursor.visible = false;

        GameController.Controller.ReplayGame(); //Run a function on a GameController to get everything in the scene back to very initial state

        yield return new WaitForSecondsRealtime(0.5f);  //Delay 0.5 sec
        
        if (GameController.Controller.isPlayToScore == false)   //If the game wasn't score-based
        {
            for (int i = 1; i < number; i++)    //Disable all period cireles on Periods UI, except first one
            {
                periodCircles[i].SetActive(false);
            }
        }

        GameFader.color = Color.clear;  //Instantly make GameFader transparent
        endPanel.SetActive(false);   //Disable end panel
        gameObject.SetActive(false);    //And disable the whole UI
        
        System.GC.Collect();    //Collect all garbage before the game starts
        GameController.Controller.UnPause();  //Unpause the game and players proceed to play

    }



}
