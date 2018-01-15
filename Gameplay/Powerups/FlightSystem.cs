using UnityEngine;

public class FlightSystem : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.FlightSystem = true;                 //Enable the bool, as usual
        player.playerMovement.airbourne = false;    //Set the airbourne bool for the player indicating that the announcer should say "He's airbourne" when the player starts flying
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.FlightSystem = false;    //Disable the bool
        player.GetComponent<Rigidbody>().useGravity = true;     //Make sure the gravity is enabled for the player (we enable-disable it at some points when the flight is active)
    }
    




    void Reset()
    {
        type = Powerups.FlightSystem;
        MessageIn = "FLIGHT SYSTEM";
        MessageOut = "FLIGHT EXPIRED";
    }
}
