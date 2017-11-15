using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{

    [HideInInspector] public new Rigidbody rigidbody;    //Caching ball rigidbody to modify it when someone picks it up
    //[HideInInspector] public float BallDrag;              //TEST if we need it after implemented some ball with the drag
    [HideInInspector] public float BallAngularDrag;         //Get initial ball drag and angular drag so we can set them back when disabling those on ball pickup

    private Goal goal;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = 50;      //We don't want the ball to have limited angular velocity of 7, we want it to roll FAST, as fast as it can get rolling  
        //BallDrag = rigidbody.drag;
        BallAngularDrag = rigidbody.angularDrag;

        goal = GameController.Controller.goal;
    }

    void Start()
    {
        
    }


    void Update()
    {
        //print(rigidbody.velocity.magnitude);
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

    
    public bool firstPlayerShot;        //This represents if and which player shot the ball (only applies until the ball hits some geometry)
    public bool secondPlayerShot;

    //We shoot the ball from the middle of the tank (inside of it) (we can't shoot it from in front of the player, cuz it will then go inside of terrain if the player is right next to it)
    //So for the player to not get the ball right back after shooting it (because OnTriggerStay raises from the ball being inside of the tank), we don't process the triggering of the ball
    //with the player that just shot the ball (until the ball hits some obstacle, as usual). So the next bools identify which player just shot the ball so the trigger doesn't raise until the ball hits some geometry
    public bool firstPlayerPossessed;  //If and which player possessed the ball when something fumbled it (only applies until the ball hits some geometry)
    public bool secondPlayerPossessed;
    
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
        //rigidbody.drag = 0;                           //DELETE this if not needed
        rigidbody.angularDrag = 0;                      //Disable the angular drag so the ball doesn't stop rotating in the UI    
        
    }

    //The ball has two colliders on it: 
    //Trigger collider which can "collide" only with players and the score-registering parts of the goal without physical collisions
    //Non-trigger collider which can collide with the floor and level geometry, reflecting from them
    //We register "trigger" collisions in OnTriggerStay instead of OnTriggerEnter to cound the cases when the tank is shooting the ball right into a wall standing right next to it, 
    //...where the ball doesn't trigger "Enter" message, because it stays in the bounds of the player, and we need the player to get the ball back after bouncing from the wall
    //And obviously we register physical collisions in OnCollisionEnter

    //TODO Adjust the bounciness of the ball from how it bounces off the floor when the arena starts

    //TEST Some weird bug when player getting the ball doesn't get his property xxxPlayerPossessed = true set 



    //private int a = 0;

    //18 GoalBallSolid
    //19 GoalBallScore
    //20 GoalSolid

    //void OnTriggerEnter(Collider other)
    //{
        
    //    if (other.gameObject.layer == 19)   //GoalScore Layer. If we hit the score-registering parts of the goal
    //    {
            
    //        if (firstPlayerShot)
    //        {
    //            //TODO Score player One
    //        }
    //        else if (secondPlayerShot)
    //        {
    //            a++;
    //            print(a);

    //            //TODO Score player Two
    //        }

    //    }


    //}


    IEnumerator OnTriggerStay(Collider other)  //When the ball "collides" with players or score-registering parts of the goal
    {       
        //if (other.gameObject.layer == 19)   //GoalScore Layer. If we hit the score-registering parts of the goal
        //{
        //    if (firstPlayerShot)
        //    {
        //        //TODO Score player One
        //    }
        //    else if (secondPlayerShot)
        //    {
        //        a++;
        //        print(a);
        //        //TODO Score player Two
        //    }

        //}
        //else 
        yield return new WaitForEndOfFrame();

        if (other.gameObject.layer == 8) //PlayerOne layer. If the ball triggered with the first player
        {
            if (firstPlayerPossessed == false)  //Check this so the ball doesn't get right back to the player when he shoots it (because the ball shoots from inside of the player)
            {
                firstPlayerPossessed = true;    //Set this so when the the ball gets out of player one, the ball triggering with the same player doesn't get processed 
                firstPlayerShot = false;        //When the player got the ball, he can't possibly be the one who just shot it, so set this just in case of some
                secondPlayerPossessed = false;  //In case after fumbling when the ball gets to the other player without touching any collider
                if (secondPlayerShot)  //If second player shot the ball previsouly (before the ball hit some obstacle, as usual), the first player "INTERCEPTED" the ball
                {
                    GameController.Controller.PlayerOne.playerStats.Interceptions++;    //Increment the amount of interceptions for player one for end-stats
                    GameController.announcer.Interception();    //TODO
                    secondPlayerShot = false;      //If the ball got intercepted, we have to reset it here 
                    
                }
                else                        //Otherwise player just picked up the ball
                {
                    GameController.announcer.Possession();      //TODO
                }                              
                ballPossess(other);             //Run the stuff related to possession on the ball and on the player

            }

        }
        else if (other.gameObject.layer == 9) //PlayerTwo layer. All the same here as for the first player, but reversed for the second one
        {
            if (secondPlayerPossessed == false)
            {
                secondPlayerPossessed = true;
                secondPlayerShot = false;
                firstPlayerPossessed = false;
                if (firstPlayerShot)
                {
                    GameController.Controller.PlayerTwo.playerStats.Interceptions++;   
                    GameController.announcer.Interception();
                    firstPlayerShot = false;
                    
                }
                else
                {
                    GameController.announcer.Possession();
                }
                                          
                ballPossess(other);
            }
        }

    }

    private void losePossession()   //This gets reset when the ball hits the obstacle both when shot the ball or fumbled
    {        
        firstPlayerPossessed = false;
        secondPlayerPossessed = false;
        firstPlayerShot = false;
        secondPlayerShot = false;
    }

    void OnCollisionEnter(Collision other)  //When the ball collides with the floor or level geometry
    {

        //if (other.gameObject.layer != 14 && other.gameObject.layer != 13) print(other.gameObject.layer);

        if (firstPlayerShot || secondPlayerShot)  //If either of the players shot the ball before the hit
        {
            if (other.gameObject.layer == 13)   //LevelGeometry layer. If hit level geometry after shooting
            {
                GameController.announcer.MissClose();
                //TODO MISS

                losePossession();   //COMM
            }
            else if (other.gameObject.layer == 19) //GoalballSolid layer
            {
                Vector3 normal = other.contacts[0].normal;
                Vector3 goalFacing = goal.transform.TransformDirection(Vector3.forward);


                if (normal == goalFacing || normal == -goalFacing)
                {
                    print("SCORE");
                    GameController.announcer.Score();

                }
                else
                {
                    GameController.announcer.Rejected();
                    //TODO REJECTED
                }

                losePossession();   //COMM
            }    
        }
        else if (firstPlayerPossessed || secondPlayerPossessed) //If the player had the ball, but didn't shoot it (got fumbled), reset the bools to no one possessing the ball 
        {                                                       //when the ball collides with anything (including the floor, the position and speed of the ball are set so the ball can't touch the ground being still inside of the player)
            losePossession();   //COMM
        }

        //If hit not LevelGeometry or GoalSolid, then it was the floor, then do nothing, just bounce off the ball (handled by physics)



    }

    //void OnTriggerExit(Collider other)
    //{
    //    if (firstPlayerShot == false && firstPlayerPossessed && ballPossessed == false && other.gameObject.layer == 8)
    //    {
    //        losePossession();

    //        print("PLAYER ONE FUMBLE");
    //    }
    //    else if (secondPlayerShot == false && secondPlayerPossessed && ballPossessed == false && other.gameObject.layer == 9)
    //    {
    //        losePossession();
    //        print("PLAYER TWO FUMBLE");
    //    }


    //}





}
