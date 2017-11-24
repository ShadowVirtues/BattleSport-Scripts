using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*
 
    TODO Manage rocked "gravity"

    TODO Armor will mean player mass which also gets calculated in knock force
    
    
*/
public class Rocket : MonoBehaviour
{
    [SerializeField] private GameObject rocketModel;    //Reference to rocket model and explosion to disable-enable them when rocket explodes
    [SerializeField] private GameObject explosion;
    private readonly WaitForSeconds explosionTime = new WaitForSeconds(0.5f);   //Explosion duration

    public float FirePower; //Rocket parameter for damage and knockback to apply. Gets set in PlayerShooting when pressing a rocket button
    public int otherPlayerLayer;    //To decide in OnCollisionEnter if we should apply force to hit object (Other player)
    public Vector3 shotDirection;   //Initial rocket facing direction, this is used in OnCollisionEnter, because rigidbody.velocity changes its vector when hitting the collider (reflects from hit surface)

    private const float forceCoeff = 0.3f;    //Coefficient to convert FirePower to Force

    //The knockback vector is getting combined from
    private const float normalForceRatio = 0.3f;   //The normal to the tank collider hit 
    private const float upForceRatio = 0.3f;      //The knockup force
    //And the actual velocity vector of the rocket which equals to (1 - normalForceRatio - upForceRatio). So they add up to total value of 1 in the end.

    private new Rigidbody rigidbody; //Cache rockets rigidbody to get its velocity vector

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();  //Cache rockets rigidbody to get its velocity vector
    }

    void OnCollisionEnter(Collision other)  //When the rocket collides with something
    {       
        if (other.gameObject.layer == otherPlayerLayer) //If the collided object is other player
        {//Apply knockback force to the player

            float codirCoeff = 0;   //Co-directional coefficient. This is used so when the player is moving in the direction of the rocket that hits him, the player gets pushed much more than when idle
            float dot = Vector3.Dot(shotDirection, other.rigidbody.velocity);   //Calculate the dot product to know how much rocket direction and player direction match to decide how much force to apply
            if (dot > 0)    //Only if the rocket and the player face the same direction
            {
                codirCoeff = dot * 0.2f + 1;    //Multiply by coefficient to get reasonable force and +1 so when dot=0, codirCoeff=1 and the initial force vector doesn't change
            }

            Vector3 forceVector = normalForceRatio * -other.contacts[0].normal +    //Normal to the collider part
                                   (1 - normalForceRatio - upForceRatio) * shotDirection * codirCoeff   //Rocket velocity vector part * codirCoeff is the player is moving the same direction as the rocket that hits him
                                   + Vector3.up * upForceRatio;     //And finally knockup part

            other.rigidbody.AddForce(forceVector * FirePower * forceCoeff, ForceMode.VelocityChange);   //Add force to the other player combined with a vector
            
            if (otherPlayerLayer == 8)           //PlayerOne layer, then hit player one (getting the reference from GameController)
            {
                GameController.Controller.PlayerTwo.playerStats.MissilesHit++;
                GameController.Controller.PlayerOne.Hit(FirePower, Weapon.Rocket);    
            }
            else if (otherPlayerLayer == 9)     //PlayerTwo layer 
            {
                GameController.Controller.PlayerOne.playerStats.MissilesHit++;
                GameController.Controller.PlayerTwo.Hit(FirePower, Weapon.Rocket);
            }
            //I could do it with "other.gameObject.GetComponent<Player>().Hit(FirePower, Weapon.Rocket);", but to not get reference every time, just get them from GameController
        }

        StartCoroutine(RocketExplosion());  //After colliding, disable rocket model and make explosion, then disable the whole rocket object for reusage
    }

    IEnumerator RocketExplosion()
    {
        rigidbody.velocity = Vector3.zero;          //Since rocket's collider is not trigger (cuz we need collision normal), on collision with something it will reflect, that's why we manually stop it
        rigidbody.angularVelocity = Vector3.zero;

        rocketModel.SetActive(false);   //Disable rocket
        explosion.SetActive(true);      //Make explosion
        yield return explosionTime;     //Wait for it to end
        explosion.SetActive(false);     //Disable explosion
        rocketModel.SetActive(true);    //Enable rocket (for future reusage)
        gameObject.SetActive(false);    //Disable the rocket so it can be used as another rocket

    }

    void FixedUpdate()
    {
        //print(rigidbody.velocity.magnitude);

        //if (rigidbody.position.y < 0.6f) rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
    }

    void OnTriggerEnter(Collider other) //COMM  //TEST with tanks with low turrets
    {
        if (other.gameObject.layer == 20)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
        }

    }

}
