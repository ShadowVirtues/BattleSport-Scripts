using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*
 
    Test rocket-rocket collisions

    Armor will mean player mass which also gets calculated in knock force
    
    
*/
public class Rocket : MonoBehaviour
{
    [SerializeField] private GameObject rocketModel;    //Reference to rocket model and explosion to disable-enable them when rocket explodes
    [SerializeField] private GameObject explosion;
    private readonly WaitForSeconds explosionTime = new WaitForSeconds(0.5f);   //Explosion duration

    public float FirePower; //Rocket parameter for damage and knockback to apply. Gets set in PlayerShooting when pressing a rocket button
    public int otherPlayerLayer;    //To decide in OnCollisionEnter if we should apply force to hit object (Other player)

    private const float forceCoeff = 0.2f;    //Coefficient to convert FirePower to Force

    //The knockback vector is getting combined from
    private const float normalForceRatio = 0.3f;   //The normal to the tank collider hit 
    private const float upForceRatio = 0.3f;      //The knockup force
    //And the actual velocity vector of the rocket which equals to (1 - normalForceRatio - upForceRatio). So they add up to 1 in the end.

    private new Rigidbody rigidbody; //Cache rockets rigidbody to get its velocity vector

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();  //Cache rockets rigidbody to get its velocity vector
    }

    void OnCollisionEnter(Collision other)  //When the rocket collides with something
    {       
        if (other.gameObject.layer == otherPlayerLayer) //If the collided object is other player
        {//Apply knockback force to the player
            Vector3 forceVector = normalForceRatio * -other.contacts[0].normal +    //Normal to the collider part
                                   (1 - normalForceRatio - upForceRatio) * rigidbody.velocity / PlayerShooting.defaultRocketSpeed   //Rocket velocity vector part
                                   + Vector3.up * upForceRatio;     //And finally knockup part

            other.rigidbody.AddForce(forceVector * FirePower * forceCoeff, ForceMode.VelocityChange);   //Add force to the other player combined with a vector

            if (otherPlayerLayer == 8)
            {
                GameController.Controller.playerOne.Hit(FirePower, Weapon.Rocket);
            }
            else if (otherPlayerLayer == 9)
            {
                GameController.Controller.playerTwo.Hit(FirePower, Weapon.Rocket);
            }


            //float enemyArmor = other.gameObject.GetComponentInChildren<Tank>().armor;
            //float damage = (160 - enemyArmor) / 250 * FirePower;
            //other.gameObject.GetComponent<Player>().Hit(damage, Weapon.Rocket);    //TODO For now it's like this. In future, we will maybe have a static PlayerOne/Two reference so we can easily get their fields


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

	
}
