using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurboLasers : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.playerShooting.laserFireRate = 0.125f;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.playerShooting.laserFireRate = 0.25f;
    }










    void Reset()
    {
        MessageIn = "TURBO LASERS";
        MessageOut = "TURBO LASERS OFF";
        duration = 12;
    }
}
