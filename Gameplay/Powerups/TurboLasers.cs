using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurboLasers : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.playerShooting.laserFireRate = 0.125f;   //Set laserFireRate in PlayerShooting
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.playerShooting.laserFireRate = 0.25f;
    }










    void Reset()
    {
        type = Powerups.TurboLazers;
        MessageIn = "TURBO LASERS";
        MessageOut = "TURBO LASERS OFF";
        duration = 12;

        name = MessageIn;
    }
}
