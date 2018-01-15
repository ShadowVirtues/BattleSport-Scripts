public class FumbleProtection : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.FumbleProtection = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.FumbleProtection = false;
    }
    






    void Reset()
    {
        type = Powerups.FumbleProtection;
        MessageIn = "FUMBLE PROTECTION";
        MessageOut = "PROTECTION OFF";       
    }
}
