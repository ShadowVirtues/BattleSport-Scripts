using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invisibility : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.player.CloakEngage();    //Run a function on a player to fade the material
        player.Invisibility = true;     //Set bool like usual
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.player.CloakDisengage(); //Unfade the material
        player.Invisibility = false;
    }

    //TODO Test with SetEverythingBack








    void Reset()
    {
        type = Powerups.Invisibility;
        MessageIn = "CLOAK ENGAGED";
        MessageOut = "CLOAK DISENGAGED";
        duration = 12;
    }
}
