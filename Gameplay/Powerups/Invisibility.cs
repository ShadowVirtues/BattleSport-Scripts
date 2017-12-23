using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.player.CloakEngage();
        player.Invisibility = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.player.CloakDisengage();
        player.Invisibility = false;
    }

    //TODO Test with SetEverythingBack








    void Reset()
    {
        MessageIn = "CLOAK ENGAGED";
        MessageOut = "CLOAK DISENGAGED";
        duration = 12;
    }
}
