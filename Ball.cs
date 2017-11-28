using System.Collections;
using DG.Tweening;
using TeamUtility.IO;
using UnityEditor;
using UnityEngine;

public class Ball : MonoBehaviour
{

    [HideInInspector] public new Rigidbody rigidbody;    //Caching ball rigidbody to modify it when someone picks it up    
    [HideInInspector] public float BallAngularDrag;         //Get initial ball drag and angular drag so we can set them back when disabling those on ball pickup, we enable drag when getting the ball into arena (shot, fumble)

    private Goal goal;          //To cache shorter reference to goal
    private Transform goalTransform;    //Reference to goal transform (the child object of actual Goal object), position from it gets taken every frame when the ball is flying towards the goal being shot by the player
    private Collider ballTrigger;   //To disable ball trigger collider for 5 seconds after scoring (so it can't get picked up)
    private Material ballMaterial;  //To be able to make the ball transparent during 5 second delay after scoring
    private float originalAlpha;    //Different balls have different original alpha, we set it to the fraction of original when ball scores
    
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();  
        rigidbody.maxAngularVelocity = 50;      //We don't want the ball to have limited angular velocity of 7, we want it to roll FAST, as fast as it can get rolling  
        BallAngularDrag = rigidbody.angularDrag;    //Store the ball's default angular drag so we can set it back when it gets into arena

