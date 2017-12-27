using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public enum Powerups    //enum with all powerup types - used in the Dictionary of active powerups to refresh their duration if the same powerup was picked
{
    BallAttractor,
    BallGuidance,
    DoubleDamage,
    FlightSystem,
    FumbleProtection,
    TurboLazers,
    Health,
    Shielding,
    SuperSpeed,
    BlindEnemy,
    BlindYourself,
    Invisibility,
    Invinsibility,
    Stabilizers,
    Mystery
}

public abstract class Powerup : MonoBehaviour   //Base abstract class for all powerups
{
    public Powerups type;   //enum type

    public Sprite tooltip;     //icon sprite of the powerup to change it in the tooltip
    public Sprite icon;         //Icon for countdown panel
    
    public string MessageIn;    //Message player sees when picking the powerup
    public string MessageOut;   //Message player sees when the powerup expires

    public int duration;        //Duration of powerup
    public WaitForSeconds timer;//Caching this on depending on duration
    
    protected abstract void ActionIn(PlayerPowerup player); //Abstract method, implemented in all powerup children classes. Action to execute when picked the powerup
    protected abstract void ActionOut(PlayerPowerup player);//And when the powerup expires. PlayerPowerup is a paramater, because this is the class which handles all the functions needed to be executed (we need to get to it somehow from powerup code)
    
    public Action<PlayerPowerup> actionIn;  //This was the first implementation, where we needed to pass the Action<> as a parameter, now we pass the powerup instance itself with its ActionIn/ActionOut voids, so this is not really necessary
    public Action<PlayerPowerup> actionOut; //So to not change all ActionIn/Outs to "public" in all derived classes, I'm leaving this implementation, when only Action<> is public and gets invoked from other scripts, it kinda brings incapsulation anyway tho

    public string Name; //Name in countdown panel

    void Awake()
    {
        actionIn = ActionIn;    //Assign actions
        actionOut = ActionOut;
        timer = new WaitForSeconds(duration);   //Cache the duration
    }

    
    protected virtual void OnTriggerEnter(Collider other)   //When some player enters the powerup trigger (virtual is overridden for BlindEnemy, when we make it like the other player picked "BlindYourself" powerup)
    {
        if (this.enabled == false) return;  //OnTriggerEnter runs on disabled scripts (which we have a load on Mystery powerup), so only run this if the script is actually enabled

        PlayerPowerup player = other.GetComponentInParent<PlayerPowerup>(); //Get the PlayerPowerup component of triggered player. Not caching it, because it doesn't happen all that often (compared to laser/rocket hitting, say)
        player.PickPowerup(this);   //Execute PickPowerup function on PlayerPowerup side, passing the whole powerup object to the function
        
        gameObject.SetActive(false);    //Disable powerup, so it gets enabled ("spawned") later
    }

    protected virtual void FixedUpdate()    //Overridden for some powerups that have more internal moving parts like SuperSpeed or DoubleDamage
    {
        transform.Rotate(Vector3.up, 5);    //Spinning the powerup
    }


    
}
