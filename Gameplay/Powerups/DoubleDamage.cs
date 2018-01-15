using UnityEngine;

public class DoubleDamage : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.DoubleDamage = true;
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.DoubleDamage = false;
    }

    [SerializeField] private Transform innerCircle;     //Additional moving parts in the powerup
    [SerializeField] private Transform middleCircle;
   
    protected override void Update()       //Override this 
    {
        base.Update();     //Make it spin like normal

        if (innerCircle != null) innerCircle.Rotate(Vector3.one, 500 * Time.deltaTime);            //But also spin the inner parts
        if (middleCircle != null) middleCircle.Rotate(new Vector3(0, 1, 1), 500 * Time.deltaTime);     //Checking for null, because if the script is on Mystery powerup, there is no moving parts, but just the regular question mark model of Mystery
    }


    void Reset()
    {
        type = Powerups.DoubleDamage;
        MessageIn = "DOUBLE DAMAGE";
        MessageOut = "NORMAL DAMAGE";       
    }
}
