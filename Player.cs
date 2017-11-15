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
    ==================GLOBAL CONCEPTS========================
    EVERYTHING PLAYER-PLAYER RELATED HAS TO REVOLVE AROUND WHICH PLAYER NUMBER IS SET UP TO EACH ONE ON TOP OF HIERARCHY IN THIS SCRIPT

    Ideally i should make ONE SINGLE "PlayerX" prefab with everything setup, and then when instantiating, just set its parameters for different players.
    ==========================================================

    Next thing to do: come up with damage system with firepower, armor which affects mass. Work on rocket push forces in general
    Maybe turn the rocket spawns inside a bit so the rockets shoot not fully forward
    Same with lasers
    
    Bake lighting for every arena
    





    Consider that we have default Physic Material and there needs to be ball bouncing out of everything     
    

    Adjust thrusters lifetime so you don't see it while moving backwards. MAYBE MAKE CULLING SHIT TO CAMERA, SO WITH INSANE FOVS PLAYERS COULDNT SEE THEIR OWN TANK
         
    When putting a chosen tank in a scene's container "PlayerOne/Two", set the layer of the tank object to respective one. Care about "PlayerExplosion" layer






    ADDITIONAL IDEAS:
    When hit, slider slowly going down with red trail like in LOL
    Ballhandling on a tank changes the speed of the ball the tank shoots it
    Different balls having different mass, so you shoot them slower and they feel heavier (from Drag property of rigidbody)
    Showing text messages on screen should be done with “ShowMessage(text)”, while the actual function will handle all the stuff about making it next line and pushing existing lines.
    When picked powerup, there should be a tooltip with circular “slider” going representing the time left for it.






*/



[System.Serializable]
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
    //public float GameTime; //TODO probably will be in GameController or something
}

public class Player : MonoBehaviour
{
    public PlayerID PlayerNumber; //From TeamUtility InputManager "Add-on". Sets which player it is

    public PlayerStats playerStats; //Instance of PlayerStats so we could fill them with all the stats

    [Header("Health")]          //All the stuff about health
    private float Health = 100;  //Tank Health variable, changes when taking damage
    [SerializeField] private Slider healthSlider;   //Reference to slider to change its value depending on the player health
    [SerializeField] private new Camera camera;     //Reference to player's camera to shake it on rocket hit

    [Header("Explosion")]                   //All the stuff about explosion
    [SerializeField] private ParticleSystem particleSmoke;    //Reference to two particle systems which lengths depend on the death timer, we change their length from those references
    [SerializeField] private ParticleSystem particleExplosion;

    [SerializeField] private GameObject explosion;  //Explosion particle system object parented to player object to enable/disable it when exploding
    [SerializeField] private GameObject tankModel;  //Tank model object to enable/disable it when exploding
    [SerializeField] private GameObject deathScreen;    //Reference to UI image covering the screen when dead to enable/disable it when exploding
    [SerializeField] private Text deathTimer;           //Reference to death timer text to count it down when dead
    [SerializeField] private Animator cameraAnim;       //Reference to camera animator to play the animation when exploded  //TODO get is from Camera camera

    private int explodedTime = 2;                       //Default time being dead (increases by 1 sec up to 4 sec every 5 deaths)
    private readonly WaitForSeconds secondOfDeath = new WaitForSeconds(1);  //One second of death (we wait for it in the coroutine, then decrease the timer time, then wait again, and so on), used in the BallClock timer
    private  readonly  WaitForSeconds deathScreenDelay = new WaitForSeconds(0.5f);  //Time during which play camera animation before entering death screen

    [Header("Ball")]            //All the stuff about the ball
    private Ball ball;          //Caching the reference to the ball from GameController   

    private bool possession = false;        //Bool representing if the player has a ball (used when fumbling and when shooting the ball)
    public float ballShootForce = 100;    //TODO Maybe dependant on some tank parameter like BallHandling

    [SerializeField] private RectTransform ballUI;      //Reference to the "ball-container" UI element that raises from below when player picks up a ball
    [SerializeField] private GameObject ballCamera;     //Reference to the RawImage object containing RenderTexture of the camera looking at the ball to disable it when the player loses the ball

    [SerializeField] private GameObject ballClock;      //Reference to Shot Clock UI Image to disable/enable it when have or don't have the ball
    [SerializeField] private Text ballClockText;        //Reference to text timer on the Shot Clock UI
    

    //====================OTHER=====================

    //Reference for being able to smoothly stop the exploding tank with setting its drag (we disable PlayerMovement script when dead, so the player can't move dead ship...
    private Rigidbody playerRigidbody;      //And in PlayerMovement custom drag when alive is implemented, so we apply the internal rigidbody drag when dead
    private PlayerMovement movement;        //Reference to disable it when dead
    private Tank tank;                  //Reference to Tank component of attached tank to get its characteristics

    private readonly WaitForSeconds tankFlashTime = new WaitForSeconds(0.05f); //Tank flashes for this time when taking damage

    private Collider[] spawnCheckColliders = new Collider[2];   //Premade array of colliders to not have any garbage allocated when checking where to spawn a tank after death with "CheckCapsule"

