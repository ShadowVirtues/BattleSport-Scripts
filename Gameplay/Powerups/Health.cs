using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.player.SetHealth(100);
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        
    }

    protected override void OnTriggerEnter(Collider other)
    {
        
    }

    void OnTriggerStay(Collider other)
    {
        if (this.enabled == false) return;

        PlayerPowerup player = other.GetComponentInParent<PlayerPowerup>();
        if (player.player.Health != 100)
        {
            player.PickPowerup(this);

            gameObject.SetActive(false);
        }

        

    }


    void Reset()
    {
        MessageIn = "HEALTH RESTORED";
        MessageOut = "";
        duration = 0;
    }
}
