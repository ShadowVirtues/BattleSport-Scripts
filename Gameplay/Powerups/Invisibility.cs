public class Invisibility : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.player.CloakEngage();    //Run a function on a player to fade the material
        player.Invisibility = true;     //Set bool like usual
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.player.CloakDisengage(); //Unfade the material
        player.Invisibility = false;
    }
    







    void Reset()
    {
        type = Powerups.Invisibility;
        MessageIn = "CLOAK ENGAGED";
        MessageOut = "CLOAK DISENGAGED";       
    }
}
