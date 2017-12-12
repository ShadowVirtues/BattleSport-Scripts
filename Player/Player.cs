using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


/*
    ===================GLOBAL CONCEPTS========================
    


    ==========================================================


    
    DO NEXT:       
    Small lag when you first score??

    Manage EventSystem in all scenes. Probably spawn it.
    Also spawn BallCamera, and there set the camera color
    Delete BallCamera and EventSystem from all arena scenes

   
    
    Menu options:
    IU Scale
    Map Scale
    Map Icons Scale
    Map Transparency

    Music
    SFX
    Announcer
    


    GENERAL THINGS TO DO:

        
        Game End
        
        
        Powerups  
        Arena Preview (TUGUSH-TUGUSH)
        Pause the game
        Options > KeyBindings
        Smooth mouse/Stick controls
        10 Levels > Props, Skyboxes
        Design Main Menu       
        2 Player Exhibition (Maybe make an arena screenshot showing, rotating video or whatever)
        


    ADDITIONAL IDEAS:
    Maybe recover balls position after scoring to get rid of that jitter, using bounds stuff?
    When hit, slider slowly going down with red trail like in LOL
    When picked powerup, there should be a tooltip with circular “slider” going representing the time left for it.


    ARENA PARAMETERS (for prefab, ScriptableObject):
    1. Size -> GameController.Controller.arenaDimension
    2. Number
    3. Name (includes number, for the end levels without number)
    4. Actual scene with the arena
    5. Powerups??? (or they will be injected into scene already)
    6. GoalType,BallType?? (it will be injected, yes, but for the arena selection in the menu)

    WHEN REPLACING SOMETHING LIKE BALL OR GOAL:
    1. SET THE GameController REFERENCE!!!

    THINGS TO DO WHEN "INJECTING" ALL THE STUFF IN THE ARENA:
    1. Inject player tanks:
        a) Set camera viewports and UI panel RectTransform 
        b) Set PlayerX layer on injected tanks
        c) Disable PlayerX for PlayerX in Camera Culling mask
        

    THINGS TO CONSIDER WHEN MAKING NEW ARENA
    1. Set all the stuff to "static", Colliders for props!!!
    2. Set light, set skybox
    3. Bake lighting, set Fog
    4. Set all layers to geometry and all interactibles:
        a) Walls (LevelGeometry)
        b) Floor (Floor)
        c) Static Props (LevelGeometry)
        d) Players (PlayerOne/Two)
        e) PlayerExplosion
        f) BallTrigger, BallCollider
        g) GoalSolid, GoalBallSolid
    5. Rocket Height Trigger
    6. PlayerOne/Two/Ball Spawns
    7. GameController References
    


*/

public static class Message //Just a class container for all on-screen messages for players
{
    public const string Score = "SCORE";
    public const string Possession = "POSSESSION";
    public const string Fumble = "FUMBLE";
    public const string EnemyDestroyed = "ENEMY DESTROYED";
    public const string Miss = "MISS";
    public const string Violation = "VIOLATION";

    public const string Overtime = "OVERTIME";
    public const string ScoreToWin = "SCORE TO WIN";
    //TODO Add powerup messages
}

[Serializable]
public class PlayerStats    //Class-container for all end-game stats
{
    public int Score;
    public int ShotsOnGoal;

    public int MissilesFired;
    public int MissilesHit;

    public int Fumbles;
    public int Interceptions;

    public int Deaths;      //In end-game screen this will be swapped for players into "Kills" field

    public float PossessionTime;    
}

