using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mystery : MonoBehaviour
{

    private Powerup[] allPowerups;

    void Awake()
    {
        allPowerups = GetComponents<Powerup>();

        foreach (Powerup pow in allPowerups)
        {
            pow.enabled = false;
        }
    }

    void OnEnable()
    {
        foreach (Powerup pow in allPowerups)
        {
            pow.enabled = false;
        }
        allPowerups[Random.Range(0, allPowerups.Length)].enabled = true;
    }

    //TEST OBVIOUSLY HEAVY
    //TODO add other scripts to it
    









}
