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

    [Header("Explosion")]                   //All the stuff about explosion
    [SerializeField] private ParticleSystem particleSmoke;    //Reference to two particle systems which lengths depend on the death timer, we change their length from those references
    [SerializeField] private ParticleSystem particleExplosion;

    [SerializeField] private GameObject explosion;  //Explosion particle system object parented to player object to enable/disable it when exploding
    [SerializeField] private GameObject tankModel;  //Tank model object to enable/disable it when exploding
    [SerializeField] private GameObject deathScreen;    //Reference to UI image covering the screen when dead to enable/disable it when exploding
    [SerializeField] private Text deathTimer;           //Reference to death timer text to count it down when dead
    [SerializeField] private Animator cameraAnim;       //Reference to camera animator to play the animation when exploded

    private int explodedTime = 2;                       //Default time being dead (increases by 1 sec up to 4 sec every 5 deaths)
    private readonly WaitForSeconds secondOfDeath = new WaitForSeconds(1);  //One second of death (we wait for it in the coroutine, then decrease the timer time, then wait again, and so on)
    private  readonly  WaitForSeconds deathScreenDelay = new WaitForSeconds(0.5f);  //Time during which play camera animation before entering death screen

    //=================BALL==============
    private Ball ball;
    private Rigidbody ballRigidbody;

    [SerializeField] private RectTransform ballUI;


    //====================OTHER=====================

    //Reference for being able to smoothly stop the exploding tank with setting its drag (we disable PlayerMovement script when dead, so the player can't move dead ship...
    private Rigidbody playerRigidbody;      //And in PlayerMovement custom drag when alive is implemented, so we apply the internal rigidbody drag when dead
    private PlayerMovement movement;        //Reference to disable it when dead

    private readonly WaitForSeconds tankFlashTime = new WaitForSeconds(0.05f); //Tank flashes for this time when taking damage

    private Collider[] spawnCheckColliders = new Collider[2];   //Premade array of colliders to not have any garbage allocated when checking where to spawn a tank after death with "CheckCapsule"

    private Material material;

    private string ballButtonName = "ShootBall";

    //=============================================

    void Awake()
    {
        ball = GameController.Controller.ball;
        ballRigidbody = ball.GetComponent<Rigidbody>();

        material = GetComponentInChildren<Tank>().GetComponent<Renderer>().material;
        playerRigidbody = GetComponent<Rigidbody>();    //Getting those references
        movement = GetComponent<PlayerMovement>();
    }

    private IEnumerator Explode()   //Process of exploding the player
    {       
        movement.enabled = false;   //Disable players ability to move with buttons
        playerRigidbody.drag = 3;   //Apply drag so the player object stops over some time when exploding, instead of instantly stopping 
        tankModel.SetActive(false); //Disable player tank model to enable its explosion
        explosion.SetActive(true);  //Yeah

        cameraAnim.enabled = true;  //Enable camera animator to play camera animation when exploded
        cameraAnim.Play("ExplodeCamera",-1,0);  //Play the camera animation, we have no animation layers, from the start (time = 0)
        yield return deathScreenDelay;  //Wait for this time in animation before enabling death screen
        deathScreen.SetActive(true);    //Enable death screen
        
        //TODO make it explode into pieces

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
        material.EnableKeyword("_EMISSION");    //Enable Emission property of the material (it has the flash color set)
        yield return tankFlashTime;             //Wait
        material.DisableKeyword("_EMISSION");   //Disable Emission property //TEST lets hope it actually saves into prefab D:
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

    public void Hit(float damage)       //Gets invoked from Rocket's OnCollisionEnter //TODO and somewhere on the laser it will be
    {
        Health -= damage;   //Decrease health

        if (Health < 0)    //If goes below 0, set it to 0, set the slider to it
        {
            Health = 0;
            healthSlider.value = Health;

            playerStats.Deaths++;    //Increment player's death to change the death timer and for end-game stats
            if (playerStats.Deaths == 5 || playerStats.Deaths == 10) IncreaseDestroyedTime();   //On 5 deaths the death timer is 3 sec, on 10 deaths it is 4 sec
            StartCoroutine(Explode());  //Start explosion sequence           
        }
        else
        {
            StartCoroutine(flashTank());    //Flash tank from taking damage
            healthSlider.value = Health;    //If we didn't explode the player, just change slider value to his health
        }
    }

    private bool possession = false;

    public void Possession()
    {
        possession = true;

        DOTween.Kill(PlayerNumber);
        ballUI.DOAnchorPosY(175, (175 - ballUI.anchoredPosition.y) / 630).SetId(PlayerNumber);

    }

    private float ballForce = 5000;

    void Update()
    {
        
        if (possession && InputManager.GetButtonDown(ballButtonName, PlayerNumber))
        {
            GameController.announcer.ShotLong();

            DOTween.Kill(PlayerNumber);
            ballUI.DOAnchorPosY(-140, (ballUI.anchoredPosition.y + 140) / 630).SetId(PlayerNumber);

            ball.transform.position = transform.TransformPoint(new Vector3(0,0.5f,2.1f));
            //ball.gameObject.SetActive(true);
            ballRigidbody.useGravity = true;
            ballRigidbody.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward));
            ballRigidbody.angularVelocity = transform.TransformDirection(new Vector3(20, 0, 0));
            ballRigidbody.AddForce(transform.TransformDirection(Vector3.forward * ballForce));
            possession = false;

            //TODO Inherit players speed
        }

        


    }
}
