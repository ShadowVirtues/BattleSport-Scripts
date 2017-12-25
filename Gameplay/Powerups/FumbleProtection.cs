using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FumbleProtection : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.FumbleProtection = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.FumbleProtection = false;
    }
    






    void Reset()
    {
        type = Powerups.FumbleProtection;
        MessageIn = "FUMBLE PROTECTION";
        MessageOut = "PROTECTION OFF";
        duration = 12;
    }
}
