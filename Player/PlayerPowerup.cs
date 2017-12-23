using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerup : MonoBehaviour
{
    public bool BallAttractor;
    public bool BallGuidance;
    public bool DoubleDamage;
    public bool FlightSystem;
    public bool FumbleProtection;
    //TurboLasers just changes things
    //Health just gets applied
    public bool Shielding;
    //SuperSpeed just changes value;
    //Blind just changes things
    public bool Invisibility;
    public bool Invinsibility;
    public bool Stabilizers;


    [HideInInspector] public Player player;
    [HideInInspector] public PlayerShooting playerShooting;
    private PlayerMovement playerMovement;



    void Awake()
    {
        player = GetComponent<Player>();
        playerShooting = GetComponent<PlayerShooting>();
        playerMovement = GetComponent<PlayerMovement>();


    }
    

    //public void PickPowerup(Action<PlayerPowerup> actionIn, Action<PlayerPowerup> actionOut, WaitForSeconds duration)
    public void PickPowerup(Powerup powerup)
    {
        //actionIn(this);
        player.pickup.Play();
        StartCoroutine(pickPowerup(powerup));
    }

    IEnumerator pickPowerup(Powerup powerup)
    {
        if (string.IsNullOrEmpty(powerup.MessageIn) == false)
        {
            player.ShowMessage(powerup.MessageIn);
        }

        powerup.actionIn(this);
        yield return powerup.timer;
        powerup.actionOut(this);

        if (string.IsNullOrEmpty(powerup.MessageOut) == false)
        {
            player.ShowMessage(powerup.MessageOut);
        }
        
    }






}
