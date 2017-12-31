using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("References")]                  //References to set in inspector
    [SerializeField] private Transform turret;  //Rotating turret of... turret
    [SerializeField] private Transform laserSpawn;  //Transform on the turret to shoot lasers from

    [SerializeField] private GameObject laserPrefab;    //Laser prefab to pool

    private Transform playerOne;    //Storing references to player transforms, cuz we are gonna lock the turret onto them
    private Transform playerTwo;
    private AudioSource sound;      //Turret has its own source for laser sound

    [Header("Parameters")]          //Parameters for each specific turret
    [SerializeField] private byte turretNumber;     //Turret number in arena (only for naming the laser pooling container)
    [SerializeField] private bool shootPlayerOneFirst = false;  //Should the turret targer first player first on the start of the arent (so not all turret targets a single player all the time, but one targets one, other targets other)
    [SerializeField] private bool canRotateUp = false;          //Maybe we decide that turret can turn its... turret in the vertical direction (initially this was a but, that got turned into a feature)

    private GameObject[] Lasers;   //Array of pooled lasers
    private Rigidbody[] laserRigidbody;    //Array of rididbodies of pooled lasers
    private Laser[] laser;                //Array of Laser components of pooled lasers

    private const float defaultLaserSpeed = 60; //Laser speed
    private int laserCount;             //Count of pooled lasers
    
    void Start()
    {
        playerOne = GameController.Controller.PlayerOne.transform;
        playerTwo = GameController.Controller.PlayerTwo.transform;  //Getting dose references
        sound = GetComponent<AudioSource>();

        //===========POOLING LASERS=============

        GameObject weaponPoolContainer = new GameObject($"Turret{turretNumber}Lasers"); //Make a container in which pool the lasers of each turret

        laserCount = 11;                    //11 is the number of lasers when the turret can shoot then non-stop without running out of them (1 laser to back it up still)       
        Lasers = new GameObject[laserCount];
        laserRigidbody = new Rigidbody[laserCount]; //Initializing arrays
        laser = new Laser[laserCount];

        for (int i = 0; i < laserCount; i++)    //For all lasers available
        {
            Lasers[i] = Instantiate(laserPrefab, weaponPoolContainer.transform);    //Instantiate a laser childed to their container

            laserRigidbody[i] = Lasers[i].GetComponent<Rigidbody>();    //Get references to components
            laser[i] = Lasers[i].GetComponent<Laser>();

            laser[i].FirePower = 50;    //Assign an average firepower to the turret laser

            Lasers[i].layer = 21;           //Assign a layer to lasers. 21 - TurretWeapon
            foreach (Transform t in Lasers[i].transform)    
            {
                t.gameObject.layer = 21;    //Set all hierarchy of the pooled laser to the layer
            }
            laser[i].otherPlayerLayer = 1 << 9 | 1 << 8;  //LayerMask of both players
        }

        //==========================================

        Messenger.AddListener(GameController.SetEverythingBackMessage, SetEverythingBack);  //Add listener for global message of setting everything back

        targetPlayerOne = shootPlayerOneFirst;  //If shooting player one first, set it so this turret starts cycling from the first player
        
        StartCoroutine(nameof(TurretSequence)); //Start coroutine of turret shooting, delaying, turning to other player
    }
    
    private readonly WaitForSeconds shotDelay = new WaitForSeconds(0.125f); //Delay between laser shots
    private readonly WaitForSeconds burstDelay = new WaitForSeconds(2);     //Delay between shot bursts
    
    IEnumerator TurretSequence()
    {
        yield return shotDelay; //When the scene loads, the game gets paused, and this code making the laser shot sound would execute on the arena load, that's why we wait for some time, so in the first frame of the game, the sound doesn't play

        while (true)    //The turret shoots infinitely
        {
            int burstShotsCount = 30;       //One burst laser count
            for (int i = 0; i < burstShotsCount; i++)
            {                
                Shoot();                //Shoot and delay the next shot
                yield return shotDelay;
            }
            FindNewTarget();    //After shooting out all the burst, find the next player to turn the turret to (with tween)

            yield return burstDelay;     //Wait for the turret to turn and start shooting again  
        }      
    }
    
    void Shoot()
    {
        for (int i = 0; i < laserCount; i++) //For each pooled laser
        {
            if (Lasers[i].activeSelf == false) //If we find a deactivated laser, shoot it
            {
                Lasers[i].SetActive(true); //Activating it before setting parameters, cuz setting rigidbody parameters for disabled objects doesn't work

                laserRigidbody[i].transform.position = laserSpawn.position + laserSpawn.TransformDirection(Vector3.forward);   //Set the position to shoot laser from assigned laser turret Transform Position
                laserRigidbody[i].transform.rotation = laserSpawn.rotation;                               //Set laser rotation from laserSpawn rotation.
                
                //Set laser velocity. Transform Vector3.forward to global space relative to the turret rotation by multiplying by rotation Quaternion
                laserRigidbody[i].velocity = defaultLaserSpeed * (laserSpawn.rotation * Vector3.forward);
                
                sound.Play(); //Play laser shot sound that would interrupt itself if played in quick succession
                
                break;   //If we found disabled laser, don't look further.
            }
        }
    }

    private bool targetPlayerOne = true;    //Bool that cycles between bursts, so the turret rotates between turning to each player

    void FindNewTarget()
    {
        Vector3 posToSeek = targetPlayerOne ? playerOne.position : playerTwo.position;  //Setting position of the player to turn to

        if (canRotateUp == false) posToSeek.y = 0;  //Zeroing y vector to not vertically rotate the turret if the bool is set

        targetPlayerOne = !targetPlayerOne; //Reverse the flag to turn the the other player the next burst

        Vector3 euler = Quaternion.LookRotation(posToSeek - transform.position).eulerAngles;    //Generate eulerAngles rotation from the vector between turret and player

        turret.DORotate(euler, 1);  //Rotate the turret over one second to the player position

    }
    
    //=========================

    void OnDestroy() 
    {
        Messenger.RemoveListener(GameController.SetEverythingBackMessage, SetEverythingBack);   //Same stuff as for DecoyGoal/Ball
    }
    
    private void SetEverythingBack()     //For this turret
    {
        StopCoroutine(nameof(TurretSequence));  //Stop the sequence

        turret.rotation = Quaternion.identity;  //Set initial rotation

        for (int i = 0; i < laserCount; i++)    //Set everything back for each individual laser
        {
            laser[i].SetEverythingBack();
        }

        StartCoroutine(nameof(TurretSequence)); //Start the sequence back
    }
    
}
