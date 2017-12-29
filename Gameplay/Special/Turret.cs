using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turret;
    [SerializeField] private Transform laserSpawn;

    [SerializeField] private GameObject laserPrefab;

    private Transform playerOne;
    private Transform playerTwo;
    private AudioSource sound;

    [Header("Parameters")]
    [SerializeField] private byte turretNumber;
    [SerializeField] private bool shootPlayerOneFirst = false;
    [SerializeField] private bool canRotateUp = false;

    private GameObject[] Lasers;   //Array of pooled lasers
    private Rigidbody[] laserRigidbody;    //Array of rididbodies of pooled lasers
    private Laser[] laser;                //Array of Laser components of pooled lasers

    private const float defaultLaserSpeed = 60;
    private int laserCount;
    

    void Start()
    {
        playerOne = GameController.Controller.PlayerOne.transform;
        playerTwo = GameController.Controller.PlayerTwo.transform;
        sound = GetComponent<AudioSource>();
        //========================

        GameObject weaponPoolContainer = new GameObject($"Turret{turretNumber}Lasers");

        laserCount = 11;                         
        Lasers = new GameObject[laserCount];
        laserRigidbody = new Rigidbody[laserCount]; //Initializing arrays
        laser = new Laser[laserCount];

        for (int i = 0; i < laserCount; i++)    //For all lasers available
        {
            Lasers[i] = Instantiate(laserPrefab, weaponPoolContainer.transform);    //Instantiate a laser childed to their container

            laserRigidbody[i] = Lasers[i].GetComponent<Rigidbody>();    //Get references to components
            laser[i] = Lasers[i].GetComponent<Laser>();

            laser[i].FirePower = 50;    //Assign a firepower to the lasers  (DoubleDamage just reapplies double firepower to weapons, when picked up)

            Lasers[i].layer = 21;           //Assign a layer to lasers. 
            foreach (Transform t in Lasers[i].transform)
            {
                t.gameObject.layer = 21;
            }
            laser[i].otherPlayerLayer = 1 << 9 | 1 << 8;  //LayerMask of both players
        }

        //=================

        Messenger.AddListener(SetEverythingBackTurret, SetEverythingBack);

        targetPlayerOne = shootPlayerOneFirst;
        
        StartCoroutine(nameof(TurretSequence));
    }


    private readonly WaitForSeconds shotDelay = new WaitForSeconds(0.125f);
    private readonly WaitForSeconds burstDelay = new WaitForSeconds(2);
    

    IEnumerator TurretSequence()
    {
        yield return shotDelay;

        while (true)
        {
            int burstShotsCount = 40;
            for (int i = 0; i < burstShotsCount; i++)
            {                
                Shoot();
                yield return shotDelay;
            }
            FindNewTarget();

            yield return burstDelay;

            

            
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
                laserRigidbody[i].transform.rotation = laserSpawn.rotation;                               //Set laser rotation from tank rotation.
                
                //Set laser velocity. Transform Vector3.forward to global space relative to the turret rotation by multiplying by rotation Quaternion + inherit tanks Z velocity
                laserRigidbody[i].velocity = defaultLaserSpeed * (laserSpawn.rotation * Vector3.forward);
                
                sound.Play(); //Play laser shot sound that would interrupt itself if played in quick succession
                
                break;   //If we found disabled laser, don't look further.
            }
        }
    }


    private bool targetPlayerOne = true;

    void FindNewTarget()
    {
        Vector3 posToSeek = targetPlayerOne ? playerOne.position : playerTwo.position;

        if (canRotateUp == false) posToSeek.y = 0;

        targetPlayerOne = !targetPlayerOne;

        Vector3 euler = Quaternion.LookRotation(posToSeek - transform.position).eulerAngles;

        turret.DORotate(euler, 1);

    }


    public const string SetEverythingBackTurret = "SetEverythingBackTurret";

    
    void OnDestroy() 
    {
        Messenger.RemoveListener(SetEverythingBackTurret, SetEverythingBack);
    }



    public void SetEverythingBack()     //For all pooled lasers, launch their own function once again :D
    {
        StopCoroutine(nameof(TurretSequence));

        turret.rotation = Quaternion.identity;

        for (int i = 0; i < laserCount; i++)
        {
            laser[i].SetEverythingBack();
        }

        
        StartCoroutine(nameof(TurretSequence));
    }
    
}
