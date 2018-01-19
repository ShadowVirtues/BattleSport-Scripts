using System;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

//StartupController is present in menu scenes where you start setting up the game, then persists until the actual game scene. During setting up the game, different menu controllers reference StartupController object, and fill their settings in it

public class StartupController : MonoBehaviour
{
    public static StartupController Controller; //Reference to current instance of StartupController, 
    
    [Header("Instantiatable Objects")]  //All stuff that StartupController can instantiate
    [SerializeField] private GameObject[] Tanks;      //StartupController has all the gameplay Tanks prefabs to inject chosen ones into loaded arena scene
    [SerializeField] private GameObject[] Balls;      //Same for all balls
    [SerializeField] private GameObject[] Powerups;   //All powerups
    [SerializeField] private GameObject PlayerPrefab; //And PlayerPrefab that also gets injected

    [SerializeField] private GameObject audioManagerPrefab;     //AudioManagerPrefab that also gets loaded into arena from here (to not have it referenced on each arena GameController
    [SerializeField] private GameObject GameUIPrefab;                 //Prefab of GameUI having all stuff like starting countdown, options menu, periods UI, GameStats
    [SerializeField] private GameObject ballCameraPrefab;       //Prefab with camera for Ball Possession

    [Header("Quick Match")]
    [SerializeField] private QuickMatch quickMatch;     //Reference to script in MainMenu, where all the stuff for quick match is held

    [Header("Not getting set into Inspector")]  //TODO [HideInInspector]. For now they are visible for testing purposes
    public Arena arena;
    public int ShotClock;
    public int PeriodTime;
    public int NumberOfPeriods;     //All the settings that get set in menus before the game
    public string PlayerOneTank;
    public string PlayerTwoTank;
    public string PlayerOneName;
    public string PlayerTwoName;

    void Awake()
    {
        if (Controller == null)
        {
            DontDestroyOnLoad(gameObject);
            Controller = this;
        }
        else if (Controller != this)
        {
            Destroy(Controller.gameObject);
            DontDestroyOnLoad(gameObject);
            Controller = this;
        }
    }

    public void GAMEButtonPress()   //This gets launched in the very end screen of the menu before the game
    {       
        SceneManager.LoadScene(arena.Scene);    //Load arena scene. All setting up of the arena gets invoked from actual scene via StartupController.Controller.LoadItemsToArena
    }
    
