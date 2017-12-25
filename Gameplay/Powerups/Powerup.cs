using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public enum Powerups
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

public abstract class Powerup : MonoBehaviour
{
    public Powerups type;

    public Sprite icon;
        
    public string MessageIn;
    public string MessageOut;

    public int duration;
    public WaitForSeconds timer;
    
    protected abstract void ActionIn(PlayerPowerup player);
    protected abstract void ActionOut(PlayerPowerup player);
    
    public Action<PlayerPowerup> actionIn;
    public Action<PlayerPowerup> actionOut;

    void Awake()
    {
        actionIn = ActionIn;
        actionOut = ActionOut;
        timer = new WaitForSeconds(duration);
    }

    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (this.enabled == false) return;

        PlayerPowerup player = other.GetComponentInParent<PlayerPowerup>();
        player.PickPowerup(this);
        
        gameObject.SetActive(false);
    }

    protected virtual void FixedUpdate()
    {
        transform.Rotate(Vector3.up, 5);
    }



    //GameController probably handles spawning powerups. 
    //The timer between powerup spawns is random between 8 and 16 seconds
    //It starts from the start of the round, then when the time comes it randomly picks powerup that isnt on the field or applied to some player and spawns it, then if some other powerup is due, starts the timer again.
    //Triggers to start powerup countdown: arena start, spawned powerup, powerup effect ended
    //If some of those occur, start countdown only if it isn't running already


}
