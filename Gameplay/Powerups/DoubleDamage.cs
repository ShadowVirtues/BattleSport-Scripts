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

    [SerializeField] private Transform innerCircle;
    [SerializeField] private Transform middleCircle;
   
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (innerCircle != null) innerCircle.Rotate(Vector3.one, 5);
        if (middleCircle != null) middleCircle.Rotate(new Vector3(0, 1, 1), 5);        
    }


    void Reset()
    {
        type = Powerups.DoubleDamage;
        MessageIn = "DOUBLE DAMAGE";
        MessageOut = "NORMAL DAMAGE";
        duration = 12;
    }
}
