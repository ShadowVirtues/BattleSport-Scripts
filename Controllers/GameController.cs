using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;


/*
    THERE WILL BE A SCRIPT IN THE MENU LIKE StartupContoller, where all the shit like game length, possession time, arena will be filled. From that script you "Find" this GameController, 
    that has all the references to stuff like PlayerOne, PlayerTwo, Scoreboard, Ball, Goal.







*/





public class GameController : MonoBehaviour
{
    public static GameController Controller;

    public static Announcer announcer;

    public Goal goal;
    public ScoreBoard scoreBoard;
    public Player PlayerOne;
    public Player PlayerTwo;
    public Ball ball;

    public Transform PlayerOneSpawn;
    public Transform PlayerTwoSpawn;
    public Transform BallSpawn;


    public int ShotClock;
    public int PeriodTime;
    public int NumberOfPeriods;

    




    public float ArenaDimension;    //This is the size of the arena in one dimension (X or Y, cuz they are equal) 
    public AudioClip Music;

    [SerializeField] private GameObject audioManagerPrefab;
    private GameObject audioManagerObject;

    public static AudioManager audioManager;


    void Awake()
    {
        Controller = this;

        StartupController.Controller.LoadItemsToArena();

        

        

        audioManagerObject = Instantiate(audioManagerPrefab);
        announcer = audioManagerObject.GetComponent<Announcer>();
        audioManager = audioManagerObject.GetComponent<AudioManager>();

        

        //ShotClock = 10;
        //PeriodTime = 180;
        //ArenaDimension = 100;       //TODO Get the size from arena 'whatever' (prefab, ScriptableObject) and convert it to float
        GameTime = PeriodTime;

        
        StartCoroutine(PeriodCountdown());

        
    }

    void Start()
    {
        if (ball.gameObject.name == "ElectricBall") //KLUUUUUUUUUUUUUUDGEEEEEEEEEEEEEEEEEEEEE (DELETE this shit tho)
            GameObject.Find("BallCamera").GetComponent<Camera>().backgroundColor = new Color(49f / 256, 77f / 256, 121f / 256, 0);

        Pause();
    }

    
    public int GameTime;

    private readonly WaitForSeconds second = new WaitForSeconds(1);

    IEnumerator PeriodCountdown()
    {
        while (GameTime > 0)
        {
            
            yield return second;
            GameTime--;
            scoreBoard.UpdateTime();
            
            
        }
        print("PERIOD ENDED");
        //TODO END PERIOD


    }

    public bool paused = false;    //COMM


    public void Pause()
    {
        if (paused == false)
        {
            Time.timeScale = 0;
            paused = true;
            audioManager.music.Pause();
        }
        else
        {
            Time.timeScale = 1;
            paused = false;
            audioManager.music.UnPause();
        }

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
