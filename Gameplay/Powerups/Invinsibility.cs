using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invinsibility : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.Invinsibility = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.Invinsibility = false;
    }
    






    void Reset()
    {
        MessageIn = "INVINSIBILITY";
        MessageOut = "INVINSIBILITY OFF";
        duration = 12;
    }
}
