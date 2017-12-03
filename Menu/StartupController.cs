using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupController : MonoBehaviour
{
    public static StartupController Controller;


    public GameObject[] Tanks;
    public GameObject[] Balls;
    public GameObject PlayerPrefab;

    [SerializeField] private GameObject audioManagerPrefab;

    [Header("Not getting set into StartupController")]
    public Arena arena;
    public int ShotClock;
    public int PeriodTime;
    public int NumberOfPeriods; //[HideInInspector]
    public string PlayerOneTank;
    public string PlayerTwoTank;


    void Awake()
    {
        Controller = this;

        
    }

    public void GAMEButtonPress()
    {
        DontDestroyOnLoad(gameObject);

        //StartCoroutine(ArenaLoadSequence());

        SceneManager.LoadScene(arena.Scene);

        





        //Load Arena Scene
        //Find GameController in it
        //Put all the game parameters in GameController
        //Get PlayerOneSpawn, PlayerTwoSpawn, BallSpawn from GameController
        //Place PlayerPrefab's and a ball into those spots, assign them to GameController
        //Place Tanks into PlayerPrefab's
        //Have GameController SetPlayer's



        //Need to make UI Canvas over in-game player UIs for fading stuff, for writing "Approaching Arena", and there also have "Period" screen
        //TO CONSIDER: UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
    }

    //IEnumerator ArenaLoadSequence()
    //{
        

    //    yield return null;

        


    //}

    public void LoadItemsToArena()
    {
        //GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();

        gameController.ShotClock = ShotClock;
        gameController.PeriodTime = PeriodTime * 60;
        gameController.NumberOfPeriods = NumberOfPeriods;
        gameController.ArenaDimension = (int)arena.Size;    //TEST
        gameController.Music = arena.Music;

        gameController.ball = Instantiate(Balls[(int)arena.ballType], gameController.BallSpawn.position, gameController.BallSpawn.rotation).GetComponent<Ball>();      //?????
        gameController.PlayerOne = Instantiate(PlayerPrefab, gameController.PlayerOneSpawn.position, gameController.PlayerOneSpawn.rotation).GetComponent<Player>();        //???
        gameController.PlayerTwo = Instantiate(PlayerPrefab, gameController.PlayerTwoSpawn.position, gameController.PlayerTwoSpawn.rotation).GetComponent<Player>();
        
        int playerOneTankIndex = Array.FindIndex(Tanks, x => x.name == PlayerOneTank);
        int playerTwoTankIndex = Array.FindIndex(Tanks, x => x.name == PlayerTwoTank);

        Instantiate(Tanks[playerOneTankIndex], gameController.PlayerOne.transform);
        Instantiate(Tanks[playerTwoTankIndex], gameController.PlayerTwo.transform);

        gameController.PlayerOne.PlayerNumber = PlayerID.One;
        gameController.PlayerTwo.PlayerNumber = PlayerID.Two;

        gameController.PlayerOne.SetPlayer();
        gameController.PlayerTwo.SetPlayer();

        gameController.audioManagerObject = Instantiate(audioManagerPrefab);
    }


    //void OnEnable()
    //{
    //    //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
    //    SceneManager.sceneLoaded += OnLevelFinishedLoading;
    //}

    //void OnDisable()
    //{
    //    //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
    //    SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    //}

    //void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    //{
    //    Debug.Log("Level Loaded");
    //    Debug.Log(scene.name);
    //    Debug.Log(mode);
    //}

}

/*
 WHAT TO SET BEFORE PLAYING
        Arena
        ShotClock
        PeriodTime
        NumberOfPeriods

        Tanks



    

 */
