using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ArenaSize { Tiny, Small = 60, MidSized = 75, Large, Humongous = 100 }

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
    
}
