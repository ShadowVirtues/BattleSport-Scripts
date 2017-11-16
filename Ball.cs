using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{

    [HideInInspector] public new Rigidbody rigidbody;    //Caching ball rigidbody to modify it when someone picks it up
    //[HideInInspector] public float BallDrag;              //TEST if we need it after implemented some ball with the drag
    [HideInInspector] public float BallAngularDrag;         //Get initial ball drag and angular drag so we can set them back when disabling those on ball pickup, we enable drag when getting the ball into arena (shot, fumble)

    private Goal goal;          //To cache shorter reference to goal
    private Collider ballTrigger;   //To disable ball trigger collider for 5 seconds after scoring (so it can't get picked up)
    private Material ballMaterial;  //To be able to make the ball transparent during 5 second delay after scoring

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();  
        rigidbody.maxAngularVelocity = 50;      //We don't want the ball to have limited angular velocity of 7, we want it to roll FAST, as fast as it can get rolling  
        //BallDrag = rigidbody.drag;                //TEST
        BallAngularDrag = rigidbody.angularDrag;    //Store the ball's default angular drag so we can set it back when it gets into arena

        goal = GameController.Controller.goal;
        ballTrigger = GetComponent<Collider>();     //Getting dose references
        ballMaterial = GetComponentInChildren<Renderer>().material;
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
        //and behind the scenes the ball is teleported below the arena, where there is the camera looking at that ball, 
        //and this camera is getting transfered to the RenderTexture that is attached to the UI where the ball rotates
        //So this ball on the UI is literally the actual ball that rolls over the arena, and we show this actual ball on the UI rotating

        rigidbody.velocity = Vector3.zero;  //Stop the ball from moving
        rigidbody.useGravity = false;       //Make so the ball doesn't fall down from the camera looking at it below the arena
        transform.position = new Vector3(0, -20, 2);    //This is where the camera is looking at the ball
        transform.rotation = Quaternion.identity;       //Make the 0,0,0 rotation of the ball
        rigidbody.angularVelocity = Vector3.up * -12;   //Make the ball rotate, which will be shown on the UI
        //rigidbody.drag = 0;                           //DELETE this if not needed
        rigidbody.angularDrag = 0;                      //Disable the angular drag so the ball doesn't stop rotating in the UI (it rotates by means of angular velocity)
        
    }

    //The ball has two colliders on it: 
    //Trigger collider which can "collide" only with players without physical collisions
    //Non-trigger collider which can collide with floor, level geometry and score, reflecting from them (not when scoring to the goal tho)
    //We register "trigger" collisions in OnTriggerStay instead of OnTriggerEnter to count the cases when the tank is shooting the ball right into a wall standing right next to it, 
    //where the ball doesn't trigger "Enter" message, because it stays in the bounds of the player, and we need the player to get the ball back after bouncing from the wall
    //And obviously we register physical collisions in OnCollisionEnter
    
    private bool triggering = false;            //Flag to consider the case when two players get into ball's trigger to pick it up at the same frame, so they can't both pick up the ball

    IEnumerator OnTriggerStay(Collider other)  //When the ball "collides" with players (Coroutine version of it so we can delay the trigger to run after OnCollisionXX)
    {
        if (triggering) yield break;    //If the other "triggerage" is running during the same frame, don't execute it (the only thing I can think of this happening is when both players touch the ball at one frame)
        triggering = true;              //Set to true in the beginning of OnTriggerStay, and set to false in the end, that way the code in it can't possibly run more than once at the same time
        //But when two players DO actually get onto a ball at the same time, "random" player picks it up, just whetever trigger gets to run before the other one
        
        yield return new WaitForEndOfFrame();   //By default, Unity processes OnTriggerXX before OnCollisionXX, with this line, we run OnTriggerXX in the end of frame, after OnCollision already executed
        //This is done to overcome the situation where the ball collides with some obstacle at the same frame player picks up the ball, so "possession" flags don't get reset due to obstacle collision right after player picks up the ball and sets those flags

        if (ballTrigger.enabled == false)   //Due to us DELAYING the execution of OnTriggerStay till the end of the frame, there can be a situation when the player is trying to throw the ball intro    
        {                                   //the goal being right in front of it, where due to collision with the goal, the possession flags are all getting into 'false', meaning player can pick up the ball again
            triggering = false;             //The same frame trigger collider of the ball gets disabled (for 5 sec after scoring), but the trigger of getting the ball already GOT 'triggered' in the beginning of the frame
            yield break;                    //so it would actually mean that the player WOULD get the ball right back. To overcome it, we just check if the ball trigger is disabled, and when it is, player can't possibly pick up the ball
        }                                   //And also since we break out of the function, set "triggering" to false
        
        if (other.gameObject.layer == 8) //PlayerOne layer. If the ball triggered with the first player
        {
            if (firstPlayerPossessed == false)  //Check this so the ball doesn't get right back to the player when he shoots it (because the ball shoots from inside of the player)
            {
                firstPlayerPossessed = true;    //Set this so when the the ball gets out of player one, the ball triggering with the same player doesn't get processed 
                firstPlayerShot = false;        //When the player got the ball, he can't possibly be the one who just shot it, so set this just in case of some BS situation (hope that it won't be broken anyway :)
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

        triggering = false; //Disable the flag in the end of frame
    }

    private void losePossession()   //This gets reset when the ball hits the obstacle both when shot the ball or fumbled
    {        
        firstPlayerPossessed = false;
        secondPlayerPossessed = false;
        firstPlayerShot = false;
        secondPlayerShot = false;
    }
    

    //So the goal has "score-registering" parts (or faces of the ball model), and also non-registering (from which the ball bounces off)
    //We check if the ball hit score-registering parts or not by checking contact normals when the ball collides with the goal
    //And when some player actually scores, the ball proceeds to go through the goal. Since Unity can't normally detect a collision before it actually occurs, when OnCollisionEnter occurs, ball has already 
    //reflected from the goal, even if it scored. That way we store the ball velocity before the collision (because FixedUpdate runs before OnCollision), and then after collision with the goal, we return
    //the velocity back to what it was before the collision, along with disabling the collider of the goal for the ball to pass through that goal

    public Vector3 prevVel; //Velocity of the ball in the previous frame to be able to "pass" the ball through the collider after it actually collides with it

    void FixedUpdate()  
    {
        if (firstPlayerShot || secondPlayerShot)    //We need to remember the ball previous velocity when some player actually shoots the ball         
        { 
            prevVel = rigidbody.velocity;
        }
    }

    private readonly WaitForSeconds scoreDelay = new WaitForSeconds(5); //5 second delay after scoring during which players can't pick up the ball

    private IEnumerator BallScore()     //Corouting of disabling-enabling colliders to introducy 5 sec delay after scoring
    {
        goal.ballSolidCollider.enabled = false;    //Disable goal collider for the ball (goal collider for the player still works)
        ballTrigger.enabled = false;    //Disable ball trigger to players can't pick up the ball
        ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, 0.5f);     //Make ball half-transparent
        rigidbody.velocity = prevVel;   //Set the ball velocity back to what it was before colliding (so the ball goes through the goal and doesn't bounce from it)
        
        yield return scoreDelay;    //Wait 5 seconds

        ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, 1);    //Disable transparency
        ballTrigger.enabled = true;    //Enable ball trigger
        goal.ballSolidCollider.enabled = true;    //Enable goal ball collider        
    }
    
    void OnCollisionEnter(Collision other)  //When the ball collides with the floor, level geometry or goal
    {
        //if (triggering) return;

        if (firstPlayerShot || secondPlayerShot)  //If either of the players shot the ball before the collision
        {
            if (other.gameObject.layer == 13)   //LevelGeometry layer. If hit level geometry after shooting
            {
                GameController.announcer.MissClose();
                //TODO MISS

                losePossession();   //Set all possession flags to false, so no one now possesses the ball
            }
            else if (other.gameObject.layer == 19) //GoalBallSolid layer. If player hit the score
            {
                //TODO Make other goals

                Vector3 normal = other.contacts[0].normal;  //Normal to the contact point of the goal
                Vector3 goalFacing = goal.transform.TransformDirection(Vector3.forward);    //The direction where the goal is facing 

                //Z-axis of the goal always points from goal-scoring part of the goal (along with all other goal-scoring parts, if present)

                if (normal == goalFacing || normal == -goalFacing)
                {
                    
                    GameController.announcer.Score();
                    goal.FlashGoalOnScore();


                    if (firstPlayerShot)
                    {
                        GameController.Controller.PlayerOne.playerStats.Score++;
                    }
                    else if (secondPlayerShot)
                    {
                        GameController.Controller.PlayerTwo.playerStats.Score++;
                    }
                    GameController.Controller.PlayerOne.Score();
                    GameController.Controller.PlayerTwo.Score();


                    StartCoroutine(BallScore());
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
        {                                                       //when the ball collides with anything (including the floor, the position and velocity of the ball on fumble are set so the ball can't touch the ground being still inside of the player)
            losePossession();   //COMM
        }
        
    }
    

    //Want to revert so the trigger runs before collision, but instead when both trigger and collision happen at the same frame, in collision ask if the trigger is running, and not run the collision if it does
    //Also move "triggering = true" to the moment when the ball gets out of the player, so it doesn't trigger in the process of the ball getting out of the player.

}
