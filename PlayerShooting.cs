using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;




/*
 
    Test rocket-rocket collisions


    Make Z-velocity dependent rocket speed
*/


public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject rocketPrefab;   //Rocket prefab to shoot with
    
    private PlayerMovement player;  //Reference to PlayerMovement component on the player. TODO probably should make some separate script "Player" where include this and other stuff non-related to movement and shooting
    private PlayerID playerNumber;  //Player number for handle mutual rocket collisions
    private Tank tank;              //Reference to Tank component to get rocketPower, firePower and rocketSpawnPoints from
    private Rigidbody playerRigidbody; //Reference to player Rigidbody for getting its Z velocity


    private GameObject[] Rockets;   //Array of pooled rockets
    private Rigidbody[] rocketRigidbody;    //Array of rididbodies of pooled rockets
    private Rocket[] rocket;                //Array of Rocket components of pooled rockets

    private int playerRocketCount;          //Variable to hold rocket count of the player
    private string rocketButtonName;        //Input for rocket shooting is handled here so caching this

    public const float defaultRocketSpeed = 35; //Rocket speed is getting set here, it is getting amplified by player velocity in Z direction

    public int shotRocketCount = 0; //Counter for the amount of shot rockets for after-game stats. Also is getting used in Mathf.Repeat to decide which turret to shoot from

    private readonly Vector3 rocketDirection = new Vector3(0,-0.005f,1);    //Vector to shoot rocket to. It is 'forward' with slight drag down (kinda like gravity). In the game has a big effect when one player is flying, the other one can't hit him from longer range

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
        playerNumber = player.PlayerNumber;
        playerRigidbody = GetComponent<Rigidbody>();
        tank = GetComponentInChildren<Tank>(); //Tank component gets inserted to PlayerX gameObject with the tank model when the tank is picked.
        rocketButtonName = "Rocket";
    }

    void Start()
    {
        GameObject rocketPoolContainer = new GameObject($"Player{playerNumber}Rockets");    //Create a container to child rockets to, so all pooled rockets are not all on top of Hierarchy
        playerRocketCount = tank.rocketCount;
        Rockets = new GameObject[playerRocketCount];
        rocketRigidbody = new Rigidbody[playerRocketCount]; //Initializing arrays
        rocket = new Rocket[playerRocketCount];
        
        for (int i = 0; i < playerRocketCount; i++) //For all rockets available
        {
            Rockets[i] = Instantiate(rocketPrefab, rocketPoolContainer.transform);  //Instantiate a rocket childed to their container

            rocketRigidbody[i] = Rockets[i].GetComponent<Rigidbody>();  //Get references to components
            rocket[i] = Rockets[i].GetComponent<Rocket>();

            rocket[i].FirePower = tank.firePower;   //Assign a firepower to the rocket 

            Rockets[i].layer = playerNumber == PlayerID.One ? 11 : 12;     //Assign a layer to rockets. 11 - "RocketPlayerOne", 12 - "RocketPlayerTwo"     
            rocket[i].otherPlayerLayer = playerNumber == PlayerID.One ? 9 : 8;  //Assign this so rockets know what player to do damage to. 9 - "PlayerTwo", 8 - "PlayerOne"

        }
    }



    Vector3 GetPlayerVelocityZ()
    {
        Vector3 velZ = Vector3.Project(playerRigidbody.velocity, transform.TransformDirection(Vector3.forward));

        print(velZ);

        //Debug.DrawRay(transform.position, );


        return velZ;
    }




    void Update()
    {
        if (InputManager.GetButtonDown(rocketButtonName, playerNumber)) //If rocket-shoot button is pressed for respective player
        {
            for (int i = 0; i < playerRocketCount; i++) //For each pooled rocket
            {
                if (Rockets[i].activeSelf == false) //If we find a deactivated rocket, shoot it
                {              
                    Rockets[i].SetActive(true); //Activating it before setting parameters, cuz setting rigidbody parameters for disabled objects doesn't work

                    int turretNumber = shotRocketCount % tank.rocketSpawnPoints.Length; //Index number of the turret to shoot the rocket from. "%" means remainder of division

                    shotRocketCount++;  //Increasing this after calculating turretNumber, so the first rocket (with count 0) shoots from the first turren, not from the second

                    rocketRigidbody[i].position = tank.rocketSpawnPoints[turretNumber].position;   //Set the position to shoot rocket from assigned turret Transform Position
                    rocketRigidbody[i].rotation = transform.rotation;                               //Set rocket rotation from tank rotation.

                    

                    rocketRigidbody[i].velocity = defaultRocketSpeed * transform.TransformDirection(rocketDirection) + GetPlayerVelocityZ();   //Set rocket velocity. Transform quasi-Vector3.forward to local space relative to the tank. TODO Inherit it from tanks Z velocity

                    print(rocketRigidbody[i].velocity.magnitude);

                    rocketRigidbody[i].angularVelocity = transform.TransformDirection(Vector3.forward) * 20;    //Set angular velocity so the rocket rotates along its local Z axis for cool looking rocket.
                    
                    break;  //If we found disabled rocket, don't look further.
                }
            }

        }
        
    }


}
