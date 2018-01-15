public class Invinsibility : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.Invinsibility = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.Invinsibility = false;
    }
    






    void Reset()
    {
        type = Powerups.Invinsibility;
        MessageIn = "INVINCIBILITY";
        MessageOut = "INVINCIBILITY OFF";
    }
}
