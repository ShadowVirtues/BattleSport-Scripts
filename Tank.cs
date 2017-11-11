using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public float acceleration;
    public float topSpeed;
    public float firePower;
    public float armor;
    public float ballHandling; //???
    public int rocketCount;

    public Transform[] rocketSpawnPoints;
    public Transform[] laserSpawnPoints;
}
