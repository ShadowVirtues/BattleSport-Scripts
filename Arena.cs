using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum ArenaSize { Tiny, Small, MidSized, Large, Humongous }



public class Arena : ScriptableObject
{
    public int Number;
    public string Name;
    public SceneField Scene;
    public ArenaSize Size;
    public Goal.GoalType goalType;
    public string goalDescription;
    public Ball.BallType ballType;

    public AudioClip Music;
    //public Powerup[] PowerUps








    /*
     

        MAIN MENU ITEMS FOR NOW:
        Instant Action Setup
        2 Player Exhibition
        Optons


        WHAT TO SET BEFORE PLAYING
        Arena
        ShotClock
        PeriodTime
        NumberOfPeriods

        Tanks







      ARENA PARAMETERS (for prefab, ScriptableObject):
        1. Size -> GameController.Controller.arenaDimension
        2. Number
        3. Name (includes number, for the end levels without number)
        4. Actual scene with the arena
        5. Powerups??? (or they will be injected into scene already)
        6. GoalType,BallType?? (it will be injected, yes, but for the arena selection in the menu)
        7. Music

    */
}
