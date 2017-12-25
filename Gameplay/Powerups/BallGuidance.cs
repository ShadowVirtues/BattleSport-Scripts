using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGuidance : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.BallGuidance = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.BallGuidance = false;
    }
    






    void Reset()
    {
        type = Powerups.BallGuidance;
        MessageIn = "BALL GUIDANCE";
        MessageOut = "GUIDANCE EXPIRED";
        duration = 12;
    }
}