    private Material material;          //Tank material to modify it when the tank gets hit or picks up the ball

    private string ballButtonName = "ShootBall";    //Caching button name for shooting the ball

    //=============================================

    void Awake()
    {
        ball = GameController.Controller.ball;
        
        tank = GetComponentInChildren<Tank>();              //Getting those references
        material = tank.GetComponent<Renderer>().material;

        playerRigidbody = GetComponent<Rigidbody>();    
        movement = GetComponent<PlayerMovement>();       
    }

    private IEnumerator Explode()   //Process of exploding the player
    {       
        movement.enabled = false;   //Disable players ability to move with buttons
        playerRigidbody.drag = 3;   //Apply drag so the player object stops over some time when exploding, instead of instantly stopping 
        tankModel.SetActive(false); //Disable player tank model to enable its explosion
        explosion.SetActive(true);  //Yeah

        cameraAnim.enabled = true;  //Enable camera animator to play camera animation when exploded
        cameraAnim.Play("ExplodeCamera",-1,0);  //Play the camera animation, we have no animation layers ("-1" parameter), from the start (time = 0)
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

        tankModel.SetActive(true);      //Enable tank model
        deathScreen.SetActive(false);   //Disable death screen
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
    
    private IEnumerator flashTank() 
    {
        //material.EnableKeyword("_EMISSION");    //Enable Emission property of the material (it has the flash color set)
        if (DOTween.IsTweening(material) == false)
        {
            float fin = 0.35f;
            Color final = new Color(fin, fin, fin);
            material.SetColor("_EmissionColor", final);
            yield return tankFlashTime;             //Wait
            material.SetColor("_EmissionColor", Color.black);


        }
        

        //material.DisableKeyword("_EMISSION");   //Disable Emission property //TEST lets hope it actually saves into prefab D:
    }

    private Vector3 FindRandomPosition()    //Algorithm for finding random position on the map for player to spawn after death
    {
        //The algorithm is checking the cylinder from the ground to the highest point of the arena where there is some object in the random X-Z point 
        //of the arena if this cylinder has something but the floor in it, if it has, then find another point where there is only a floor in the cylinder
        //We use OverlapCapsuleNonAlloc for this, which requires the Collider[] array to store found colliders in this cylinder being checked
        
        int offset = 4;             //Offset from the arena border so player doesn't spawn right next to a wall
        float levelDimension = 30;  //TODO this will depend on the arene size

        int iter = 0;   //A way to stop the infinite loop of finding the random spawn point if we can't find it

        while (true)    //There is the infinite loop
        {
            Array.Clear(spawnCheckColliders, 0, spawnCheckColliders.Length);    //Clear the array of colliders just in case before performing the next check 

            Vector2 coord = new Vector2(Random.Range(-levelDimension + offset, levelDimension - offset), Random.Range(-levelDimension + offset, levelDimension - offset)); //Get random point of the map with RNG
            Physics.OverlapCapsuleNonAlloc(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 10, coord.y), 2, spawnCheckColliders);    //Capsule starts from Y=0 (floor) to 10 (highest point where arena objects are) TODO increase this when made the highest arena
            for (int i = 0; i < 2; i++) //The spawnCheckColliders array has only the length of 2, because the condition when there is only the floor found is when its the first collider found and the second collider is null
            {                           //If the first collider isn't the floor, or second collider isn't the floor as well, then its the wrong condition
                if (spawnCheckColliders[0].gameObject.layer == 14 && spawnCheckColliders[1] == null)
                {
                    return new Vector3(coord.x, 5, coord.y);    //Return position to spawn tank at. Y=5 is the height from where the tank drops
                }
                

            }
            iter++;
            if (iter > 100) Debug.LogError("Couldn't find the spawn site"); //If we performed 100 checks and haven't found a spawn site, there must be something wrong
        }
      
    }
    
    public void Possession()        //Function that is getting run in the Ball.cs when picking up the ball
    {
        possession = true;          //Set the bool so fumbles can occur and so the player can shoot the ball

        DOTween.Kill(PlayerNumber); //The next line after it slides up the "ball-container" so in case of player getting the ball instantly after losing it, kill the existing slide-down animation to start a new one
        ballUI.DOAnchorPosY(175, (175 - ballUI.anchoredPosition.y) / 630).SetId(PlayerNumber);  //Slide up the "ball-container" to Y=175 position of the UI over time dependion on its current position (full uninterrupted slide animation takes 0.5 sec)
        ballCamera.SetActive(true);     //Enable rotating ball on the UI

        //============

        //material.EnableKeyword("_EMISSION");    

        float init = 0.1f;
        Color initial = new Color(init, init, init);
        material.SetColor("_EmissionColor", initial);

        float fin = 0.35f;
        Color final = new Color(fin, fin, fin);

        material.DOColor(final, "_EmissionColor", 0.05f).SetLoops(6, LoopType.Yoyo).OnComplete(() => { material.SetColor("_EmissionColor", Color.black); });

        //material.DisableKeyword("_EMISSION");
        //material.SetColor("_EmissionColor", 1);



        //============

        if (GameController.Controller.ShotClock != 0)   //If in the game settings shot clock is not set to 0, then show the shot clock on screen
        {
            ballClock.SetActive(true);  //Enable UI element
            StartCoroutine(nameof(ballClockTimer)); //Start the countdown
        }

    }
    
