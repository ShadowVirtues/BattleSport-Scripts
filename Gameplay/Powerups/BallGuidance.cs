public class BallGuidance : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.BallGuidance = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.BallGuidance = false;
    }
    






    void Reset()
    {
        type = Powerups.BallGuidance;
        MessageIn = "BALL GUIDANCE";
        MessageOut = "GUIDANCE EXPIRED";      
    }
}
