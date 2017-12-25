using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

public class BlindEnemy : BlindYourself
{

    private const string EnemyBlinded = "ENEMY BLINDED";

    protected override void OnTriggerEnter(Collider other)
    {
        if (this.enabled == false) return;

        Player player = other.GetComponentInParent<Player>();

        player.ShowMessage(EnemyBlinded);

        if (player.PlayerNumber == PlayerID.One)
        {           
            GameController.Controller.PlayerTwo.powerup.PickPowerup(this);
            
        }
        else if (player.PlayerNumber == PlayerID.Two)
        {            
            GameController.Controller.PlayerOne.powerup.PickPowerup(this);
        }

        

        gameObject.SetActive(false);
    }

    
}
