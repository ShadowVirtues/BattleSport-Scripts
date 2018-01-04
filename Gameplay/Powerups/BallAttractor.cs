public class BallAttractor : Powerup
{
    protected override void ActionIn(PlayerPowerup player)
    {
        player.BallAttractor = true;
    }

    protected override void ActionOut(PlayerPowerup player)
    {
        player.BallAttractor = false;
    }
    






    void Reset()
    {
        type = Powerups.BallAttractor;
        MessageIn = "BALL ATTRACTOR";
        MessageOut = "ATTRACTOR OFF";
        duration = 12;

        name = MessageIn;
    }
}