public class Player : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenuItem("Set Player", nameof(SetPlayer))]    //BADASS THING in the editor to right click "PlayerNumber" field for the player to have a MenuItem to set all stuff like Camera viewport, layers and UI for the selected value in that field
#endif
    public PlayerID PlayerNumber; //From TeamUtility InputManager "Add-on". Sets which player it is

    public PlayerStats playerStats; //Instance of PlayerStats so we could fill them with all the stats

    [Header("Health")]          //All the stuff about health
    [SerializeField] private Slider healthSlider;   //Reference to slider to change its value depending on the player health
    [SerializeField] private new Camera camera;     //Reference to player's camera to shake it on rocket hit
    private float Health = 100;  //Tank Health variable, changes when taking damage

    [Header("Explosion")]                   //All the stuff about explosion
    [SerializeField] private ParticleSystem particleSmoke;    //Reference to two particle systems which lengths depend on the death timer, we change their length from those references
    [SerializeField] private ParticleSystem particleExplosion;

    [SerializeField] private GameObject explosion;  //Explosion particle system object parented to player object to enable/disable it when exploding   
    [SerializeField] private GameObject deathScreen;    //Reference to UI image covering the screen when dead to enable/disable it when exploding
    [SerializeField] private Text deathTimer;           //Reference to death timer text to count it down when dead
    private Animator cameraAnim;       //Reference to camera animator to play the animation when exploded (getting it from Camera camera)
    private const string explodeAnimName = "ExplodeCamera";   
    private const string emissionColor = "_EmissionColor";    //Caching those strings for no garbage

    private int explodedTime = 2;                       //Default time being dead (increases by 1 sec up to 4 sec every 5 deaths)
    private readonly WaitForSeconds secondOfDeath = new WaitForSeconds(1);  //One second of death (we wait for it in the coroutine, then decrease the timer time, then wait again, and so on), used in the BallClock timer
    private readonly WaitForSeconds deathScreenDelay = new WaitForSeconds(0.5f);  //Time during which play camera animation before entering death screen
    private readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();        //Caching this as well, for enabling enemy radar icon

    [Header("Ball")]            //All the stuff about the ball
    [SerializeField] private RectTransform ballUI;      //Reference to the "ball-container" UI element that raises from below when player picks up a ball
    [SerializeField] private GameObject ballCamera;     //Reference to the RawImage object containing RenderTexture of the camera looking at the ball to disable it when the player loses the ball

    private Ball ball;          //Caching the shorter reference to the ball from GameController   

    private bool possession;        //Bool representing if the player has a ball (used when fumbling and when shooting the ball)
    [HideInInspector] public PlayerRadar playerRadar;        //Reference to playerRadar of current player to be able to get the reference to the ball icon on the radar in the static function
    private float ballShootForce;           //Dependant on tank parameter BallHandling
    private float pickupTime;               //The time moment in seconds from the start of the game the ball got picked up (to count the PossessionTime for stats)

    [SerializeField] private AudioSource pickup;    //"Self-interruptible" audioSource player the pickup sound (ball shot is the same sound as well)

    [SerializeField] private GameObject ballClock;      //Reference to Shot Clock UI Image to disable/enable it when have or don't have the ball
    [SerializeField] private Text ballClockText;        //Reference to text timer on the Shot Clock UI
    [SerializeField] private GameObject endPeriodTimer; //Render texture with the camera view on the scoreBoard to see the period countdown before it ends 

    [SerializeField] private GameObject scoreImage;     //Reference to score panel-like image where you write score text that you enable when scoring
    [SerializeField] private Text scoreText;            //Reference to score text to change it when you score                                                 

    //====================OTHER=====================

    //Reference for being able to smoothly stop the exploding tank with setting its drag (we disable PlayerMovement script when dead, so the player can't move dead ship...
    private Rigidbody playerRigidbody;      //And in PlayerMovement custom drag when alive is implemented, so we apply the internal rigidbody drag when dead
    private PlayerMovement movement;        //Reference to disable it when dead
    private Tank tank;                  //Reference to Tank component of attached tank to get its characteristics
    [Header("Other UI")]
    [SerializeField] private Text messageBox;   //Text box in the middle of the screen to show messages on for players
    [HideInInspector] public string PlayerName;     //Player Name that gets set in the menu
    [SerializeField] private Text playerNameUI;   //Player Name textBox below player health

    private Collider[] spawnCheckColliders = new Collider[2];   //Premade array of colliders to not have any garbage allocated when checking where to spawn a tank after death with "CheckCapsule"

    private Material material;          //Tank material to modify it when the tank gets hit or picks up the ball

    private const string ballButtonName = "ShootBall";    //Caching button name for shooting the ball

    //=============================================

    void Awake()
    {
        playerRadar = GetComponent<PlayerRadar>();      //We need playerRadar reference in Awake, since that's when UI scale settings are getting applied
    }

    void Start() //Since when loading the scene, we first spawn PlayerPrefab (which would INSTANTLY run Awake here) and only after that we get the Tank in, we have to get all references in Start, when the Tank has already been put in
    {
        ball = GameController.Controller.ball;

        tank = GetComponentInChildren<Tank>();              //Getting dose references
        
        material = tank.GetComponent<Renderer>().material;
         
        playerRigidbody = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();

        cameraAnim = camera.GetComponent<Animator>();

        playerNameUI.text = PlayerName;     //Set player name on the UI

        ballShootForce = tank.BallHandling;     //BallHandling means the force the player shoots the ball with
        playerRigidbody.mass = tank.Armor / 10; //Setting tank mass to tenth of the armor, so more armored tanks throw the less armored across the map when colliding with them
    }

    private IEnumerator Explode()   //Process of exploding the player
    {       
        movement.enabled = false;   //Disable players ability to move with buttons
        playerRigidbody.drag = 3;   //Apply drag so the player object stops over some time when exploding, instead of instantly stopping 
        tank.gameObject.SetActive(false); //Disable player tank model to enable its explosion
        explosion.SetActive(true);  //Yeah
        PlayerRadar.HidePlayerFromRadar(PlayerNumber, true);    //Hide this player icon from enemy radar

        cameraAnim.enabled = true;  //Enable camera animator to play camera animation when exploded
        cameraAnim.Play(explodeAnimName,-1,0);  //Play the camera animation, we have no animation layers ("-1" parameter), from the start (time = 0)
        yield return deathScreenDelay;  //Wait for this time in animation before enabling death screen
        deathScreen.SetActive(true);    //Enable death screen
        
        //TODO make tank explode into pieces

        for (int i = explodedTime; i > 0; i--)  //Count down the death timer
        {
            deathTimer.text = i.ToString(); //i is the current timer value
            yield return secondOfDeath;     //Wait a second between changing timer values
        }

        cameraAnim.enabled = false;     //Camera animation returns to its initial position after some animation time, so to make sure it had the time to return, disable animator only after the death timer
        
        explosion.SetActive(false);     //Explosion quasi-animation (particle system) goes on for the whole time of death (tank is smoking), that's why disable it at the very end
        transform.position = FindRandomPosition();  //Find random position on the map by checking the cylinder where tank can fall from "the sky" to the ground without anything interrupting
        transform.rotation = Quaternion.Euler(0, Random.Range(0,4) * 90, 0);    //Set tank rotation to random between 0,90,180,270 degrees (perpendicular to walls)

        movement.enabled = true;        //Enable player ability to move
        playerRigidbody.drag = 0;       //Disable rigidbody's drag we used for stopping the player after exploding

        Health = 100;
        healthSlider.value = Health;  //Set full health and update the slider

        tank.gameObject.SetActive(true);      //Enable tank model
        deathScreen.SetActive(false);   //Disable death screen

        yield return endOfFrame; //Wait until the end of frame, before revealing the enemy on the map. This is due to Update updating the radar running before the couroutine yields, so the frame of player being at the explosion position would slip through
        PlayerRadar.HidePlayerFromRadar(PlayerNumber, false);    //Reveal this player icon from enemy radar
    }
    
    private void IncreaseDestroyedTime()    //Gets launched on 5 and 10 death in a game
    {
        explodedTime += 1;  //Incread death timer by 1 second

        ParticleSystem.MainModule moduleSmoke = particleSmoke.main;         //Get particle main module to set duration and lifetime
        ParticleSystem.MainModule moduleExplosion = particleExplosion.main;
        
        moduleSmoke.duration = explodedTime - 0.5f; //Smoke ends 0.5 sec before quasi-animation ends
        moduleExplosion.duration = explodedTime;

        moduleSmoke.startLifetimeMultiplier = explodedTime;     //This multiplies the particle lifetime curve by the death timer
        moduleExplosion.startLifetimeMultiplier = explodedTime;
    }
    
    private readonly WaitForSeconds tankFlashTime = new WaitForSeconds(0.05f); //Tank flashes for this time when taking damage

    private IEnumerator flashTank()     //Flash tank on hit
    {        
        if (DOTween.IsTweening(material) == false)      //If the tank material is already flashing from picking up the ball, don't flash it from being hit
        {
            const float fin = 0.35f;                
            Color final = new Color(fin, fin, fin); //Flash the tank to this color
            material.SetColor(emissionColor, final);
            yield return tankFlashTime;             //Wait
            material.SetColor(emissionColor, Color.black);   //Set the color back
        }       
    }

    private Vector3 FindRandomPosition()    //Algorithm for finding random position on the map for player to spawn after death
    {
        //The algorithm is checking the cylinder from the ground to the highest point of the arena where there is some object in the random X-Z point 
        //of the arena if this cylinder has something but the floor in it, if it has, then find another point where there is only a floor in the cylinder
        //We use OverlapCapsuleNonAlloc for this, which requires the Collider[] array to store found colliders in this cylinder being checked
        
        int offset = 4;             //Offset from the arena border so player doesn't spawn right next to a wall
        float levelDimension = GameController.Controller.ArenaDimension / 2;  //Get arena dimension (total X=Y length) from GameController, to convert it into max coordinate need to divide by 2

        int iter = 0;   //A way to stop the infinite loop of finding the random spawn point if we can't find it

        while (true)    //There is the infinite loop
        {
            Array.Clear(spawnCheckColliders, 0, spawnCheckColliders.Length);    //Clear the array of colliders just in case before performing the next check 

            Vector2 coord = new Vector2(Random.Range(-levelDimension + offset, levelDimension - offset), Random.Range(-levelDimension + offset, levelDimension - offset)); //Get random point of the map with RNG
            //Capsule starts from Y=0 (floor) to 10 (highest point where arena objects are) TODO increase this when made the highest arena
            Physics.OverlapCapsuleNonAlloc(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 10, coord.y), 2, spawnCheckColliders, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < 2; i++) //The spawnCheckColliders array has only the length of 2, because the condition when there is only the floor found is when its the first collider found and the second collider is null
            {                           //If the first collider isn't the floor, or second collider isn't the floor as well, then its the wrong condition
                if (spawnCheckColliders[0].gameObject.layer == 14 && spawnCheckColliders[1] == null)
                {
                    return new Vector3(coord.x, 5, coord.y);    //Return position to spawn tank at. Y=5 is the height from where the tank drops
                }               
            }
            iter++;
            if (iter > 100)
            {
                Debug.LogError("Couldn't find the spawn site"); //If we performed 100 checks and haven't found a spawn site, there must be something wrong
                return new Vector3(0, 20, 0);
            }

                
        }
      
    }
    
    public void Possession()        //Function that is getting run from Ball.cs when picking up the ball
    {
        possession = true;          //Set the bool so fumbles can occur and so the player can shoot the ball
        ShowMessage(Message.Possession);    //Show "POSSESSION" message on screen for this player
        
        PlayerRadar.ballPossession = true;      //Set the static variable in the Radar so we know to not calculate ball icon on the radar if someone possesses the ball
        PlayerRadar.HideBallFromRadars(true);   //Hide ball from radars if someone picks up the ball

        DOTween.Kill(PlayerNumber); //The next line after it slides up the "ball-container" so in case of player getting the ball instantly after losing it, kill the existing slide-down animation to start a new one
        ballUI.DOAnchorPosY(175, (175 - ballUI.anchoredPosition.y) / 630).SetId(PlayerNumber);  //Slide up the "ball-container" to Y=175 position of the UI over time dependion on its current position (full uninterrupted slide animation takes 0.5 sec)
        ballCamera.SetActive(true);     //Enable rotating ball on the UI
        //============Flashing Tank on Ball Pickup===========        
        float init = 0.1f;
        Color initial = new Color(init, init, init);    //"Low" flash color
        material.SetColor(emissionColor, initial);

        float fin = 0.35f;
        Color final = new Color(fin, fin, fin);         //"High" flash color

        material.DOColor(final, emissionColor, 0.05f).SetLoops(6, LoopType.Yoyo).OnComplete(() => { material.SetColor(emissionColor, Color.black); });       //Flash 3 times (6 times back and fourth) and set the color back in the end
        //====================================================

        if (GameController.Controller.ShotClock != 0)   //If in the game settings shot clock is not set to 0, then show the shot clock on screen (otherwise when the period is ending the period countdown will be always shown)
        {            
            if (GameController.Controller.PeriodEnding == false)    //If the period is not ending (so its more than 20 seconds on the clock)
            {               
                ballClock.SetActive(true);  //Enable ShotClock UI element
            }
            else    //Or if the period is ending
            {
                if (GameController.Controller.ShotClock < GameController.Controller.GameTime)   //If shotclock is less than remaining period time (means shot clock will manage to fully run out before period ends)
                {                    
                    ballClock.SetActive(true);          //Show shot clock and disable end period timer
                    endPeriodTimer.SetActive(false);    
                }
                //else the period timer will be enabled from other functions anyway
            }
            
            StartCoroutine(nameof(ballClockTimer)); //Start the shot clock countdown
        }
        else
        {
            pickupTime = Time.time; //If we there is no shot clock, remember the time moment when the ball got picked up
        }
        
    }

    public void PeriodEnding()  //Function that gets launched from GameController when <20 seconds of period
    {
        //If there is no shot clock, show period timer, OR if there IS shot clock, but current player doesn't have a ball, OR if player has a ball, but shot clock is bigger than remaining period time (means period will end sooner than shot clock run out)
        if (GameController.Controller.ShotClock == 0 || (GameController.Controller.ShotClock != 0 && possession == false) || (possession == true && GameController.Controller.ShotClock > GameController.Controller.GameTime))
        {
            endPeriodTimer.SetActive(true); //Show period countdown
            ballClock.SetActive(false);     //And hide ball clock if it was active
        }
    }

    private IEnumerator ballClockTimer()    //Coroutine counting down the shot clock timer
    {
        for (int i = GameController.Controller.ShotClock; i >= 0; i--)  //Count down from the set time in game settings
        {
            ballClockText.text = i.ToString("D2"); //i is the current timer value, D2 is the format of the number: 00, 01, 02... 
            yield return secondOfDeath;     //Wait a second between changing timer values
            playerStats.PossessionTime++;   //Count every second into the PossessionTime of this player
        }
        LoseBall(FumbleCause.Violation);    //If the countown manages to go all the way down, lose ball due to violation. It is written just after the timer, because we StopCoroutine every time countdown is interrupted
    }
    
    void Update()
    {
        if (Time.timeScale == 0) return;    //COMM

        if (InputManager.GetButtonDown("Pause", PlayerNumber))
        {
            //GameController.Controller.Pause();  //COMM
            GameController.Controller.PauseMenu(PlayerNumber);
        }
        
        if (possession && InputManager.GetButtonDown(ballButtonName, PlayerNumber)) //If player has a ball and presses Shoot Ball button
        {
            LoseBall(FumbleCause.Shot); //"LOSE" ball due to shooting it
        }


    }

    public void Hit(float firePower, Weapon weapon)       //Gets invoked from Rocket or Laser OnCollisionEnter
    {
        float damage = (160 - tank.Armor) / 250 * firePower;    //Bullshit formula, but that's the closest I could decypher actual game's damage system

        Health -= damage;   //Decrease health

        if (Health < 0)    //If goes below 0, set it to 0, set the slider to it
        {
            Health = 0;
            healthSlider.value = Health;    //Set slider value

            playerStats.Deaths++;    //Increment player's death to change the death timer and for end-game stats
            if (playerStats.Deaths == 5 || playerStats.Deaths == 10) IncreaseDestroyedTime();   //On 5 deaths the death timer is 3 sec, on 10 deaths it is 4 sec
            StartCoroutine(Explode());  //Start explosion sequence           

            if (PlayerNumber == PlayerID.One)
            {
                GameController.Controller.PlayerTwo.ShowMessage(Message.EnemyDestroyed);    //Show "ENEMY DESTROYED" message for the opposite player
            }
            else if (PlayerNumber == PlayerID.Two)
            {
                GameController.Controller.PlayerOne.ShowMessage(Message.EnemyDestroyed);
            }
            GameController.announcer.Kill();        //Kill line from announcer

            if (possession) LoseBall(FumbleCause.Death);    //If player had a ball, fumble it           
        }
        else            //If the player didn't die from damage
        {
            StartCoroutine(flashTank());    //Flash tank from taking damage
            healthSlider.value = Health;    //If we didn't explode the player, just change slider value to his health

            if (weapon == Weapon.Rocket) camera.DOShakePosition(0.5f, 0.15f, 20, 90, false).OnComplete(() => {camera.transform.localPosition = new Vector3(0, 0.8f, 0); }); //Shake the camera and return it back in the end
            
            //TODO Maybe make some part deattach from a tank

            if (possession && weapon == Weapon.Rocket)  //If player had a ball and got hit by a rocket
            {
                float chance = (firePower - tank.Armor * 0.2f) * 0.01f;  //The chance of fumble is "FirePowerOfShootingTank - ArmorOfReceivingTank/5"
                float random = Random.value;                        //Random value (0,1)
                if (random < chance) LoseBall(FumbleCause.Fumble);    //Calculate a chance of fumble and fumble the ball with the chance
            }
             
        }
    }
    
    private enum FumbleCause { Shot, Fumble, Death, Violation }     //Enum that is a parameter for the LoseBall function

    private void LoseBall(FumbleCause cause)    //Function to lose the ball
    {
        ballCamera.SetActive(false);    //Disable the UI that shows the ball rotating (this is because the ball rotating would show on both player's UIs otherwise)
        DOTween.Kill(PlayerNumber);     //Next line from this one animates the "ball-container" to slide down, so in the case of player losing the ball instantly after picking it up, kill the existing animation and run new one
        ballUI.DOAnchorPosY(-140, (ballUI.anchoredPosition.y + 140) / 630).SetId(PlayerNumber); //Animate "ball-container" to slide down to '-140' position with the speed taking into account its current position (so the max distance of sliding would take 0.5 seconds)

        ball.transform.position = transform.TransformPoint(new Vector3(0, 0.75f, 0));    //Teleport the ball from rotating under the map to the center of the player (the anchor of the player is on the very bottom of the tank, so raise it a bit to Y=0.5) (Look Ball.cs to see about this teleportation from under the map)
        ball.rigidbody.useGravity = true;    //Make the ball affected by gravity (when under the map we disable gravity) 
        ball.rigidbody.angularDrag = ball.BallAngularDrag;  //Set the angular drag back to the ball
        
        possession = false;                 //Set that the player doesn't have a ball anymore

        PlayerRadar.ballPossession = false;     //Start calculating position of ball icon on the UI now that no one possesses the ball
        PlayerRadar.HideBallFromRadars(false);  //Reveal the ball on the radar

        if (GameController.Controller.ShotClock != 0)   //If there is shot clock
        {
            if (GameController.Controller.PeriodEnding) //If the period is ending, show period timer (it gets disabled if shot clock get shown)
            {
                endPeriodTimer.SetActive(true);
            }           
            ballClock.SetActive(false);             //Disable shot clock timer  //OPTIMIZE enabling and disabling the UI Image generates garbage for some reason (look MaskableObject.SetActive, or smth)
            StopCoroutine(nameof(ballClockTimer));  //Stop the countdown
        }
        else
        {
            playerStats.PossessionTime += Time.time - pickupTime; //If there is no shot clock, then we count PossessionTime by subtracting the time when the ball got fumbled from the time the ball got picked up
        }   //Basically, when there is shot clock, we count the possession every second with the shot timer, but when there is none, we count it with time intervals between picking up the ball and losing it
        

        //I would go for "switch-case" here, but it's absolutely unreadable for me, also there is not much performance difference for 4 items
        if (cause == FumbleCause.Shot)      //If the player shot the ball
        {
            if (PlayerNumber == PlayerID.One) //Set in the ball which player shot the ball
            {
                ball.firstPlayerShot = true;
                ball.firstPlayerPossessedShot = true;
            }
            else if (PlayerNumber == PlayerID.Two)
            {
                ball.secondPlayerShot = true;
                ball.secondPlayerPossessedShot = true;
            }
            ball.PlayerShot(PlayerNumber);      //Run a function on a ball to check when the ball missed the goal

            pickup.Play();      //Play "pickup" sound when player shoots the ball

            playerStats.ShotsOnGoal++;  //Increment the amount of ShotsOnGoal for the end-stats
            
            ball.transform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward));   //Set the rotation of the ball facing the direction of the tank
            ball.rigidbody.angularVelocity = transform.TransformDirection(new Vector3(20, 0, 0));               //Set angular velocity of the ball rotating to the direction of the shot

            //Setting velocity instead of applying force to be able to store that velocity at the same frame (With force it would get applied the next frame), also to be able to add the player speed to inherit it 
            ball.rigidbody.velocity = transform.TransformDirection(Vector3.forward * ballShootForce / ball.rigidbody.mass) + playerRigidbody.velocity;   //Dividing only ballShootForce by mass, so the player velocity get inherited with bigger value
            if (ball.rigidbody.velocity.y < 0) ball.rigidbody.velocity = new Vector3(ball.rigidbody.velocity.x, 0, ball.rigidbody.velocity.z);  //Make it so that vertical velocity gets inherited only when player moves up, but not down (for higher ability of performing cool shots)
            
            ball.prevVel = ball.rigidbody.velocity; //Store the velocity of the ball, so when the player is right in front of the goal, we actually have some "previous" value to give to the ball 
            //(gets overrided if some FixedUpdate frame gets snagged in the process of the ball flying from the player to the goal (look FixedUpdate in Ball.cs)

            float distance = Vector3.Distance(playerRigidbody.position, GameController.Controller.goal.ballCollider.position);  //Calculate the distance from the goal at the ball shot point
            if (distance > 30)
                GameController.announcer.ShotLong();        //Announcer says different line depending on how far from the goal the ball was shot
            else
                GameController.announcer.ShotShort();

        }
        else if (cause == FumbleCause.Fumble)   //If the other player fumbled the ball with rocket
        {
            fumbleBall();   //Function to get the random direction in the horizontal plane to throw the ball to
            GameController.announcer.Fumble();        //"Fumble" line from announcer
            ShowMessage(Message.Fumble);              //"Fumble" on screen for player
        }
        else if (cause == FumbleCause.Death)    //If the player died with the ball
        {
            fumbleBall();   //Just throw the ball to a random direction            
        }
        else if (cause == FumbleCause.Violation)    //If the timer counted all the way down - shot clock violation
        {
            fumbleBall();   //Throw the ball to random directoin
            GameController.announcer.Violation(); 
            ShowMessage(Message.Violation);         //Announcer says "Shot clock violation" and "Violation" + "Fumble" on screen
            ShowMessage(Message.Fumble);
        }
        
    }
    
    private void fumbleBall()       //Function that gets called when fumbled with any mean but player actually shootin the ball himself
    {
        playerStats.Fumbles++;          //We are counting fumbles for end-stats from normal fumbles, violations and deaths with ball

        float fumbleBallforce = 100;    //TODO Adjust the value maybe     
        Vector2 randDirCircle = Random.insideUnitCircle.normalized;    //Get random direction in the cirle lying in the horizontal plane of the player
        Vector3 ballDirection = new Vector3(randDirCircle.x, 0, randDirCircle.y);   //Transform Vector2 to Vector3       
        ball.transform.rotation = Quaternion.LookRotation(ballDirection);   //Set ball rotation to this random direction
        //Since the ball flies to random direction, no need to set the angular velocity, let it just be free since its all random anyway
        ball.rigidbody.AddForce(ballDirection * fumbleBallforce / ball.rigidbody.mass, ForceMode.VelocityChange);    //Add force to the generated direction dependant on ball mass
    }

    private readonly WaitForSeconds scoreUITime = new WaitForSeconds(5);    //During this time the score shows on screen after scoring
   
    public void Score()         //That function gets invoked for both players from Ball.cs when someone scores
    { 
        StartCoroutine(ScoreUI());
    }

    private IEnumerator ScoreUI()       //Show UI element showing score for 5 seconds
    {
        scoreText.text = playerStats.Score.ToString();  //Score is getting counted to playerStats, so get it from there and set the text on the UI
        scoreImage.SetActive(true);                     //Enable showing the UI element
        yield return scoreUITime;                       //Wait 5 seconds
        scoreImage.SetActive(false);                    //Disable the UI
    }


    private readonly WaitForSeconds messageDelay = new WaitForSeconds(3);   //Messages show for 3 seconds on screen

    public void ShowMessage(string message) //Public function getting launched from everywhere showing messages on screen for the player
    {
        StartCoroutine(MessageQueue(message));
    }

    private IEnumerator MessageQueue(string message)    
    {
        messageBox.text += message + Environment.NewLine;   //Each message adds the message to existing written messages and adds the line (so the next possible message will by displayed at the next line)
        yield return messageDelay;                          //Then we wait 3 seconds before disabling the message
        messageBox.text = messageBox.text.Remove(0, messageBox.text.IndexOf(Environment.NewLine, StringComparison.Ordinal) + 2);    //Removes the first line written in the message box
        //Since all the messages have exactly 3 second delay, the first line will ALWAYS be the one that was actually written by this function
    }
    
    public void SetPlayer()    //Function that is getting called after injecting players when loading the scene, also when you right click "PlayerNumber" field in the inspector and select "Set Player"
    {        
        Tank tankObject = GetComponentInChildren<Tank>();   //Try get child Tank component of that player
        if (tankObject == null)                             //If tank component wasn't assigned, tell error
        {
            Debug.LogError("Where is the tank?");
            return;
        }
        Camera playerCamera = GetComponentInChildren<Camera>(); //Get camera in the children to set it's viewport and culling mask
        RectTransform playerHUD = playerCamera.transform.Find("Player HUD Canvas/PlayerPanel").GetComponent<RectTransform>();   //Get PlayerPanel UI RectTransform to set it

        if (PlayerNumber == PlayerID.One)   //If the value selected for PlayerNumber field is "One"
        {
            gameObject.name = "PlayerOne";  //Set player object name 
            gameObject.layer = 8;           //Set layer of the "root" object to respective player
            
            SetLayerRecursively(tankObject.gameObject, 8);  //Set the whole hierarchy of the tank object to respective player layer 
            //(setting layers only for tank because only they have colliders that consider layers, and only tank has solid objects and particles to exclude from camera culling mask)

            playerCamera.rect = new Rect(0, 0, 0.5f, 1);    //Set camera rect
            playerCamera.cullingMask = ~(1 << 8);           //Set camera culling mask (so the player can't see his own tank and its particles)
            
            playerHUD.anchorMin = new Vector2(0,0);         //Set player UI RectTransform
            playerHUD.anchorMax = new Vector2(0.5f, 1);

            GameObject.Find("GameController").GetComponent<GameController>().PlayerOne = GetComponent<Player>();    //Set player variable in the GameController       
        }          
        else if (PlayerNumber == PlayerID.Two)  //All the same stuff for player two
        {
            gameObject.name = "PlayerTwo";
            gameObject.layer = 9;
            
            SetLayerRecursively(tankObject.gameObject, 9);
 
            playerCamera.rect = new Rect(0.5f, 0, 0.5f, 1);
            playerCamera.cullingMask = ~(1 << 9);

            playerHUD.anchorMin = new Vector2(0.5f, 0);
            playerHUD.anchorMax = new Vector2(1, 1);

            GameObject.Find("GameController").GetComponent<GameController>().PlayerTwo = GetComponent<Player>();
        }
   
    }

    void SetLayerRecursively(GameObject obj, int newLayer)  //Function that sets the whole hierarchy of the object to specified layer
    {
        if (obj == null)    //We recursively set layers for every object we find in the hierarchy, so the recursion will stop if there are no objects in the hierarchy
        {
            return;
        }

        obj.layer = newLayer;   //Set the actual object to the layer

        foreach (Transform child in obj.transform)  //And look all children of this object
        {
            if (child == null)      //Idk why this honestly, it's what I copied. Foreach shouldn't just return null results from what I understand
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);    //Launch the same layer setting for each child and for their hierarchy if they have it
        }
    }

    

    public void SetEverythingBack()
    {
        playerRigidbody.velocity = Vector3.zero;    //Zero player's velocity

        gameObject.SetActive(false);        //Simple way to stop all coroutines
        gameObject.SetActive(true);

        //=============EXPLODING============= Everything here is concequence of player exploding just before period ends     
        
        camera.transform.localPosition = new Vector3(0, 0.8f, 0);   //Set camera position to it's original position
        camera.transform.localRotation = Quaternion.identity;       //Rotation as well
        cameraAnim.enabled = false;     //Disable animator (with exploding camera animation)

        explosion.SetActive(false);     //Disable explosion 
        
        movement.enabled = true;        //Enable player ability to move if it was disabled due to explosion
        playerRigidbody.drag = 0;       //Disable rigidbody's drag we used for stopping the player after exploding

        Health = 100;
        healthSlider.value = Health;  //Set full health and update the slider

        tank.gameObject.SetActive(true);      //Enable tank model
        deathScreen.SetActive(false);   //Disable death screen

        PlayerRadar.HidePlayerFromRadar(PlayerNumber, false);    //Reveal this player icon from enemy radar
        //====================EXPLODE================

        //=================POSSESSION============   This all is a consequence of picking up a ball
        DOTween.CompleteAll();      //Complete all tweens (for current stage, there are none we need to still be going)
        ballCamera.SetActive(false);    //Disable ball
        ballUI.anchoredPosition = new Vector2(ballUI.anchoredPosition.x, -140);  //Set ballUI thingy back
        ball.rigidbody.useGravity = true;    //Make the ball affected by gravity (when under the map we disable gravity) 
        ball.rigidbody.angularDrag = ball.BallAngularDrag;  //Set the angular drag back to the ball
        possession = false;                     //Set so we don't possess
        PlayerRadar.ballPossession = false;     //Start calculating position of ball icon on the UI now that no one possesses the ball
        PlayerRadar.HideBallFromRadars(false);  //Reveal the ball on the radar
        ballClock.SetActive(false);             //Disable shot clock timer  
        endPeriodTimer.SetActive(false);        //Disable end period timer
        StopCoroutine(nameof(ballClockTimer));  //Stop the countdown

        //=================POSSESSION============


        material.SetColor(emissionColor, Color.black);  //Set the tank color back if it was flashing
        scoreImage.SetActive(false);                    //Hide showing scores on screen if they were showing
        messageBox.text = string.Empty;                 //Clear message box
        
    }



}
