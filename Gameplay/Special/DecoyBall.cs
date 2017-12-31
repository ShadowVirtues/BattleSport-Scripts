using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DecoyBall : MonoBehaviour
{
    [HideInInspector] public new Rigidbody rigidbody;   //Caching, cuz we apply additional gravity and attract to players
    private Material ballMaterial;          //Material to fade when ball gets scored
    public Vector3Int additionalGravity;    //Additional gravity to copy from legit ball

    private Collider ballTrigger;   //To disable ball trigger collider for 5 seconds after scoring (so it can't get picked up)
    private float originalAlpha;    //Different balls have different original alpha, we set it to the fraction of original when ball scores

    void Awake()
    {
        ballMaterial = GetComponentInChildren<Renderer>().material;  //Getting dose references 
        rigidbody = GetComponent<Rigidbody>();
        ballTrigger = GetComponent<Collider>();   
        originalAlpha = ballMaterial.color.a;       //And parameters
    }
  
    public void Score() //Launch this for all balls from DecoyBallController when scored
    {
        StartCoroutine(BallScore());    
    }

    private IEnumerator BallScore()     //Coroutine of disabling-enabling colliders to introduce 5 sec delay after scoring
    {       
        ballTrigger.enabled = false;    //Disable ball trigger so players can't pick up the ball
        if (additionalGravity.y == -20) ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, originalAlpha * 0.03f);    //Hack to get BurningBall alpha to very small amount instead of 0.4
        else ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, originalAlpha * 0.4f);     //Make ball half-transparent
       
        yield return Ball.scoreDelay;    //Wait static from Ball.cs 5 seconds

        ballMaterial.DOFade(originalAlpha, additionalGravity.y == -20 ? 1f : 0.2f); //Cool-ass way to use ternary operator in a function parameter. 
        //So thats a slight hack: only BurningBall has -20 gravity and when the ball IS BurningBall, fade it during one second instead of 0.2 as for other balls (because of how particles are done for this ball)

        ballTrigger.enabled = true;    //Enable ball trigger        
    }

    void OnTriggerEnter(Collider other) //When players get into ball trigger
    {
        if (other.gameObject.layer == 8 || other.gameObject.layer == 9) //If one of the players entered
        {            
            if (other.gameObject.layer == 8)    //Depending on which player entered, play their own pickup sound
            {
                GameController.Controller.PlayerOne.pickup.Play(); 
            }
            else
            {
                GameController.Controller.PlayerTwo.pickup.Play();
            }
            gameObject.SetActive(false);    //Disable the ball

        }

    }

    //========================ATTRACTING STUFF====================

    private Transform playerOne;        //Transforms of players to calculate the attractionVector to attract the ball if they have BallAttractor
    private Transform playerTwo;

    void Start()
    {
        playerOne = GameController.Controller.PlayerOne.transform;  //Getting dose
        playerTwo = GameController.Controller.PlayerTwo.transform;
    }

    public float attractingForce;   //Will copy those from legit ball
    public float attractingAngle;

    void FixedUpdate()
    {
        rigidbody.AddForce(additionalGravity, ForceMode.Acceleration);    //Constantly applying additional gravity (regardless of if it was picked by player or not, cuz we dont teleport the ball under map)

        if (GameController.Controller.PlayerOne.powerup.BallAttractor && GameController.Controller.PlayerOne.Health != 0)   //Attract to the player all the time if they are alive
        {
            AttractBall(playerOne.position, attractingForce, attractingAngle);
        }

        if (GameController.Controller.PlayerTwo.powerup.BallAttractor && GameController.Controller.PlayerTwo.Health != 0)
        {
            AttractBall(playerTwo.position, attractingForce, attractingAngle);
        }
        
    }

    private void AttractBall(Vector3 target, float attractForce, float attractAngle)    //Function to run when attracting to the player or to the goal, target - to attract to
    {
        Vector3 attractVector = (target - transform.position).normalized;   //Calculate a normalizer attraction vector by subtracting
        rigidbody.AddForce(attractVector * attractForce, ForceMode.Force);  //Add force to the ball in the direction of attraction
        //If we only add force, the ball could get the the point it would literally spin around the player, not being able to properly get to him, so we need to redirect the ball to the direction of the target as well
        rigidbody.velocity = Vector3.RotateTowards(rigidbody.velocity, attractVector, attractAngle, 0); //Last parameter is for changing the vector depending on the difference between current and target, which we don't need, so 0
    }

    public void SetEverythingBack() //Launched from DecoyBallController, to get back the collider and material color
    {
        ballMaterial.color = new Color(ballMaterial.color.r, ballMaterial.color.g, ballMaterial.color.b, originalAlpha);
        ballTrigger.enabled = true;
    }
}
