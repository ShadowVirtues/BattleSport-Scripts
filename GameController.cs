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

    public int ShotClock;

    public float arenaSize = 60;    //TODO Get the size from arena 'whatever' (prefab, ScriptableObject) and convert it to float

    void Awake()
    {
        Controller = this;
        announcer = GetComponent<Announcer>();
        ShotClock = 10;
        PeriodTime = 180;
        GameTime = PeriodTime;


        StartCoroutine(PeriodCountdown());
    }

    public int PeriodTime;

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
