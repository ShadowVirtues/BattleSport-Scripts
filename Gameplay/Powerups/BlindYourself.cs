using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindYourself : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.blinder.SetActive(true);
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.blinder.SetActive(false);
    }







    void Reset()
    {
        type = Powerups.BlindYourself;
        MessageIn = "BLINDED";
        MessageOut = "SIGHT RESTORED";
        duration = 6;
    }
}
