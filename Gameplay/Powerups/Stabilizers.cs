using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stabilizers : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.Stabilizers = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.Stabilizers = false;
    }
    






    void Reset()
    {
        type = Powerups.Stabilizers;
        MessageIn = "STABILIZERS";
        MessageOut = "STABILIZERS OFF";
        duration = 12;
    }
}
