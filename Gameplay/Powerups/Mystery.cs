using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mystery : MonoBehaviour    //Script that is attached to Mystery powerup along with ALL other powerup scripts
{

    private Powerup[] allPowerups;  //This is the array of all powerup scripts on mystery powerup GameObject

    void Awake()
    {
        allPowerups = GetComponents<Powerup>(); //Get all Powerup script componentS

        foreach (Powerup pow in allPowerups)    //Disable all of them on Awake
        {
            pow.enabled = false;
        }
    }

    void OnEnable()     //When Mystery gets enabled ("spawned") in the arena, disable all Powerup components
    {
        foreach (Powerup pow in allPowerups)
        {
            pow.enabled = false;
        }
        allPowerups[Random.Range(0, allPowerups.Length)].enabled = true;    //And enable a random one
    }
    //IT'S SUCH A SUPER FREAKING BADASS AND SMART IMPLEMENTATION OF MYSTERY POWERUP!!!!!!!!!!!!!!!!!!

    








}
