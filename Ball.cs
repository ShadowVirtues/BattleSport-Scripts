using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Ball : MonoBehaviour
{

    private new Rigidbody rigidbody;    //Caching ball rigidbody to modify it when someone picks it up


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

    }

    void Start()
    {
        rigidbody.maxAngularVelocity = 50;      //We don't want the ball to have limited angular velocity of 7, we want it to roll FAST, as fast as it can get rolling  
    }


    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Joystick1Button2))
        //{

        //    rigidbody.rotation = Quaternion.LookRotation(GameController.Controller.playerTwo.transform.TransformDirection(Vector3.forward));
        //    rigidbody.angularVelocity = GameController.Controller.playerTwo.transform.TransformDirection(new Vector3(20, 0, 0));

        //}


        //Assembly.GetAssembly(typeof(SceneView)).GetType("UnityEditor.LogEntries").GetMethod("Clear").Invoke(new object(), null);

        //if (firstPlayerPossessed) print("FIRST PLAYER POSSESSED");
        //if (secondPlayerPossessed) print("SECOND PLAYER POSSESSED");
        //if (firstPlayerPossessed == false && secondPlayerPossessed == false) print("NO ONE POSSESSED");

        

    }

    
    private bool firstPlayerPossessed;  //This represents if and which player possessed the ball when he shot it (this only applies until the ball hits some geometry)
    private bool secondPlayerPossessed;

    private void ballPossess(Collider other)    //This runs when player picks up the ball
    {
        other.gameObject.GetComponentInParent<Player>().Possession();   //Run a public function on the player that picked up the ball (it has everything to possession that is related to the player)

        //And all the ball-related stuff to possession is handeled here 
        //So when the player picks up the ball, a thing on UI pops up with rotating ball, showing that the player possesses the ball
        //So when the player picks up the ball, behind the scenes the ball is teleported below the arena, where there is the camera looking at that ball, 
        //...and this camera is getting transfered to the RenderTexture that is attached to the UI where the ball rotates
        //So this ball on the UI is literally the actual ball that rolls over the arena, and we show this actual ball on the UI rotating

        rigidbody.velocity = Vector3.zero;  //Stop the ball from moving
        rigidbody.useGravity = false;       //Make so the ball doesn't fall down from the camera looking at it below the arena
        transform.position = new Vector3(0, -20, 2);    //This is where the camera is looking at the ball
        transform.rotation = Quaternion.identity;       //Make the 0,0,0 rotation of the ball
        rigidbody.angularVelocity = Vector3.up * -12;   //Make the ball rotate, which will be shown on the UI
        
    }

    //The ball has two colliders on it: 
    //Trigger collider which can "collide" only with players and the score-registering parts of the goal without physical collisions
    //Non-trigger collider which can collide with the floor and level geometry, reflecting from them
    //We register "trigger" collisions in OnTriggerStay instead of OnTriggerEnter to cound the cases when the tank is shooting the ball right into a wall standing right next to it, 
    //...where the ball doesn't trigger "Enter" message, because it stays in the bounds of the player, and we need the player to get the ball back after bouncing from the wall
    //And obviously we register physical collisions in OnCollisionEnter

    //TODO Adjust the bounciness of the ball from how it bounces off the floor when the arena starts
    //TODO Make player flash when picked up the ball
    //TODO Count the amount of interceptions for the stats

    void OnTriggerStay(Collider other)  //When the ball "collides" with players or score-registering parts of the goal
    {       
        if (other.gameObject.layer == 19)   //GoalScore Layer. If we hit the score-registering parts of the goal
        {
            if (firstPlayerPossessed)
            {
                //TODO Score player One
            }
            else if (secondPlayerPossessed)
            {
                //TODO Score player Two
            }

        }
        else if (other.gameObject.layer == 8) //PlayerOne layer. If the ball triggered with the first player
        {
            if (!firstPlayerPossessed)  //We shoot the ball from the middle of the tank (inside of it) (we can't shoot it from in front of the player, cuz it will then go inside of terrain if the player is right next to it)
            {                           //So for the player to not get the ball right back after shooting it (because OnTriggerStay raises from the ball being inside of the tank), we don't process the triggering 
                                        //...with the player that just shot the ball (until the ball hits some obstacle, as usual) 
                if (secondPlayerPossessed)  //If second player possessed the ball previsouly (before the ball hit some obstacle, as usual), the first player "INTERCEPTED" the ball
                {
                    GameController.announcer.Interception();    //TODO
                }
                else                        //Otherwise player just picked up the ball
                {
                    GameController.announcer.Possession();      //TODO
                }               
                firstPlayerPossessed = true;    //Set this so when the player shoots the ball, we know that the first player possessed it
                secondPlayerPossessed = false;      //If the ball got intercepted, we have to set it here that it's not the second player that possessed the ball
                ballPossess(other);             //Run the stuff related to possession on the ball and on the player

            }

        }
        else if (other.gameObject.layer == 9) //PlayerTwo layer. All the same here as for the first player, but reversed for the second one
        {
            if (!secondPlayerPossessed)
            {
                if (firstPlayerPossessed)
                {
                    GameController.announcer.Interception();
                }
                else
                {
                    GameController.announcer.Possession();
                }
                secondPlayerPossessed = true;
                firstPlayerPossessed = false;               
                ballPossess(other);
            }
        }

    }

    private void losePossession()   //COMM after fumble implementation
    {        
        firstPlayerPossessed = false;
        secondPlayerPossessed = false;
    }

    void OnCollisionEnter(Collision other)  //When the ball collides with the floor or level geometry
    {
        if (firstPlayerPossessed || secondPlayerPossessed)  //If either of the players possessed the ball before the hit
        {
            if (other.gameObject.layer == 13)   //LevelGeometry layer. If hit level geometry after shooting
            {
                GameController.announcer.MissClose();
                //TODO MISS

                losePossession();   //COMM
            }
            else if (other.gameObject.layer == 18) //GoalSolid layer. If we hit the solid part of the goal (non-score-registering)
            {
                GameController.announcer.Rejected();
                //TODO REJECTED

                losePossession();   //COMM
            }    
        }
        //If hit not LevelGeometry or GoalSolid, then it was the floor, then do nothing, just bounce off the ball (handled by physics)



    }




}
