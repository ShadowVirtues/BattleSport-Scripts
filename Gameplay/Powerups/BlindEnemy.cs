using TeamUtility.IO;
using UnityEngine;

public class BlindEnemy : BlindYourself     //Inherits from BlindYouself!
{

    private const string EnemyBlinded = "ENEMY BLINDED";

    protected override void OnTriggerEnter(Collider other)  //Overriding OnTriggerEnter to make it like the other player picked up "BlindYourself" powerup
    {
        if (this.enabled == false) return;  //This one won't be on mystery, but writing this anyway

        Player player = other.GetComponentInParent<Player>();   //Get Player component instead of PlayerPowerup

        player.ShowMessage(EnemyBlinded);   //Show message for PICKED player, that he blinded enemy

        if (player.PlayerNumber == PlayerID.One)
        {           
            GameController.Controller.PlayerTwo.powerup.PickPowerup(this);  //Launch a function for OTHER player like they picked the powerup
            
        }
        else if (player.PlayerNumber == PlayerID.Two)
        {            
            GameController.Controller.PlayerOne.powerup.PickPowerup(this);
        }
        
        gameObject.SetActive(false);
    }

    
}
