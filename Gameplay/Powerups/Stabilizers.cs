public class Stabilizers : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.Stabilizers = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.Stabilizers = false;
    }
    






    void Reset()
    {
        type = Powerups.Stabilizers;
        MessageIn = "STABILIZERS";
        MessageOut = "STABILIZERS OFF";
    }
}
