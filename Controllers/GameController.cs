using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeamUtility.IO;
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
    public List<Powerup> Powerups;

    public int ShotClock;
    public int PeriodTime;
    public int InitialPeriodTime;   //Storing the initial period time here
    public int NumberOfPeriods;
    public bool isPlayToScore;
    public float ArenaDimension;    //This is the size of the arena in one dimension (X or Y, cuz they are equal, arena always has square shape) 
    public AudioClip Music;   

    [SerializeField] private GameObject audioManagerPrefab; //DELETE. This is to be able to load INJECTED scenes, otherwise audioManager gets instantiates from StartupController
    public GameObject audioManagerObject;           //This is where the audioManager gets instantiated into (to have reference to it to get reference to Announcer and actual AudioManager component)
    public GameUI gameUI;                       //GameUI object that gets instantiated in StartupController

    public static AudioManager audioManager;        //Static reference for AudioManager component to produce sound from various sources in scripts

    #region Starting The Arena

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
            ShotClock = 35;
            PeriodTime = 1800;
            ArenaDimension = 100;      
            PlayerOne.PlayerName = "PLAYER 1";
            PlayerTwo.PlayerName = "PLAYER 2";
            InputManager.Load();
        }

        announcer = audioManagerObject.GetComponent<Announcer>();           //Get Announcer component from instantiated audioManager object
        audioManager = audioManagerObject.GetComponent<AudioManager>();     //Get AudioManager component from instantiated audioManager object
        
    }

    void Start()    //Stuff we need to do when all Awakes has executed
    {               
        if (isPlayToScore == false)   //After ScoreBoard's Awake has executed initializing it, fill all its stuff
        {
            GameTime = PeriodTime;
            CurrentPeriod = 1;
            scoreBoard.NextPeriod();
            StartCoroutine(nameof(PeriodCountdown));
        }

        Pause();    //When everything in the arena finishes loading, game gets paused while start-game-countdown screen still runs
        
        StartCoroutine(nameof(GameTimeCounter));    //Launch corouting to count overall game time for the stats (we could get it from period lengths, but there are no periods in case of score-based game)

        PowerupSpawningSequence();          //Start spawning powerups

        if (StartupController.Controller != null)   //DELETE. For testing in INJECTED arenas when StartupController doesn't exist
        {
            gameUI.GameFader.color = Color.black;   //Setting gameFader to have black color just in case
            gameUI.CountdownPanel.SetActive(true);  //Enabling countdown panel if it was disabled            
        }
        else     //DELETE. For testing in INJECTED arenas when StartupController doesn't exist
        {
            Destroy(gameUI.CountdownPanel);
            gameUI.gameObject.SetActive(false);
            UnPause();            
        }
            
    }

    public void StartGame() //Function to start the game, gets launched from CountdownPanel
    {
        gameUI.GameFader.color = Color.clear;        //In the end of countdown, instantly make GameFader transparent (show the game)
        Destroy(gameUI.CountdownPanel);             //We don't need countdown panel anymore, so destroy it
        gameUI.gameObject.SetActive(false);         //Disable game UI

        GC.Collect();   //Collect all garbage before starting the game
        UnPause();        //Unpause the game and players proceed to play
    }
    
    #endregion

    #region Controlling the game

    public int GameTime;        //Variable indicating current time left until end of the period, counts down in Coroutine
    public int TotalGameTime;   //Indicates total game time for end-game stats
    public int CurrentPeriod;   //Number of current period

    private readonly WaitForSeconds second = new WaitForSeconds(1); //One second of period time and game time

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

    public void Pause()             //Function that pauses and unpauses the game
    {
        Time.timeScale = 0;     //Set timescale to 0        
        audioManager.music.Pause(); //Stop the music (when setting timeScale to 0, all sounds will actually still continue playing, which is what we want, except for music)
    }

    public void UnPause()
    {
        Time.timeScale = 1;     //Set those things back if game is paused        
        audioManager.music.UnPause();
    }

    public PlayerID PausedPlayer;   //Player number that paused the game (so MenuSelectors can use it to only process a single player input)
    public bool IsSplitScreenVertical = true;   //Flag to know what split screen type is set so we know where to position pause menu

    public void PauseMenu(PlayerID player)  //Function that gets called from Player.cs when pressing "Pause" button
    {
        Pause();        //Pause the game
        PausedPlayer = player;      //Set the player who paused the game
        gameUI.gameObject.SetActive(true);      //Enable UI Canvas with all non-player UI
        gameUI.PauseMenu();               //Run a function on the side of GameUI to hide all panels and shot pause menu panel
    }


    #endregion

    #region Powerups

    //GameController handles spawning powerups. 
    //The timer between powerup spawns is random between 8 and 24 seconds
    //It starts from the start of the round, then when the time comes it randomly picks powerup that isnt on the field and spawns it, then if some other powerup is due, starts the timer again.
    //Triggers to start powerup countdown: arena start, spawned powerup, powerup picked by player
    //If some of those occur, start countdown only if it isn't running already

    private void PowerupSpawningSequence()  //Function that initially starts the powerup spawning in the start of the arena
    {
        if (Powerups.Count != 0)    //If there are powerups assigned to the arena
        {
            StartPowerupCountdown();    //Start the countdown to spawn them
        }
    }

    private bool delayRunning = false;

    public void StartPowerupCountdown()
    {
        if (delayRunning == false)  //If the delay/countdown for next powerup is not already running (happens only in the start of the arena, or when all powerups are already spawned)
        {
            StartCoroutine(nameof(SpawnDelay)); //Start it
        }       
    }
    
    private List<Powerup> toPickFrom = new List<Powerup>(); //List to fill with powerups that are not spawned, to pick a random one from (that are in disabled state)

    private IEnumerator SpawnDelay()
    {
        delayRunning = true;    //In the beginning, set that we are running the coroutine already

        int randomDelay = UnityEngine.Random.Range(8, 25);  //Pick a random delay in spawning the powerup 
        yield return new WaitForSeconds(randomDelay);   //Since the delay is random, creating new WaitForSeconds every time D:

        delayRunning = false;   //After the delay, set that we are not running the delay anymore

        toPickFrom.Clear();     //Clear the list of powerups to pick from from previous spawn

        toPickFrom = Powerups.Where(x => x.gameObject.activeSelf == false).ToList();    //Fill the list with the powerups that are not spawned

        if (toPickFrom.Count == 0)  //The situation of runtime reaching this condition shouldn't occur, there must be some error if this happens
        {
            Debug.LogError("No powerups are disabled???");  //So log error
        }
        else
        {
            int random = UnityEngine.Random.Range(0, toPickFrom.Count); //Get a random powerup from disabled ones
            SpawnPowerup(toPickFrom[random]);   //Spawn it
        }
        
    }

    private void SpawnPowerup(Powerup powerup)  
    {
        powerup.transform.position = FindRandomPosition(1, 0.5f, 0);   //Find random position for powerup and put it there
        powerup.gameObject.SetActive(true); //Enable it

        if (Powerups.Any(x => x.gameObject.activeSelf == false))    //If there are disabled powerups (not all of them are spawned), run a countdown for the next powerup
        {
            StartPowerupCountdown();
        }
    }
    
    public static Vector3 FindRandomPosition(float height, float radius, float heightToSpawn, Vector3 defaultSpawn = default(Vector3))    //Algorithm for finding random position on the map to spawn whatever (dead player, teleporting goal, powerup)
    {
        //The algorithm is checking the cylinder from the ground to the [height] parameter of the function, ignoring the floor with layerMask
        //If there is nothing in this cylinder, then we can spawn our thing in this spot

        int offset = 4;             //Offset from the arena border so player doesn't spawn right next to a wall
        float levelDimension = Controller.ArenaDimension / 2;  //Get arena dimension (total X=Y length). To convert it into max coordinate, need to divide by 2

        for (int iter = 0; iter < 100; iter++)
        {
            Vector2 coord = new Vector2(UnityEngine.Random.Range(-levelDimension + offset, levelDimension - offset), UnityEngine.Random.Range(-levelDimension + offset, levelDimension - offset)); //Get random point of the map with RNG
            //Capsule starts from Y=0 (floor) to [height] parameter of this function (highest point to check)
            if (!Physics.CheckCapsule(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, height, coord.y), radius, ~(1 << 14), QueryTriggerInteraction.Ignore))   //Ignore triggers (in our case - RocketHeightTrigger)           
            {
                return new Vector3(coord.x, heightToSpawn, coord.y);    //Return position to spawn whatever object at
            }

        }

        Debug.LogError("Couldn't find the spawn site"); //If we performed 100 checks and haven't found a spawn site, there must be something wrong
        return defaultSpawn;

    }

    #endregion

    #region Resetting the arena

    public const string SetEverythingBackMessage = "SetEverythingBackMessage";

    public void SetEverythingBack(bool overtime = false, bool replay = false) //Function that is implemented in all scripts that needs resetting when new period starts. If 'overtime' is "true", means we are setting it for overtime
    {                                                                         //if 'replay' is true, means we are getting the game completely to initial state
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

        Messenger.Broadcast(SetEverythingBackMessage);    //Broadcast the message for all stuff that we don't have reference to in GameController

        PlayerOne.GetComponent<PlayerShooting>().SetEverythingBack();   //Launch it on PlayerShooting as well
        PlayerTwo.GetComponent<PlayerShooting>().SetEverythingBack();

        foreach (Powerup powerup in Powerups)   //Disable all powerups
        {
            powerup.gameObject.SetActive(false);
        }
        delayRunning = false;               //Set flag so no powerup delay is running
        StopCoroutine(nameof(SpawnDelay));          //Stop spawn delay
        PowerupSpawningSequence();              //Start spawn delay

        PeriodEnding = false;         //Set so period isn't ending

        if (replay == false)    //If its not a replay, set all the next perios stuff for scoreboard
        {
            if (overtime == false)  //If it's no overtime
            {
                GameTime = PeriodTime;        //Set period time back
                scoreBoard.NextPeriod();      //Set period circles on scoreboard
                StartCoroutine(nameof(PeriodCountdown));  //Start period countdown
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
        }
        
    }


    public void ReplayGame()    //Function that resets all the stuff when pressed "Replay Game" after finishing the game 
    {
        StopCoroutine(nameof(GameTimeCounter)); //Stop game time counter
        TotalGameTime = 0;                      //Clear the variable that counts it
        StartCoroutine(nameof(GameTimeCounter));//Restart it (it will start counting as soon as timeScale becomes 1

        StopCoroutine(nameof(PeriodCountdown)); //Stop period countdown in case we launched replay game from pause menu in the middle of the round

        PlayerOne.playerStats = new PlayerStats();    //Recreate the instance of playerStats to clear their values
        PlayerTwo.playerStats = new PlayerStats();       
        
        if (NumberOfPeriods == 0)   //If number of periods is 0, means the game mode is "Play To Score"
        {
            isPlayToScore = true;
            PeriodTime = InitialPeriodTime;     //In that case, InitialPeriodTime holds the amount of score needed to end the game
        }
        else
        {
            isPlayToScore = false;
            PeriodTime = InitialPeriodTime * 60;    //If its time-based, then multiply the amount of minutes set in InitialPeriodTime to put seconds into PeriodTime
        }

        if (isPlayToScore == false)   //If it is not play to score
        {
            GameTime = PeriodTime;  //Set back period time
            CurrentPeriod = 1;      //Set first period
            scoreBoard.ResetPeriodCircles();    //Launch a function on scoreboard to disable all red period circles
            scoreBoard.NextPeriod();            //Enable first circle
            
            StartCoroutine(nameof(PeriodCountdown));    //Start period countdown
        }

        scoreBoard.UpdateScore();   //Update score on scoreboard after player stats were cleared (set 0-0 score)
        
        audioManager.music.Play();      //Play and pause music for starting playing in the moment of period starting
        audioManager.music.Pause();


    }

#endregion
    
}
