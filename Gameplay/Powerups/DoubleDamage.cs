using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDamage : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.DoubleDamage = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.DoubleDamage = false;
    }

    [SerializeField] private Transform innerCircle;     //Additional moving parts in the powerup
    [SerializeField] private Transform middleCircle;
   
    protected override void FixedUpdate()       //Override this 
    {
        base.FixedUpdate();     //Make it spin like normal

        if (innerCircle != null) innerCircle.Rotate(Vector3.one, 5);            //But also spin the inner parts
        if (middleCircle != null) middleCircle.Rotate(new Vector3(0, 1, 1), 5);     //Checking for null, because if the script is on Mystery powerup, there is no moving parts, but just the regular question mark model of Mystery
    }


    void Reset()
    {
        type = Powerups.DoubleDamage;
        MessageIn = "DOUBLE DAMAGE";
        MessageOut = "NORMAL DAMAGE";
        duration = 12;

        name = MessageIn;
    }
}
