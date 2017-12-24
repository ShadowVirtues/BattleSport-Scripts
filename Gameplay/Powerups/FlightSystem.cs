using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightSystem : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.FlightSystem = true;
        player.playerMovement.airbourne = false;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.FlightSystem = false;
        player.GetComponent<Rigidbody>().useGravity = true;
    }
    
    //TODO HE'S AIRBOURNE





    void Reset()
    {
        MessageIn = "FLIGHT SYSTEM";
        MessageOut = "FLIGHT EXPIRED";
        duration = 12;
    }
}
