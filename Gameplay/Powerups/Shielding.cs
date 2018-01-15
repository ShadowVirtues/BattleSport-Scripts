public class Shielding : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.Shielding = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.Shielding = false;
    }
    






    void Reset()
    {
        type = Powerups.Shielding;
        MessageIn = "SHIELDING";
        MessageOut = "SHIELDING OFF";       
    }
}
