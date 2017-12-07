using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Controller;    //Static reference to GameController to access it from any script

    public static Announcer announcer;          //Static reference to announcer to access it from any script (all in-game announcer sounds are implemented in it, and lots of various stuff need to play announcer sound)

    [Header("Setting In the Arena")]    //Next fields are getting set for every specific arena
    public Goal goal;                   //Reference to arena goal
    public ScoreBoard scoreBoard;       //Reference to scoreBoard

    public Transform PlayerOneSpawn;
    public Transform PlayerTwoSpawn;    //Reference to spawn points of player and ball (this is where StartupController spawns everything)
    public Transform BallSpawn;

    [Header("Shouldn't be set in the Arena")]   //TODO [HideInInspector]. Right now they are visible for testing purposes
    public Player PlayerOne;
    public Player PlayerTwo;
    public Ball ball;

    public int ShotClock;
    public int PeriodTime;
    public int NumberOfPeriods;
    public bool isPlayToScore;
    public float ArenaDimension;    //This is the size of the arena in one dimension (X or Y, cuz they are equal, arena always has square shape) 
    public AudioClip Music;   

    [SerializeField] private GameObject audioManagerPrefab; //DELETE. This is to be able to load INJECTED scenes, otherwise audioManager gets instantiates from StartupController
    public GameObject audioManagerObject;           //This is where the audioManager gets instantiated into (to have reference to it to get reference to Announcer and actual AudioManager component)
    public GameUI gameUI;                       //GameUI object that gets instantiated in StartupController

    public static AudioManager audioManager;        //Static reference for AudioManager component to produce sound from various sources in scripts

    void Awake()            //This runs very first when arena scene is loaded
    {
        Controller = this;  //Set static reference of GameController to 'this'

        if (StartupController.Controller != null)   //DELETE. For testing in INJECTED arenas when StartupController doesn't exist
        {
            StartupController.Controller.LoadItemsToArena(this);    //'Hack' solution to get StartupController to load all the stuff into arena AFTER one frame of LoadScene, but BEFORE all Awakes on all objects in arena start executing
        }
        else    //DELETE. Setting game parameters when StarupController doesn't exist
        {
            audioManagerObject = Instantiate(audioManagerPrefab);
            NumberOfPeriods = 3;
            isPlayToScore = false;
            ShotClock = 10;
            PeriodTime = 180;
            ArenaDimension = 100;      
            PlayerOne.PlayerName = "PLAYER 1";
            PlayerTwo.PlayerName = "PLAYER 2";
        }

        announcer = audioManagerObject.GetComponent<Announcer>();           //Get Announcer component from instantiated audioManager object
        audioManager = audioManagerObject.GetComponent<AudioManager>();     //Get AudioManager component from instantiated audioManager object
        
    }

    void Start()    //Stuff we need to do when all Awakes has executed
    {
        if (ball.gameObject.name == "ElectricBall") //KLUUUUUUUUUUUUUUDGEEEEEEEEEEEEEEEEEEEEE (DELETE this shit tho, probably should spawn ballCamera and set the color depending on a ball)
            GameObject.Find("BallCamera").GetComponent<Camera>().backgroundColor = new Color(49f / 256, 77f / 256, 121f / 256, 0);
        
        if (isPlayToScore == false)   //After ScoreBoard's Awake has executed initializing it, fill all its stuff
        {
            GameTime = PeriodTime;
            CurrentPeriod = 1;
            scoreBoard.NextPeriod();
            StartCoroutine(nameof(PeriodCountdown));
        }

        Pause();    //When everything in the arena finishes loading, game gets paused while start-game-countdown screen still runs
        
        StartCoroutine(nameof(GameTimeCounter));    //Launch corouting to count overall game time for the stats (we could get it from period lengths, but there are no periods in case of score-based game)
        StartCoroutine(Countdown());    //TODO Start the countdown before the game starts
    }
   
    public int GameTime;        //Variable indicating current time left until end of the period, counts down in Coroutine
    public int TotalGameTime;   //Indicates total game time for end-game stats
    public int CurrentPeriod;   //Number of current period

    private readonly WaitForSeconds second = new WaitForSeconds(1); //One second of period time

    public bool PeriodEnding;   //Bool to get player scripts to know its <20 seconds of period time, so UI shows the countdown

    IEnumerator GameTimeCounter()   //Corouting counting total game time
    {
        while (true)        //Always execute it, until we stop it in the very end of the game
        {
            yield return second;    //Wait a second
            TotalGameTime++;        //Add a second to counter
        }        
    }

    IEnumerator PeriodCountdown()   //Gets started on the scene load or in the start of every period
    {
        scoreBoard.UpdateTime();    //Set initial timer on scoreboard (so it's correct in the first second of the game)
        while (GameTime > 0)        //While we didn't reach 0 seconds in the period timer
        {           
            yield return second;    //Wait a second
            GameTime--;             //Decrease game time by one second

            if (GameTime <= 20 && PeriodEnding == false)    //When period has 20 seconds left, show the scoreboard view for players to see the countdown, launch functions on players only once when below 20 seconds
            {
                PeriodEnding = true;    //Set the flags player script uses to correlate between shotclock
                PlayerOne.PeriodEnding();   //Launch function on players to show the UI
                PlayerTwo.PeriodEnding();
            }
                
            scoreBoard.UpdateTime(); //Update time on scoreBoard           
        }      
        gameUI.EndPeriod();     //When the whole period time goes to 0, launch the sequence of showing Periods UI and preparing for the next period

    }

    IEnumerator Countdown() //Initial countdown when starting the game
    {
        gameUI.GameFader.color = Color.black;   //Setting gameFader to have black color just in case
        gameUI.CountdownPanel.SetActive(true);  //Enabling countdown panel if it was disabled
        int count = 3;  //How many seconds to count
         
        for (int i = count; i >= 0; i--)
        {           
            gameUI.Countdown.text = $"GAME STARTING IN {i}";            
            yield return new WaitForSecondsRealtime(1);         //Since the game is paused at that point, use unscaled time
        }
        gameUI.GameFader.color = Color.clear;        //In the end of countdown, instantly make GameFader transparent (show the game)
        Destroy(gameUI.CountdownPanel);             //We don't need countdown panel anymore, so destroy it
        gameUI.gameObject.SetActive(false);         //Disable game UI
        GC.Collect();   //Collect all garbage before starting the game
        Pause();        //Unpause the game and players proceed to play
    }
    
    public bool paused = false;    //Flag to know if the game is paused (for other scripts to know) 

    public void Pause()             //Function that pauses and unpauses the game
    {
        if (paused == false)        //If the game is not paused
        {
            Time.timeScale = 0;     //Set timescale to 0
            paused = true;          //Set the flag
            audioManager.music.Pause(); //Stop the music (when setting timeScale to 0, all sounds will actually still continue playing, which is what we want, except for music)
        }
        else
        {
            Time.timeScale = 1;     //Set those things back if game is paused
            paused = false;
            audioManager.music.UnPause();
        }

    }

    //void LateUpdate()
    //{
    //    if (Input.GetKeyDown(KeyCode.Joystick1Button2))
    //    {
    //        StopCoroutine(nameof(PeriodCountdown));
    //        gameUI.EndPeriod();

    //    }

    //}


    public void SetEverythingBack(bool overtime = false) //Function that is implemented in all scripts that needs resetting when new period starts. If parameter is "true", means we are setting it for overtime
    {
        PlayerOne.transform.position = PlayerOneSpawn.position;
        PlayerOne.transform.rotation = PlayerOneSpawn.rotation;
        PlayerTwo.transform.position = PlayerTwoSpawn.position; //Reset players/ball position and rotation to where they got spawned initially
        PlayerTwo.transform.rotation = PlayerTwoSpawn.rotation;
        ball.transform.position = BallSpawn.position;
        ball.transform.rotation = BallSpawn.rotation;

        PlayerOne.SetEverythingBack();
        PlayerTwo.SetEverythingBack();  //Launch specific functions like this on all of them and goal
        ball.SetEverythingBack();
        goal.SetEverythingBack();

        PlayerOne.GetComponent<PlayerShooting>().SetEverythingBack();   //Launch it on PlayerShooting as well
        PlayerTwo.GetComponent<PlayerShooting>().SetEverythingBack();

        PeriodEnding = false;         //Set so period isn't ending

        if (overtime == false)  //If it's no overtime
        {            
            GameTime = PeriodTime;        //Set period time back
            scoreBoard.NextPeriod();      //Set period circles on scoreboard
            StartCoroutine(PeriodCountdown());  //Start period countdown
        }
        else    //If it is overtime
        {
            scoreBoard.SetPeriods();    //Launch a function on scoreboard to shut down period timers and set caption "Play to XX" (because before that we switch "isPlayToScore=true" in GameUI)
            PlayerOne.ShowMessage(Message.Overtime);
            PlayerOne.ShowMessage(Message.ScoreToWin);  //Show messages for players when the game starts
            PlayerTwo.ShowMessage(Message.Overtime);
            PlayerTwo.ShowMessage(Message.ScoreToWin);
        }

        audioManager.music.Stop();      //Stop the music
        audioManager.music.Play();      //Play and pause it for starting playing in the moment of period starting
        audioManager.music.Pause();

        //SET EVERYTHING BACK
        //!!Set Players to their positions
        //!!Zero players velocity
        //!!Clear messages
        //!!Set their health back
        //!!Set Ball to its position
        //!!Zero its velocity, angularVelocity, reset possession stuff (check possession UI)
        //!!Disable all rockets and lasers
        //TODO Check what happens with powerups, if they remain in the same place or start spawning from none. Also check if effects persist on players
        //!!Set goal to original position
        //Set period time back, launch it again
        //Set scoreboard time back, set periods on it
        //!!Check what happens to PossessionTime of player if he possesses when period ends, if it doesnt increase because of timeScale = 0
        //!!Check what happens if you are dead during period end (consider camera animation)
        //!!Check what happens if just-scored-ui is going, if ball is transparent
        //!!Check what happens if either tank is flashing, or goal is flashing
        //!!Check what happens if explosion of rocker/laser is going on

        //Play the game normally and see what else can be going during period end


        //And finally unpause the game



    }


    public void ReplayGame()    //Function that resets all the stuff when pressed "Replay Game" after finishing the game //TODO
    {
        StopCoroutine(nameof(GameTimeCounter));
        TotalGameTime = 0;
        StartCoroutine(nameof(GameTimeCounter));


    }

    //void Awake()
    //{
    //    if (Controller == null)
    //    {
    //        DontDestroyOnLoad(gameObject);    //SINGLETON SHIT
    //        Controller = this;
    //    }
    //    else if (Controller != this)
    //    {
    //        Destroy(gameObject);
    //    }
    //}
}
