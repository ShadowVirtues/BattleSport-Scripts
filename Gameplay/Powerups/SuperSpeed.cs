using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperSpeed : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.playerMovement.SetSuperSpeed(true);
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.playerMovement.SetSuperSpeed(false);
    }

    [SerializeField] private Transform fans;

    protected override void FixedUpdate()
    {        
        
        if (fans != null)
        {
            transform.Rotate(Vector3.up, 3);
            fans.Rotate(Vector3.right, 5);  
        }
        else
        {
            base.FixedUpdate();
        }
        
    }


    void Reset()
    {
        MessageIn = "SUPER SPEED";
        MessageOut = "NORMAL SPEED";
        duration = 12;
    }
}