    private IEnumerator ballClockTimer()    //Coroutine counting down the shot clock timer
    {
        for (int i = GameController.Controller.ShotClock; i >= 0; i--)  //Count down from the set time in game settings
        {
            ballClockText.text = i.ToString("D2"); //i is the current timer value, D2 is the format of the number: 00, 01, 02... //OPTIMIZE Kerning script generates garbage every second timer changes
            yield return secondOfDeath;     //Wait a second between changing timer values
        }
        LoseBall(FumbleCause.Violation);    //If the countown manages to go all the way down, lose ball due to violation. It is written just after the timer, because we StopCoroutine every time countdown is interrupted
    }
    
    void Update()
    {
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
            healthSlider.value = Health;

            playerStats.Deaths++;    //Increment player's death to change the death timer and for end-game stats
            if (playerStats.Deaths == 5 || playerStats.Deaths == 10) IncreaseDestroyedTime();   //On 5 deaths the death timer is 3 sec, on 10 deaths it is 4 sec
            StartCoroutine(Explode());  //Start explosion sequence           

            GameController.announcer.Kill();        //TODO

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
                float chance = (firePower - tank.Armor / 5) / 100;  //The chance of fumble is "FirePowerOfShootingTank - ArmorOfReceivingTank/5"
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

        ball.transform.position = transform.TransformPoint(new Vector3(0, 0.5f, 0));    //Teleport the ball from rotating under the map to the center of the player (the anchor of the player is on the very bottom of the tank, so raise it a bit to Y=0.5) (Look Ball.cs to see about this teleportation from under the map)
        ball.rigidbody.useGravity = true;    //Make the ball affected by gravity (when under the map we disable it) 
        ball.rigidbody.angularDrag = ball.BallAngularDrag;  //Set the angular drag back to the ball
        
        possession = false;                 //Set that the player doesn't have a ball anymore
        if (GameController.Controller.ShotClock != 0)   //If the ShotClock is not 0 (not shot clock violations, gets set in game settings)
        {
            ballClock.SetActive(false);             //Disable shot clock timer  //OPTIMIZE enabling and disabling the UI Image generates garbage for some reason (look MaskableObject.SetActive, or smth)
            StopCoroutine(nameof(ballClockTimer));  //Stop the countdown
        }

        //I would go for "switch-case" here, but it's absolutely unreadable for me, also there is not much performance difference for 4 items
        if (cause == FumbleCause.Shot)      //If the player shot the ball
        {
            if (PlayerNumber == PlayerID.One)   //Set in the ball which player shot the ball
                ball.firstPlayerShot = true;
            else if (PlayerNumber == PlayerID.Two)
                ball.secondPlayerShot = true;

            playerStats.ShotsOnGoal++;  //Increment the amount of ShotsOnGoal for the end-stats
            
            ball.transform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward));   //Set the rotation of the ball facing the direction of the tank
            ball.rigidbody.angularVelocity = transform.TransformDirection(new Vector3(20, 0, 0));               //Set angular velocity of the ball rotating to the direction of the shot
            ball.rigidbody.AddForce(transform.TransformDirection(Vector3.forward * ballShootForce), ForceMode.Impulse); //Add the force in the direction

            GameController.announcer.ShotLong();        //TODO different length from goal

            //TODO Inherit players speed

        }
        else if (cause == FumbleCause.Fumble)   //If the other player fumbled the ball with rocket
        {
            fumbleBall();   //Function to get the random direction in the horizontal plane to throw the ball to

            GameController.announcer.Fumble();        //TODO

            //TODO Count fumbles
            //CHECK if the fumble amount includes deaths with ball and violations
        }
        else if (cause == FumbleCause.Death)    //If the player died with the ball
        {
            fumbleBall();   //Just throw the ball to a random direction
        }
        else if (cause == FumbleCause.Violation)    //If the timer counted all the way down - shot clock violation
        {
            fumbleBall();   //Throw the ball to random directoin
            GameController.announcer.Violation(); //TODO
        }
        
    }

    

    private void fumbleBall()       //Function that gets called when fumbled with any mean but player actually shootin the ball himself
    {
        float fumbleBallforce = 100;    //TODO Adjust the value

        Vector2 randDirCircle = Random.insideUnitCircle;    //Get random direction in the cirle lying in the horizontal plane of the player
        Vector3 ballDirection = new Vector3(randDirCircle.x, 0, randDirCircle.y);   //Transform Vector2 to Vector3
        ball.transform.rotation = Quaternion.LookRotation(ballDirection);   //Set ball rotation to this random direction
        //Since the ball flies to random direction, no need to set the angular velocity, let it just be free since its all random anyway
        ball.rigidbody.AddForce(ballDirection * fumbleBallforce, ForceMode.Impulse);    //Add force to the generated direction
        
    }





    

    
}
