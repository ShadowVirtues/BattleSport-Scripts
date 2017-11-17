using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;




public enum Weapon { Rocket, Laser }


public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private GameObject rocketPrefab;   //Rocket prefab to shoot with
    [SerializeField] private GameObject laserPrefab;   //Laser prefab to shoot with

    private Player player;  //Reference to Player component on the player
    private PlayerID playerNumber;  //Player number for handle mutual rocket collisions
    private Tank tank;              //Reference to Tank component to get rocketPower, firePower and rocketSpawnPoints from
    private Rigidbody playerRigidbody; //Reference to player Rigidbody for getting its Z velocity


    private GameObject[] Rockets;   //Array of pooled rockets
    private Rigidbody[] rocketRigidbody;    //Array of rididbodies of pooled rockets
    private Rocket[] rocket;                //Array of Rocket components of pooled rockets

    private GameObject[] Lasers;   //Array of pooled lasers
    private Rigidbody[] laserRigidbody;    //Array of rididbodies of pooled lasers
    private Laser[] laser;                //Array of Laser components of pooled lasers


    private float laserFireRate = 0.25f;    //TODO TurboLazers
    private int laserCount;
    private int playerRocketCount;          //Variable to hold rocket count of the player
    private string rocketButtonName = "Rocket";        //Input for rocket shooting is handled here so caching this
    private string laserButtonName = "Laser";         //Input for laser shooting

    private AudioSource tankSoundSource;    //TODO
    
    public const float defaultRocketSpeed = 35; //Rocket speed is getting set here, it is getting amplified by player velocity in Z direction (it is used in Rocket.cs to calculate the rocket flight vector)
    private const float defaultLaserSpeed = 60; //Same for lasers, except lasers don't push other players, so private

    private readonly Vector3 rocketDirection = new Vector3(0,-0.005f,1);    //TEST with flight. Vector to shoot rocket to. It is 'forward' with slight drag down (kinda like gravity). In the game has a big effect when one player is flying, the other one can't hit him from longer range

    void Awake()
    {
        player = GetComponent<Player>();
        playerNumber = player.PlayerNumber;
        playerRigidbody = GetComponent<Rigidbody>();
        tank = GetComponentInChildren<Tank>(); //Tank component gets inserted to PlayerX gameObject with the tank model when the tank is picked.
        //rocketButtonName = "Rocket";
        //laserButtonName = "Laser";

        tankSoundSource = GetComponent<AudioSource>();  //TODO
    }

    void Start()
    {
        GameObject weaponPoolContainer = new GameObject($"Player{playerNumber}Weapons");    //Create a container to child rockets to, so all pooled rockets are not all on top of Hierarchy
        int layerToSet = playerNumber == PlayerID.One ? 10 : 11;    //Hardcoded for now which player will be able to hit which player. 10 - "WeaponPlayerOne", 11 - "WeaponPlayerTwo"

        //=======================ROCKETS=========================

        playerRocketCount = tank.RocketCount;
        Rockets = new GameObject[playerRocketCount];
        rocketRigidbody = new Rigidbody[playerRocketCount]; //Initializing arrays
        rocket = new Rocket[playerRocketCount];
        
        for (int i = 0; i < playerRocketCount; i++) //For all rockets available
        {
            Rockets[i] = Instantiate(rocketPrefab, weaponPoolContainer.transform);  //Instantiate a rocket childed to their container

            rocketRigidbody[i] = Rockets[i].GetComponent<Rigidbody>();  //Get references to components
            rocket[i] = Rockets[i].GetComponent<Rocket>();

            rocket[i].FirePower = tank.FirePower;   //Assign a firepower to the rocket //TODO for now like this, maybe later there will be static reference for each player

            Rockets[i].layer = layerToSet;     //Assign a layer to rockets. 
            foreach (Transform t in Rockets[i].transform)
            {
                t.gameObject.layer = layerToSet;
            }
            rocket[i].otherPlayerLayer = playerNumber == PlayerID.One ? 9 : 8;  //(Hardcoded for now) Assign this so rockets know what player to do damage to. 9 - "PlayerTwo", 8 - "PlayerOne"

        }

        //=============================LASERS===========================

        laserCount = 9;                         //9 laser instances are enough so you are able to shoot without running out of lasers with TurboLazers powerup
        Lasers = new GameObject[laserCount];
        laserRigidbody = new Rigidbody[laserCount]; //Initializing arrays
        laser = new Laser[laserCount];

        for (int i = 0; i < laserCount; i++)    //For all lasers available
        {
            Lasers[i] = Instantiate(laserPrefab, weaponPoolContainer.transform);    //Instantiate a laser childed to their container

            laserRigidbody[i] = Lasers[i].GetComponent<Rigidbody>();    //Get references to components
            laser[i] = Lasers[i].GetComponent<Laser>();

            laser[i].FirePower = tank.FirePower;    //Assign a firepower to the rocket //TODO for now like this, maybe later there will be static reference for each player
                
            Lasers[i].layer = layerToSet;           //Assign a layer to rockets. 
            foreach (Transform t in Lasers[i].transform)
            {
                t.gameObject.layer = layerToSet;
            }
            laser[i].otherPlayerLayer = playerNumber == PlayerID.One ? 9 : 8;       //(Hardcoded for now) Assign this so rockets know what player to do damage to. 9 - "PlayerTwo", 8 - "PlayerOne"
        }
        
    }
    
    private float laserNextFire;    //Variable that increases with Time.time to identify the moment of time we can shoot next laser
    private bool leftLaser = true;  //Variable that goes true-false all the time to change turret from which laser shoots 
    //(2 laser turrets for every tank, except Repulse, 2 turrets in the same spot are made there to be all the same)

    void Update()
    {
        //==================ROCKETS==================
        if (InputManager.GetButtonDown(rocketButtonName, playerNumber)) //If rocket-shoot button is pressed for respective player
        {
            for (int i = 0; i < playerRocketCount; i++) //For each pooled rocket
            {
                if (Rockets[i].activeSelf == false) //If we find a deactivated rocket, shoot it
                {              
                    Rockets[i].SetActive(true); //Activating it before setting parameters, cuz setting rigidbody parameters for disabled objects doesn't work

                    int turretNumber = player.playerStats.MissilesFired % tank.RocketSpawnPoints.Length; //Index number of the turret to shoot the rocket from. "%" means remainder of division

                    player.playerStats.MissilesFired++;  //Increasing this after calculating turretNumber, so the first rocket (with count 0) shoots from the first turren, not from the second

                    rocketRigidbody[i].transform.position = tank.RocketSpawnPoints[turretNumber].position;   //Set the position to shoot rocket from assigned turret Transform Position
                    rocketRigidbody[i].transform.rotation = transform.rotation;                               //Set rocket rotation from tank rotation.

                    rocket[i].shotDirection = transform.TransformDirection(Vector3.forward);

                    Vector3 playerVelocityZGlobal = Vector3.Project(playerRigidbody.velocity, transform.TransformDirection(Vector3.forward)); //Get the velocity of the player in Z axis (player looking forward). This vector returns the vector in global space, so it just adds to the rocket velocity
                    rocketRigidbody[i].velocity = defaultRocketSpeed * transform.TransformDirection(rocketDirection) + playerVelocityZGlobal;   //Set rocket velocity. Transform almost-Vector3.forward to local space relative to the tank + inherit tanks Z velocity                   
                    //TODO Make minimal speed after settled all speed parameters for tanks
                    rocketRigidbody[i].angularVelocity = transform.TransformDirection(Vector3.forward) * 20;    //Set angular velocity so the rocket rotates along its local Z axis for cool looking rocket.

                    //if (doubleDamage)
                    //    rocket[i].FirePower = tank.firePower * 2; //TEST. TODO When implementing, remember to set the damage back to normal

                    break;  //If we found disabled rocket, don't look further.
                }
            }

        }
        //==================LASERS================== 
        if (InputManager.GetButton(laserButtonName, playerNumber) && Time.time > laserNextFire) //If laser-shoot button is pressed for respective player
        {
            laserNextFire = Time.time + laserFireRate;  //Every time we shoot this increases by fire rate time

            for (int i = 0; i < laserCount; i++) //For each pooled laser
            {
                if (Lasers[i].activeSelf == false) //If we find a deactivated laser, shoot it
                {
                    Lasers[i].SetActive(true); //Activating it before setting parameters, cuz setting rigidbody parameters for disabled objects doesn't work

                    int turretNumber = leftLaser ? 0 : 1; //Index number of the turret to shoot the laser from, depending on the boolean
                    leftLaser = !leftLaser;     //After we shot, set the bool so the other turren shoots next

                    laserRigidbody[i].transform.position = tank.LaserSpawnPoints[turretNumber].position;   //Set the position to shoot laser from assigned laser turret Transform Position
                    laserRigidbody[i].transform.rotation = transform.rotation;                               //Set laser rotation from tank rotation.

                    Vector3 playerVelocityZGlobal = Vector3.Project(playerRigidbody.velocity, transform.TransformDirection(Vector3.forward)); //Get the velocity of the player in Z axis (player looking forward). This vector returns the vector in global space, so it just adds to the laser velocity
                    laserRigidbody[i].velocity = defaultLaserSpeed * transform.TransformDirection(Vector3.forward) + playerVelocityZGlobal;   //Set laser velocity. Transform Vector3.forward to local space relative to the tank + inherit tanks Z velocity

                    //tankSoundSource.Play(); //TODO

                    //if (doubleDamage)
                    //    laser[i].FirePower = tank.firePower * 2; //TEST. TODO When implementing, remember to set the damage back to normal

                    break;  //If we found disabled laser, don't look further.
                }
            }

        }

    }


}
