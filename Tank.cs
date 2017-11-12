using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public float Acceleration;
    public float TopSpeed;
    public float FirePower;
    public float Armor;
    public float BallHandling; //???
    public int RocketCount;

    public Transform[] RocketSpawnPoints;
    public Transform[] LaserSpawnPoints;
}
