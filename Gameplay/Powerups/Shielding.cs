using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shielding : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.Shielding = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.Shielding = false;
    }
    






    void Reset()
    {
        type = Powerups.Shielding;
        MessageIn = "SHIELDING";
        MessageOut = "SHIELDING OFF";
        duration = 12;

        name = MessageIn;
    }
}
