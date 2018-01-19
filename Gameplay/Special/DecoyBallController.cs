using UnityEngine;

public class DecoyBallController : MonoBehaviour
{    
    [SerializeField] private int decoyBallCount;    //Number of decoy balls

    private DecoyBall[] decoyBalls;         //Array of DecoyBall components
    private GameObject[] decoyBallObjects;  //Array of decoy ball GameObjects

    private Collider[] spawnCheckColliders = new Collider[2];   //Usual stuff for CheckCapsule
    
    void Start()
    {
        GameObject decoyGoalContainer = new GameObject("DecoyBalls");   //Make a container to pool decoy balls

        decoyBalls = new DecoyBall[decoyBallCount];         //Initialize arrays
        decoyBallObjects = new GameObject[decoyBallCount];

        for (int i = 0; i < decoyBallCount; i++)    //For the whole decoy ball count
        {
            decoyBallObjects[i] = Instantiate(GameController.Controller.ball.gameObject, decoyGoalContainer.transform); //We are literally taking the legit ball from the arena and duplicating it
            Destroy(decoyBallObjects[i].GetComponent<Ball>());      //Removing the "Ball" script component from it
            decoyBalls[i] = decoyBallObjects[i].AddComponent<DecoyBall>();  //And adding DecoyBall component

            decoyBallObjects[i].transform.position = GameController.FindRandomPosition(10, 1, GameController.Controller.BallSpawn.position.y);     //Find random position to put the ball to with our badass function

            decoyBalls[i].additionalGravity = GameController.Controller.ball.additionalGravity; //Set parameters to decoy ball from the legit ball
            decoyBalls[i].attractingForce = GameController.Controller.ball.attractingForce;
            decoyBalls[i].attractingAngle = GameController.Controller.ball.attractingAngle;
        }

        Messenger.AddListener(GameController.SetEverythingBackMessage, SetEverythingBack);  //Add listener for setting everything back message

    } 
    
    public void Scored()    //Function to launch when the legit ball got scored
    {               
        for (int i = 0; i < decoyBallCount; i++)    //Check all balls
        {
            if (decoyBallObjects[i].activeSelf == false)    //For all disabled balls, we will shoot them out from the goal along with legit ball with some angle
            {
                decoyBallObjects[i].transform.position = GameController.Controller.ball.transform.position;     //Set the position of disabled decoy ball to legit ball position

                float range = 15;   //Max angle from legit ball to which distort the decoy ball velocity vector
                float random = Random.Range(-range, range);     //Genarate a random float in range of the angle to both directions
                
                decoyBalls[i].rigidbody.velocity = Quaternion.AngleAxis(random, Vector3.up) * GameController.Controller.ball.rigidbody.velocity;    //Multiply a angle quaternion by the legit ball velocity to distort the decoy ball velocity vector
                
                decoyBallObjects[i].SetActive(true);    //Set the ball active
            }
            else    //For all enabled balls make them move the same speed as legit ball
            {
                if (decoyBalls[i].rigidbody.velocity == Vector3.zero)   //If the decoy ball was not moving when the legit ball got scored
                {
                    Vector3 movement = Random.onUnitSphere; //Generate a random direction for it with our badass method
                    movement.y = 0;
                    decoyBalls[i].rigidbody.velocity = movement.normalized * GameController.Controller.ball.rigidbody.velocity.magnitude;   //And apply the legit goal velocity magnitude to that normalized movement vector
                }
                else    //If the ball was moving to some extent
                {
                    decoyBalls[i].rigidbody.velocity = decoyBalls[i].rigidbody.velocity.normalized * GameController.Controller.ball.rigidbody.velocity.magnitude;  //Normalize the velocity and multiply it by legit ball velocity magnitude
                }
                
            }

            decoyBalls[i].Score();  //Launch a function on all balls to make them half-transparent for 5 sec like a legit ball
        }
      
    }

    //===========USUAL SETEVERYTHINGBACK STUFF============

    void OnDestroy()
    {
        Messenger.RemoveListener(GameController.SetEverythingBackMessage, SetEverythingBack);
    }

    private void SetEverythingBack()
    {
        for (int i = 0; i < decoyBallCount; i++)
        {
            decoyBallObjects[i].SetActive(false);   //Reenable balls to stop their coroutine and at the same time we need to disable all balls if they got disabled by passing over them
            decoyBallObjects[i].SetActive(true);
            decoyBallObjects[i].transform.position = GameController.FindRandomPosition(10, 1, GameController.Controller.BallSpawn.position.y); //Randomly position them on the height of the legit ball
            decoyBalls[i].rigidbody.velocity = Vector3.zero;    //Zero their velocity

            decoyBalls[i].SetEverythingBack();      //Launch function on all balls to get their material and trigger back
        }

    }
    
}
