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
    private const string rocketButtonName = "Rocket";        //Input for rocket shooting is handled here so caching this
    private const string laserButtonName = "Laser";         //Input for laser shooting

    [SerializeField] private AudioSource rocketSource;    //Each player has 3 AudioSources on them, two or which are for laser and rocket shot sound, which are self-interrupting
    [SerializeField] private AudioSource laserSource;      

    public const float defaultRocketSpeed = 35; //Rocket speed is getting set here, it is getting amplified by player velocity in Z direction (it is used in Rocket.cs to calculate the rocket flight vector)
    private const float minimalRocketSpeed = 25;    //Rocket minimal speed, so when the tank is moving back while shooting, rocket isn't super slow
    private const float defaultLaserSpeed = 60; //Same for lasers, except lasers don't push other players, so private
    private const float minimalLaserSpeed = 40;    //Laser minimal speed, so when the tank is moving back while shooting, laser isn't super slow

    private readonly Vector3 rocketDirection = new Vector3(0,-0.01f,1);    //TEST with flight. Vector to shoot rocket to. It is 'forward' with slight drag down (kinda like gravity). In the game has a big effect when one player is flying, the other one can't hit him from longer range
    
    void Start()        //Since when loading the scene, we first spawn PlayerPrefab (which would INSTANTLY run Awake here) and only after that we get the Tank in, we have to get all references in Start, when the Tank has already been put in
    {
        player = GetComponent<Player>();    
        playerNumber = player.PlayerNumber;
        playerRigidbody = GetComponent<Rigidbody>();
        tank = GetComponentInChildren<Tank>(); //Tank component gets inserted to PlayerX gameObject with the tank model when the tank is picked.

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

            rocket[i].FirePower = tank.FirePower;   //Assign a firepower to the rocket  (DoubleDamage just reapplies double firepower to weapons, when picked up)

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

            laser[i].FirePower = tank.FirePower;    //Assign a firepower to the lasers  (DoubleDamage just reapplies double firepower to weapons, when picked up)

            Lasers[i].layer = layerToSet;           //Assign a layer to lasers. 
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
        if (Time.timeScale == 0) return;    //When you pause the game with timeScale, Updates still keep running, and we don't want players to shoot rockets while in pause
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
                    
                    //Set rocket velocity. Tank's turrets point slightly inside, and we have gravity on rockets injected in "rocketDirection", 
                    //so multiply turret rotation Quaternion by rocketDirection, to get global Vector3 of turret rotation in direction of rocketDirection  + inherit tanks Z velocity                                   
                    rocketRigidbody[i].velocity = defaultRocketSpeed * (tank.RocketSpawnPoints[turretNumber].rotation * rocketDirection) + playerVelocityZGlobal;

                    if (rocketRigidbody[i].velocity.magnitude < minimalRocketSpeed) //Rocket minimal speed is 25, if after inheriting tanks speed it's less then minimal, make it minimal speed
                    {
                        rocketRigidbody[i].velocity = rocketRigidbody[i].velocity.normalized * minimalRocketSpeed;
                    }
                        
                    rocketRigidbody[i].angularVelocity = transform.TransformDirection(Vector3.forward) * 20;    //Set angular velocity so the rocket rotates along its local Z axis for cool looking rocket.

                    rocketSource.Play();    //Play rocket shot sound that would interrupt itself if played in quick succession

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

                    laserRigidbody[i].transform.position = tank.LaserSpawnPoints[turretNumber].position + transform.TransformDirection(Vector3.forward);   //Set the position to shoot laser from assigned laser turret Transform Position
                    laserRigidbody[i].transform.rotation = transform.rotation;                               //Set laser rotation from tank rotation.
                    
                    Vector3 playerVelocityZGlobal = Vector3.Project(playerRigidbody.velocity, transform.TransformDirection(Vector3.forward)); //Get the velocity of the player in Z axis (player looking forward). This vector returns the vector in global space, so it just adds to the laser velocity
                    
                    //Set laser velocity. Transform Vector3.forward to global space relative to the turret rotation by multiplying by rotation Quaternion + inherit tanks Z velocity
                    laserRigidbody[i].velocity = defaultLaserSpeed * (tank.LaserSpawnPoints[turretNumber].rotation * Vector3.forward) + playerVelocityZGlobal;

                    if (laserRigidbody[i].velocity.magnitude < minimalLaserSpeed) //Laser minimal speed is 40, if after inheriting tanks speed it's less then minimal, make it minimal speed
                    {
                        laserRigidbody[i].velocity = laserRigidbody[i].velocity.normalized * minimalLaserSpeed;
                    }

                    laserSource.Play(); //Play laser shot sound that would interrupt itself if played in quick succession

                    //if (doubleDamage)
                    //    laser[i].FirePower = tank.firePower * 2; //TEST. TODO When implementing, remember to set the damage back to normal

                    break;  //If we found disabled laser, don't look further.
                }
            }

        }

    }


}
