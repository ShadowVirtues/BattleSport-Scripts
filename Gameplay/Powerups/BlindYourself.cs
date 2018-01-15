public class BlindYourself : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.blinder.SetActive(true);     //Enable blinder on yourself
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.blinder.SetActive(false);     //Disable blinder on yourself
    }







    void Reset()
    {
        type = Powerups.BlindYourself;
        MessageIn = "BLINDED";
        MessageOut = "SIGHT RESTORED";
        duration = 6;
    }
}
