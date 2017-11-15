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
    //public ScoreBoard scoreBoard;
    public Player PlayerOne;
    public Player PlayerTwo;
    public Ball ball;

    public int ShotClock;

    void Awake()
    {
        Controller = this;
        announcer = GetComponent<Announcer>();
        ShotClock = 10;
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
