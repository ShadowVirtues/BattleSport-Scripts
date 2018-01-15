public class BallAttractor : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.BallAttractor = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.BallAttractor = false;
    }
    






    void Reset()
    {
        type = Powerups.BallAttractor;
        MessageIn = "BALL ATTRACTOR";
        MessageOut = "ATTRACTOR OFF";       
    }
}
