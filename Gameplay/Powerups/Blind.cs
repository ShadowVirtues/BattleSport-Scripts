using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blind : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.player.Blind(mystery);      
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.player.UnBlind(mystery);
    }

    private bool mystery;

    void Start()
    {
        mystery = GetComponent<Mystery>() != null;
    }





    void Reset()
    {
        MessageIn = "";
        MessageOut = "";
        duration = 5;
    }
}