        goal = GameController.Controller.goal;
        goalTransform = goal.ballCollider;        //The child object of goal is the one that is actually moving     
        ballTrigger = GetComponent<Collider>();                                                                     //Getting dose references
        ballMaterial = GetComponentInChildren<Renderer>().material;
        originalAlpha = ballMaterial.color.a;   //And some parameters as well
    }
    
    //TODO [HideInInspector]    and maybe move this to "playerOne.possessed", "playerOne.ballShot"? This should simplify a lot of code here. But only do it after we are sure everything works withot bugs or errors
    public bool firstPlayerShot;        //This represents if and which player shot the ball for scoring and interceptions (only applies until the ball hits some geometry)
    public bool secondPlayerShot;

    //We shoot the ball from the middle of the tank (inside of it) (we can't shoot it from in front of the player, cuz it will then go inside terrain if the player is right next to it)
    //So for the player to not get the ball right back after shooting it (because OnTriggerStay raises from the ball being inside of the tank), we don't process the triggering of the ball
    //with the player that just shot the ball (until the ball hits some obstacle or after 2 seconds, as usual). So the next bools identify which player just shot the ball (or got fumbled from) 
     
    public bool firstPlayerPossessed;  //If and which player possessed the ball when something fumbled it (only applies until the ball hits some geometry, or 2 seconds)
    public bool secondPlayerPossessed;

    public bool firstPlayerPossessedShot;   //Those are for the player to be able to pick the ball back after 2 seconds since shooting it (when the ball ended up moving super slow)
    public bool secondPlayerPossessedShot;  //And also those ones will prevent the player to refresh the ballClock against some wall (basically, you can't pick up the ball for 2 seconds after shooting it) (you can do so if rejected tho)
    //TODO make private

    private readonly WaitForSeconds pickupAfterShotDelay = new WaitForSeconds(2);   //Delay after shooting when the shooting player can't pick up the ball

    public void PlayerShot(PlayerID player)     //Public function that gets called from Player.cs when player shoots the ball
    {
        if (player == PlayerID.One) StartCoroutine(nameof(pickupDelayOne));     //Depending on which player shot, start respective coroutine of 2 seconds delay until this player can pick the ball up again
        else if (player == PlayerID.Two) StartCoroutine(nameof(pickupDelayTwo));

        StartCoroutine(nameof(BallShot));   //Also run the coroutine checking when the ball flew past the goal to register a miss
    }
    
    private IEnumerator BallShot()  //Coroutine registering a miss at the moment ball flies past the goal if the player missed it
    {                               //(In the original game it would register only if the ball hit some geometry after shooting it doe)
        while (true)    //Until we break out of coroutine (when the ball flies past the goal)
        {
            Vector3 ballToGoal = goalTransform.position - rigidbody.position;   //This gets the vector pointing from the ball to the goal 
            if (Vector3.Angle(rigidbody.velocity, ballToGoal) > 90)             //If the angle between preceding vector and ball velocity is more than 90 deg, 
            {                                                                   //it means that the ball is flying away from the goal (player can't possibly score anymore with this ball flight path)
                float distance = Vector3.Distance(rigidbody.position, goalTransform.position);  //Calculate the distance to the goal from the ball
                if (distance > 1.5f)    //Register a miss only if the ball is farther than 1.5 units away from goal to consider the situation when the ball hits the farthest point of the goal from the player (when its already past goal's center)
                {                    
                    Miss(distance);     //Run a function deciding if it was a "Close Shot" or "Way Off"
                    yield break;        //Stop coroutine from running, cuz the ball already flew past the goal
                }           
            }
            yield return null;          //If the angle is less than 90, the ball is still moving towards the goal, check it at the next frame
        }                               //Even if the ball never reaches the goal like this, other player, or same player after 2 seconds can pick up the ball 
    }

    private void Miss(float distance)   //Function that checks if it was a "Close Shot" or "Way Off" from the distance argument
    {
        if (distance > 5) GameController.announcer.MissLong();  //If the ball flew past the goal more than 5 units away, its a "Way Off"
        else GameController.announcer.MissClose();

        playerMissMessage();    //Function that writes "Miss" on screen for particular player (acually done as separate function because we use it on "Rejected" as well)

        losePossession(false);                                  //Lose possession from both players, but the shooting player still can't pick up the ball for 2 seconds
    }

    void playerMissMessage()    //Function to write "Miss"
    {
        if (firstPlayerShot)
        {
            GameController.Controller.PlayerOne.ShowMessage(Message.Miss);  //Depending on what the player shot the ball
        }
        else if (secondPlayerShot)
        {
            GameController.Controller.PlayerTwo.ShowMessage(Message.Miss);
        }
    }

    private IEnumerator pickupDelayOne()    //Have to make a coroutines for each player, to be able to stop them (I couldn't find an easier way to stop a coroutine with a parameter, so two non-parameter ones)
    {
        yield return pickupAfterShotDelay;  //Wait 2 seconds
        firstPlayerPossessed = false;       //Set possession flags so the shooting player can again pick up the ball
        firstPlayerPossessedShot = false;                
    }

    private IEnumerator pickupDelayTwo()    //Same for other player
    {
        yield return pickupAfterShotDelay;
        secondPlayerPossessed = false;
        secondPlayerPossessedShot = false;
    }
    
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
        rigidbody.angularDrag = 0;                      //Disable the angular drag so the ball doesn't stop rotating in the UI (it rotates by means of angular velocity)
        
    }

    //The ball has two colliders on it: 
    //Trigger collider which can "collide" only with players without physical collisions
    //Non-trigger collider which can collide with floor, level geometry and score, reflecting from them (not when scoring to the goal tho)
    //We register "trigger" collisions in OnTriggerStay instead of OnTriggerEnter to count the cases when the tank is shooting the ball right into a wall standing right next to it, 
    //where the ball doesn't trigger "Enter" message, because it stays in the bounds of the player, and we need the player to get the ball back after bouncing from the wall
    //And obviously we register physical collisions in OnCollisionEnter
    
    private bool triggering = false;            //Flag to consider the case when two players get into ball's trigger to pick it up at the same frame, so they can't both pick up the ball

    IEnumerator OnTriggerStay(Collider other)  //When the ball "collides" with players (Coroutine version of it so we can know that trigger function is running during OnCollisionXX)
    {        
        if (other.gameObject.layer == 8) //PlayerOne layer. If the ball triggered with the first player
        {
            if (firstPlayerPossessed == false && firstPlayerPossessedShot == false)  //Check this so the ball doesn't get right back to the player when he shoots it (because the ball shoots from inside of the player)
            {
                if (triggering) yield break;    //If the other "triggerage" is running during the same frame, don't execute it (the only thing I can think of this happening is when both players touch the ball at one frame)      
                triggering = true;              //Set to true in the beginning of OnTriggerStay, and set to false in the end of frame, after OnCollisionXX, that way the code in it can't possibly run more than once at the same time
                                                //AND we know, that triggerage is running, when the ball collides with something the same frame
                                                //But when two players DO actually get onto a ball at the same time, "random" player picks it up, just whatever trigger gets to run before the other one

                firstPlayerPossessed = true;    //Set this so when the the ball gets out of player one, the ball triggering with the same player doesn't get processed 
                firstPlayerShot = false;        //When the player got the ball, set so he isn't shooting it now
                secondPlayerPossessed = false;  //In case after fumbling when the ball gets to the other player without touching any collider
                secondPlayerPossessedShot = false;  //In case after shooting the ball, it flies past the goal and hits the other player before ending the 2sec delay

                StopCoroutine(nameof(pickupDelayTwo)); //Stop 2 sec delay coroutine on other player if this player got the ball before it ended (This player coroutine can't possibly be running here, if the player can pick up a ball)
                StopCoroutine(nameof(BallShot));       //In case before missing the ball, other player intercepts the ball
                if (secondPlayerShot)  //If second player shot the ball previsouly (before the ball hit some obstacle, as usual), the first player "INTERCEPTED" the ball
                {
                    GameController.Controller.PlayerOne.playerStats.Interceptions++;    //Increment the amount of interceptions for player one for end-stats
                    GameController.announcer.Interception();    //Announcer line "Interception"
                    secondPlayerShot = false;      //If the ball got intercepted, we have to reset it here (we have to know the player shot before actually setting it up to false)
                }
                else                        //Otherwise player just picked up the ball
                {
                    GameController.announcer.Possession();      //"He's got the bawl"
                }                              
                ballPossess(other);             //Run the stuff related to possession on the ball and on the player

            }

        }
        else if (other.gameObject.layer == 9) //PlayerTwo layer. All the same here as for the first player, but reversed for the second one
        {
            if (secondPlayerPossessed == false && secondPlayerPossessedShot == false)
            {
                if (triggering) yield break;                
                triggering = true;

                secondPlayerPossessed = true;
                secondPlayerShot = false;
                firstPlayerPossessed = false;
                firstPlayerPossessedShot = false;

                StopCoroutine(nameof(pickupDelayOne));
                StopCoroutine(nameof(BallShot));
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
        yield return new WaitForEndOfFrame();   //Run everything trigger-related before OnCollisionXX, but still know that during the same frame some trigger has run, so don't run some related stuff in OnCollisionXX
        triggering = false; //Disable the flag in the end of frame
    }

    private void losePossession(bool full)   //This gets reset when the ball hits the obstacle both when shot the ball or fumbled
    {                                        //The parameter to be able to reset ALL possible possession flags (when someone scores or rejected), or all of them without 2 sec player shot delay (when hit geometry or missed)
        firstPlayerPossessed = false;
        secondPlayerPossessed = false;
        firstPlayerShot = false;
        secondPlayerShot = false;
        StopCoroutine(nameof(BallShot));    //Stop coroutine checking if the ball flew past the goal

        if (full)   //If scored or rejected, stop 2 second delay on both players
        {
            firstPlayerPossessedShot = false;
            secondPlayerPossessedShot = false;
            StopCoroutine(nameof(pickupDelayOne));
            StopCoroutine(nameof(pickupDelayTwo));
        }

    }
    

    //So the goal has "score-registering" parts (or faces of the ball model), and also non-registering (from which the ball bounces off)
    //We check if the ball hit score-registering parts or not by checking contact normals when the ball collides with the goal
    //And when some player actually scores, the ball proceeds to go through the goal. Since Unity can't normally detect a collision before it actually occurs, when OnCollisionEnter occurs, ball has already 
    //reflected from the goal, even if it scored. That way we store the ball velocity before the collision (because FixedUpdate runs before OnCollision), and then after collision with the goal, we return
    //the velocity back to what it was before the collision, along with disabling the collider of the goal for the ball to pass through that goal

    [HideInInspector] public Vector3 prevVel; //Velocity of the ball in the previous frame to be able to "pass" the ball through the collider after it actually collides with it

    public Vector3 additionalGravity;       //To make the ball itself and different balls have different gravity

    void FixedUpdate()
    {
        //print(rigidbody.velocity.magnitude);

        if (PlayerRadar.ballPossession == false) rigidbody.AddForce(additionalGravity, ForceMode.Acceleration);    //Constantly applying additional gravity if the ball is in the arena (not possessed)

        if (firstPlayerShot || secondPlayerShot)    //We need to remember the ball previous velocity when some player actually shoots the ball         
        { 
            prevVel = rigidbody.velocity;
        }
    }

    private readonly WaitForSeconds scoreDelay = new WaitForSeconds(5); //5 second delay after scoring during which players can't pick up the ball

    private IEnumerator BallScore()     //Coroutine of disabling-enabling colliders to introduce 5 sec delay after scoring
    {
        goal.ballSolidCollider.enabled = false;    //Disable goal collider for the ball (goal collider for the player still works)
        ballTrigger.enabled = false;    //Disable ball trigger to players can't pick up the ball
        if (additionalGravity.y == -20) ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, originalAlpha * 0.03f);    //Hack to get BurningBall alpha to very small amount instead of 0.4
        else ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, originalAlpha * 0.4f);     //Make ball half-transparent
        
        rigidbody.velocity = prevVel;   //Set the ball velocity back to what it was before colliding (so the ball goes through the goal and doesn't bounce from it)
        
        yield return scoreDelay;    //Wait 5 seconds

        ballMaterial.DOFade(originalAlpha, additionalGravity.y == -20 ? 1f : 0.2f); //Cool-ass way to use ternary operator in a function parameter. 
        //So thats a slight hack: only BurningBall has -20 gravity and when the ball IS BurningBall, fade it during one second instead of 0.2 as for other balls (because of how particles are done for this ball)

        ballTrigger.enabled = true;    //Enable ball trigger
        goal.ballSolidCollider.enabled = true;    //Enable goal ball collider        
    }
    
    void OnCollisionEnter(Collision other)  //When the ball collides with the floor, level geometry or goal
    {
        if (firstPlayerShot || secondPlayerShot)  //If either of the players shot the ball before
        {
            if (other.gameObject.layer == 13)   //LevelGeometry layer. If hit level geometry after shooting
            {                
                float distance = Vector3.Distance(rigidbody.position, goalTransform.position);  //Calculate the distance from the goal
                Miss(distance);     //To decide if it was "Near Miss" or "Way Off"                
            }
            else if (other.gameObject.layer == 19) //GoalBallSolid layer. If player hit the score
            {                
                if (goal.goalType == Goal.GoalType.FourSided)   //4-sided goal scores on any collision with the ball
                {
                    Score();        //Run a function to count the score to the player and flash the goal
                }
                else if (goal.goalType == Goal.GoalType.TwoSided)
                {
                    Vector3 normal = other.contacts[0].normal;  //Normal to the contact point of the goal
                    Vector3 goalFacing = goal.ballCollider.TransformDirection(Vector3.forward);    //The direction where the goal is facing 

                    //Z-axis of the goal always points from one of the goal-scoring parts of the goal
                    if (normal == goalFacing || normal == -goalFacing)      //For 2-sided goal, if we hit front or back side of it
                    {
                        Score();        
                    }
                    else    //If we hit some other part - REJECTED
                    {
                        GameController.announcer.Rejected();   //Let announcer say "Rejected"
                        playerMissMessage();                    //Write miss depending on what player shot the ball
                    }

                }
                else if (goal.goalType == Goal.GoalType.OneSided)
                {
                    Vector3 normal = other.contacts[0].normal;  //Normal to the contact point of the goal
                    Vector3 goalFacing = goal.ballCollider.TransformDirection(Vector3.forward);    //The direction where the goal is facing 
                   
                    if (normal == goalFacing)   //If we hit 1-sided goal exclusively from the front
                    {
                        Score();
                    }
                    else
                    {
                        GameController.announcer.Rejected();    //Same
                        playerMissMessage();                         
                    }

                }


                losePossession(true);   //After hitting the goal, whether scored or not, reset ALL possession flags
            }    
        }
        else if (firstPlayerPossessed || secondPlayerPossessed) //If the player had the ball, but didn't shoot it (got fumbled), reset the bools to no one possessing the ball when the ball collides with anything 
        {                                                       //(including the floor, the position and velocity of the ball on fumble are set so the ball can't touch the ground being still inside of the player)
            if (triggering) return;     //If during this frame OnTrigger has run, meaning player picked up or intercepted the ball, then don't reset the possession flags

            losePossession(false);   
        }
        
    }

    private void Score()    //Function that runs when someone scores
    {
        GameController.announcer.Score();       //Let announcer say "Score"
        goal.FlashGoalOnScore();                //Run a public flash function on the goal
        
        if (firstPlayerShot)                    //Add the score to respective player
        {
            GameController.Controller.PlayerOne.playerStats.Score++;    //Increment the score for the player that did it
            GameController.Controller.PlayerOne.ShowMessage(Message.Score); //Show message on screen for this player
        }
        else if (secondPlayerShot)
        {
            GameController.Controller.PlayerTwo.playerStats.Score++;
            GameController.Controller.PlayerTwo.ShowMessage(Message.Score); 
        }
        GameController.Controller.PlayerOne.Score();    //Run Score functions on both players to show the UI of them
        GameController.Controller.PlayerTwo.Score();
        GameController.Controller.scoreBoard.UpdateScore(); //Update the score on the scoreboard

        StartCoroutine(BallScore());        //Coroutine for disabling-enabling ball and score colliders in 5 sec delay after scoring

    }

    
}