    public void LoadItemsToArena(GameController gameController)      //This gets invoked from arena's GameController, passing its reference so we can fill its fields and use its functions
    {      
        gameController.ShotClock = ShotClock;               //Setting all settings from StartupController to GameController of the loaded scene
        gameController.NumberOfPeriods = NumberOfPeriods;
        if (NumberOfPeriods == 0)   //If number of periods is 0, means the game mode is "Play To Score"
        {
            gameController.isPlayToScore = true;
            gameController.PeriodTime = PeriodTime;     //In that case, PeriodTime holds the amount of score needed to end the game
            gameController.InitialPeriodTime = PeriodTime;  //Storing the initial value so we can recover when replaying game (to consider case when period time gets increased by 1 when entering overtime)
        }
        else
        {
            gameController.isPlayToScore = false;
            gameController.PeriodTime = PeriodTime * 60;   //If its time-based, then multiply the amount of minutes set in StartupController to put seconds into GameController
            gameController.InitialPeriodTime = PeriodTime;
        }
        
        gameController.ArenaDimension = (int)arena.Size;    //arena.Size is an enum that has its int identifiers set to actual arena dimensions
        gameController.Music = arena.Music;                 //Music is arena-specific

        //Instantiate particular ball, which is set into arena file, into BallSpawn transform, which is the empty transform in the arena. Athe the same time get the referenct to ball component and assign it to GameController
        gameController.ball = Instantiate(Balls[(int)arena.ballType], gameController.BallSpawn.position, gameController.BallSpawn.rotation).GetComponent<Ball>();      
        gameController.PlayerOne = Instantiate(PlayerPrefab, gameController.PlayerOneSpawn.position, gameController.PlayerOneSpawn.rotation).GetComponent<Player>();   //Same stuff for players
        gameController.PlayerTwo = Instantiate(PlayerPrefab, gameController.PlayerTwoSpawn.position, gameController.PlayerTwoSpawn.rotation).GetComponent<Player>();   
        
        int playerOneTankIndex = Array.FindIndex(Tanks, x => x.name == PlayerOneTank);  //Tanks to assign to each player are identified by the name of their prefab, so find the prefab in the array with that name, and remember its index
        int playerTwoTankIndex = Array.FindIndex(Tanks, x => x.name == PlayerTwoTank);

        Instantiate(Tanks[playerOneTankIndex], gameController.PlayerOne.transform);     //Instantiate the tank with the corresponding index into Player object
        Instantiate(Tanks[playerTwoTankIndex], gameController.PlayerTwo.transform);

        gameController.PlayerOne.PlayerNumber = PlayerID.One;                           //Set player numbers
        gameController.PlayerTwo.PlayerNumber = PlayerID.Two;

        gameController.PlayerOne.SetPlayer();                       //Run the function to set all the stuff for players, like camera viewport, HUD Canvas Rect, corresponding layers
        gameController.PlayerTwo.SetPlayer();

        gameController.PlayerOne.PlayerName = PlayerOneName;        //Set player names
        gameController.PlayerTwo.PlayerName = PlayerTwoName;
       
        gameController.audioManagerObject = Instantiate(audioManagerPrefab);        //Instantiate AudioManager
        gameController.gameUI = Instantiate(GameUIPrefab).GetComponent<GameUI>();   //Instantiate GameUI and get reference to its script component

        GameObject ballCamera = Instantiate(ballCameraPrefab);      //Spawning ball camera under the map (for showing rotating ball on player UI)
        if (arena.ballType == Ball.BallType.Electric) ballCamera.GetComponent<Camera>().backgroundColor = new Color(49f / 256, 77f / 256, 121f / 256, 0);   //For electric ball, set the camera color, so it shows with proper colors on UI (render texture absence of proper particle shader workaround)
         
        GameObject powerupContainer = new GameObject("Powerups");   //Make a container for powerups

        if (arena.Powerups != null) //If there are any powerups for the arena
        {
            for (int i = 0; i < arena.Powerups.Length; i++) //For all of them
            {
                int powerupIndex = Array.FindIndex(Powerups, x => x.name == arena.Powerups[i].name);    //Find the index in StartupController powerup collection from the name in arena powerups array
                gameController.Powerups.Add(Instantiate(Powerups[powerupIndex], powerupContainer.transform).GetComponent<Powerup>());   //Add to the list of powerups in gameController instantiated powerup with an index to the container, and get Powerup component. ALL. AT. THE. SAME. TIME. .
                gameController.Powerups[i].gameObject.SetActive(false); //Disable all spawned powerups
            }
        }
        
        CustomInputModule.Instance.Menu = false;    //Disabling universal input when in game
        CustomInputModule.Instance.Enabled = true;  //Enabling flag so when universal input does get enabled (in controls menu), we don't have to set it
        
    }

    public void QuickMatch()    //Function that randomly fills all controller values to start Quick Match
    {
        arena = quickMatch.AllArenas[UnityEngine.Random.Range(0, quickMatch.AllArenas.Length)]; //Randomly pick an arena from object in MainMenu that has references to them
        
        int tankOption = UnityEngine.Random.Range(0, quickMatch.tankOptions.Count);     //Randomly pick the entry in array with possibilities of tank cases
        bool tankMutual = UnityEngine.Random.value > 0.5f;  //Randomly generate bool to pick which of two tanks will go to each player

        if (tankMutual)
        {
            PlayerOneTank = quickMatch.tankOptions[tankOption][0];  //Set first tank to first player, second tank to second player
            PlayerTwoTank = quickMatch.tankOptions[tankOption][1];
        }
        else
        {
            PlayerOneTank = quickMatch.tankOptions[tankOption][1];  //Set second tank to first player, first tank to second player
            PlayerTwoTank = quickMatch.tankOptions[tankOption][0];
        }

        bool scoreBased = UnityEngine.Random.value > 0.75f;     //Chance of getting score-based game instead of time-based is 25%

        if (scoreBased)
        {
            NumberOfPeriods = 0;    //Set number of periods to 0, if score-based
            PeriodTime = 15;        //Set score to reach to 15 (yes, to PeriodTime)
        }
        else    //If time-based
        {
            NumberOfPeriods = UnityEngine.Random.Range(3, 6);   //Randomly pick between 3,4 or 5 periods
            PeriodTime = 3; //Set period time to 3 minutes
        }

        ShotClock = UnityEngine.Random.Range(5, 16);    //Randomly pick ShotClock between 5 and 15 seconds
        if (arena.Number == 61) //Specifically for arena 61 set it starting from 10 seconds (which is still a little for this arena)
        {
            ShotClock = UnityEngine.Random.Range(10, 16);
        }
        
        PlayerOneName = "PLAYER 1"; //Set default names for both players
        PlayerTwoName = "PLAYER 2";
    }



    



}
