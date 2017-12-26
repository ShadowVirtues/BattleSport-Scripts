using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

//StartupController is present in menu scenes where you start setting up the game, then persists until the actual game scene. During setting up the game, different menu controllers 'Find" StartupController object, and fill their settings in it

public class StartupController : MonoBehaviour
{
    public static StartupController Controller; //Reference to current instance of StartupController, now only gets used to set up the arena AFTER one frame of LoadScene, and BEFORE all arena objects Awakes
    
    public GameObject[] Tanks;      //StartupController has all the gameplay Tanks prefabs to inject chosen ones into loaded arena scene
    public GameObject[] Balls;      //Same for all balls
    public GameObject[] Powerups;   //COMM
    public GameObject PlayerPrefab; //And PlayerPrefab that also gets injected

    [SerializeField] private GameObject audioManagerPrefab;     //AudioManagerPrefab that also gets loaded into arena from here (to not have it referenced on each arena GameController
    [SerializeField] private GameObject GameUIPrefab;                 //Prefab of GameUI having all stuff like starting countdown, options menu, periods UI, GameStats
    [SerializeField] private GameObject ballCameraPrefab;       //Prefab with camera for Ball Possession, that also has EventSystem on it

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
        Controller = this;      //Set static controller reference

        DontDestroyOnLoad(gameObject);  //Make it not get destroyed in further scenes (if you go back to main menu, it gets destroyed manually)
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
        }
        else
        {
            gameController.isPlayToScore = false;
            gameController.PeriodTime = PeriodTime * 5;    //If its time-based, then multiply the amount of minutes set in StartupController to put seconds into GameController
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
         
        GameObject powerupContainer = new GameObject("Powerups");   //COMM

        if (arena.Powerups != null)
        {
            for (int i = 0; i < arena.Powerups.Length; i++)
            {
                int powerupIndex = Array.FindIndex(Powerups, x => x.name == arena.Powerups[i].name);
                gameController.Powerups.Add(Instantiate(Powerups[powerupIndex], powerupContainer.transform).GetComponent<Powerup>());
                gameController.Powerups[i].gameObject.SetActive(false);
            }
        }
        

        







        CustomInputModule.Instance.Menu = false;    //Disabling universal input when in game
        CustomInputModule.Instance.Enabled = true;  //Enabling flag so when universal input does get enabled (in controls menu), we don't have to set it
    }



}
