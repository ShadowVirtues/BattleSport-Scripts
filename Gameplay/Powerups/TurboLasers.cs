public class TurboLasers : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.playerShooting.laserFireRate = 0.125f;   //Set laserFireRate in PlayerShooting
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.playerShooting.laserFireRate = 0.25f;
    }










    void Reset()
    {
        type = Powerups.TurboLazers;
        MessageIn = "TURBO LASERS";
        MessageOut = "TURBO LASERS OFF";       
    }
}
