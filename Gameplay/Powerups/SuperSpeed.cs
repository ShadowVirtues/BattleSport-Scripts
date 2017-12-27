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

    [SerializeField] private Transform fans;    //Has rotating fans inside

    protected override void FixedUpdate()
    {               
        if (fans != null)   //If there are fans
        {
            transform.Rotate(Vector3.up, 3);    //Rotate the whole powerup with 3 speed
            fans.Rotate(Vector3.right, 5);      //Rotate the fans
        }
        else
        {   
            base.FixedUpdate();     //If there is no fans, which means it's Mystery, rotate with normal speed
        }
        
    }


    void Reset()
    {
        type = Powerups.SuperSpeed;
        MessageIn = "SUPER SPEED";
        MessageOut = "NORMAL SPEED";
        duration = 12;

        name = MessageIn;
    }
}
