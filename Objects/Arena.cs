using UnityEngine;
using UnityEngine.Video;

public enum ArenaSize { Tiny = 50, Small = 60, MidSized = 75, Large = 90, Humongous = 100 }

public class Arena : ScriptableObject
{
    public int Number;                  //Number of the arena
    public string Name;                 //Name of the arena to show in the selector (includes number so we can make unnumbered arenas)
    public SceneField Scene;            //Actual scene file with the arena
    public ArenaSize Size;              //Size of the arena
    public Goal.GoalType goalType;      //Type of the goal in it for showing it when selecting arena and CountdownPanel
    public string goalDescription;      //Description of the goal if its not "Normal"
    public Ball.BallType ballType;      //Type of the ball in it for the same stuff as for goal, but also for spawning specific one into arena
    public string ballDescription;      //Description of the ball if its not "Normal"
    public VideoClip arenaVideo;        //Video with the arena for its preview  //Record in 690x270 resolution

    public AudioClip Music;             //Music to play in the arena

    public Powerup[] Powerups;          //Powerups that are gonna spawn in this arena

}
