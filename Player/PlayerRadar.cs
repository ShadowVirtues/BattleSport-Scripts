using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRadar : MonoBehaviour
{
    [SerializeField] private RectTransform ballRadar;
    [SerializeField] private RectTransform goalRadar;   //References to icons on the radar
    [SerializeField] private RectTransform enemyRadar;

    public RectTransform radarBackground;   //Background image of radar, gets set directly from PauseMenu.cs, that's why public
    public CanvasScaler canvasScaler;       //Canvas Scaler component of Player HUD Canvas, for easy UI scaling

    //========Stuff for scaling radar=================
    private float radarScale = 1;  //Radar Scale coefficient to multiply icons position by (their position does not depend on radar scale directly via Rect Transform)
    private float iconsScale = 1;  //Icons Scale to multiply icons scale along with radar scale
    private Vector2 initialRadarScale;  //Initial radar scales to have a starting point of what to multiply
    private Vector2 initialBallScale;
    private Vector2 initialGoalScale;
    private Vector2 initialEnamyScale;
    //===========================================

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

    void Awake()
    {
        initialRadarScale = radarBackground.sizeDelta;  
        initialBallScale  = ballRadar .sizeDelta;       //Getting dose initial scales (in Awake, cuz that's when the settings are getting applied)
        initialGoalScale  = goalRadar .sizeDelta;       
        initialEnamyScale = enemyRadar.sizeDelta;        
    }

    void Start()    //No Awake, cuz no references after the scene load
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
        goal = GameController.Controller.goal.ballCollider; //Not the goal itself, cuz we move the child object of it
        
        float arenaDimension = GameController.Controller.ArenaDimension;

        arenaSize = Mathf.Sqrt(arenaDimension * arenaDimension * 2);       //Get the arena size from GameController
    }
    
    private Vector2 RadarPosition(Transform item)   //Function that converts the relative position of items in the arena to their position on the radar
    {
        float itemX = (item.position.x - player.position.x) / arenaSize * radarDimension;  //Subtract X and Z components of the player from the item to get the relative position, normalize them to radar dimensions
        float itemZ = (item.position.z - player.position.z) / arenaSize * radarDimension;

        Vector3 relative = player.TransformDirection(itemX, 0, -itemZ); //Transform direction of the item to be relative to players rotation
        return new Vector2(relative.x * radarScale, -relative.z * radarScale);    //Return the coordinates where to put the icon on the radar, counting radar scale
        //I was kinda randomly putting '-'s during those transformations until I got the radar to show the relative position accordingly, not sure why you need to put those '-'s there
    }

    public void ApplyRadarScale(float value)    //Function that gets run in the start of the game to apply settings and when players change the value in settings
    {
        radarBackground.sizeDelta = initialRadarScale * value;          //No icons scales for radar scale
        ballRadar.sizeDelta = initialBallScale * iconsScale * value;    //Set the scales counting both setting value and icons scale
        goalRadar.sizeDelta = initialGoalScale * iconsScale * value;
        enemyRadar.sizeDelta = initialEnamyScale * iconsScale * value;

        radarScale = value;     //Set the coefficient to count during icon scale setting and in radar position calculation
        try
        {
            UpdateRadar();   //This is to apply the position when scaling the radar. Will not have all references when applying the scale in the start of the game,
        }                    //that's why simply not executing it if it can't. It will get executed during setting the value in settings
        catch {}


    }

    public void ApplyIconsScale(float value)    //The same, but only for icons
    {
        ballRadar.sizeDelta = initialBallScale * radarScale * value;
        goalRadar.sizeDelta = initialGoalScale * radarScale * value;
        enemyRadar.sizeDelta = initialEnamyScale * radarScale * value;

        iconsScale = value;
    }

	void Update ()
	{
	    if (Time.timeScale == 0) return;    //Don't update the radar during the pause

	    UpdateRadar();
    }

    private void UpdateRadar()
    {
        if (ballPossession == false)    //Only if no one possesses the ball show it on the radar
        {
            ballRadar.anchoredPosition = RadarPosition(ball);
        }
        goalRadar.anchoredPosition = RadarPosition(goal);   //Show all the items on the radar
        enemyRadar.anchoredPosition = RadarPosition(enemy);

    }
}
