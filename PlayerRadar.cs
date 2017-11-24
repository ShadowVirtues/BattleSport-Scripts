using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

public class PlayerRadar : MonoBehaviour
{
    [SerializeField] private RectTransform ballRadar;
    [SerializeField] private RectTransform goalRadar;   //References to icons on the radar
    [SerializeField] private RectTransform enemyRadar;   

    private Transform player;       //Current player transform

    private Transform enemy;
    private Transform ball;     //Transforms of respective objects in the arena, which positions will be shown on the radar
    private Transform goal;

    private float arenaSize;       //This is the diagonal size of the arena
    private float radarDimension = 120; //This is the radius of the radar in units on the UI

    public static bool ballPossession;  //Static variable to know if someone possesses the ball to stop moving it on the radar
    
    public static void HideBallFromRadars(bool state)   //Static function to hide or reveal the ball on the radar if someone possesses/loses the ball
    {
        GameController.Controller.PlayerOne.playerRadar.ballRadar.gameObject.SetActive(!state);     //Really long path to get those ball references, but I couldn't find a better approach
        GameController.Controller.PlayerTwo.playerRadar.ballRadar.gameObject.SetActive(!state);
    }

    public static void HidePlayerFromRadar(PlayerID playerNumber, bool state)   //Hide/reveal some player from radar. Gets called from player that gets exploded, hides/reveals his icon from enemy radar
    {
        if (playerNumber == PlayerID.One)
        {
            GameController.Controller.PlayerTwo.playerRadar.enemyRadar.gameObject.SetActive(!state);
        }
        else if (playerNumber == PlayerID.Two)
        {
            GameController.Controller.PlayerOne.playerRadar.enemyRadar.gameObject.SetActive(!state);
        }

    }

    void Awake ()
    {
        Player playerPlayer = GetComponent<Player>();   //Get the Player reference to figure out which player number it is

        player = GetComponent<Transform>();     //Get the reference of transform of current player (cache it, cuz we use it multiple times every single frame)

        if (playerPlayer.PlayerNumber == PlayerID.One)  //If this is player one, set his enemy to player two, and vice versa
        {
            enemy = GameController.Controller.PlayerTwo.transform;
        }
        else if (playerPlayer.PlayerNumber == PlayerID.Two)
        {
            enemy = GameController.Controller.PlayerOne.transform;
        }

        ball = GameController.Controller.ball.transform;    //Get reference of ball and goal transforms
        goal = GameController.Controller.goal.transform;
    }

    void Start()
    {
        float arenaDimension = GameController.Controller.arenaDimension;

        arenaSize = Mathf.Sqrt(arenaDimension * arenaDimension * 2);       //Get the arena size from GameController
    }
    
    private Vector2 RadarPosition(Transform item)   //Function that converts the relative position of items in the arena to their position on the radar
    {
        float itemX = (item.position.x - player.position.x) / arenaSize * radarDimension;  //Subtract X and Z components of the player from the item to get the relative position, normalize them to radar dimensions
        float itemZ = (item.position.z - player.position.z) / arenaSize * radarDimension;

        Vector3 relative = player.TransformDirection(itemX, 0, -itemZ); //Transform direction of the item to be relative to players rotation
        return new Vector2(relative.x, -relative.z);    //Return the coordinates where to put the icon on the radar
        //I was kinda randomly putting '-'s during those transformations until I got the radar to show the relative position accordingly, not sure why you need to put those '-'s there
    }

	void Update ()
	{        
	    if (ballPossession == false)    //Only if no one possesses the ball show it on the radar
	    {
	        ballRadar.anchoredPosition = RadarPosition(ball);
        }	    
	    goalRadar.anchoredPosition = RadarPosition(goal);   //Show all the items on the radar
	    enemyRadar.anchoredPosition = RadarPosition(enemy);
    }
}
