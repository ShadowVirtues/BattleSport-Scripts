using UnityEngine;

public class SuperSpeed : Powerup
{
    public override void ActionIn(PlayerPowerup player)
    {
        player.playerMovement.SetSuperSpeed(true);
    }

    public override void ActionOut(PlayerPowerup player)
    {
        player.playerMovement.SetSuperSpeed(false);
    }

    [SerializeField] private Transform fans;    //Has rotating fans inside

    protected override void Update()
    {
        if (fans != null)   //If there are fans
        {
            transform.Rotate(Vector3.up, 300 * Time.deltaTime);    //Rotate the whole powerup with 3 speed
            fans.Rotate(Vector3.right, 500 * Time.deltaTime);      //Rotate the fans
        }
        else
        {   
            base.Update();     //If there is no fans, which means it's Mystery, rotate with normal speed
        }
        
    }


    void Reset()
    {
        type = Powerups.SuperSpeed;
        MessageIn = "SUPER SPEED";
        MessageOut = "NORMAL SPEED";       
    }
}
