using UnityEngine;

public class Health : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.player.SetHealth(100);   //Just set health to 100
    }

    public override void ActionOut(PlayerPowerup player) //Empty override, because we HAVE to implement abstract stuff, even tho there is nothing to do
    {
        
    }

    protected override void OnTriggerEnter(Collider other)  //Overriding OnTriggerEnter with it being empty, because we need OnTriggerStay here
    {
        
    }

    void OnTriggerStay(Collider other)  //So the player can be INSIDE the Health powerup with full health not picking it, and still pick it if the other player shoots him
    {
        if (this.enabled == false) return;

        PlayerPowerup player = other.GetComponentInParent<PlayerPowerup>();
        if (player.player.Health != 100)    //Only if player health is not full
        {
            player.PickPowerup(this);   //Pick powerup

            gameObject.SetActive(false);    //And disable it
        }

        

    }


    void Reset()
    {
        MessageIn = "HEALTH RESTORED";
        MessageOut = "";
        duration = 0;        
    }
}
