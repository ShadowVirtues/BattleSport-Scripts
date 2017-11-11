using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private GameObject laserModel;    //Reference to rocket model and explosion to disable-enable them when rocket explodes
    [SerializeField] private GameObject explosion;
    private readonly WaitForSeconds explosionTime = new WaitForSeconds(0.5f);   //Explosion duration

    public float FirePower; //Rocket parameter for damage and knockback to apply. Gets set in PlayerShooting when pressing a rocket button
    public int otherPlayerLayer;    //To decide in OnCollisionEnter if we should apply force to hit object (Other player)

    private new Rigidbody rigidbody; //Cache rockets rigidbody to get its velocity vector

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();  //Cache rockets rigidbody to get its velocity vector
    }

    void OnCollisionEnter(Collision other)  //When the rocket collides with something
    //void OnTriggerEnter(Collider other)  //When the rocket collides with something
    {
        StopCoroutine(nameof(LifeTime));

        if (other.gameObject.layer == otherPlayerLayer) //If the collided object is other player
        {
            

            float enemyArmor = other.gameObject.GetComponentInChildren<Tank>().armor; //TODO Maybe switch damage calculations in actual Player script (just send Hit(FirePower))
            float damage = (160 - enemyArmor) / 250 * FirePower;
            other.gameObject.GetComponent<Player>().Hit(damage / 6);    //TODO For now it's like this. In future, we will maybe have a static PlayerOne/Two reference so we can easily get their fields


        }

        transform.position = other.contacts[0].point - transform.TransformDirection(Vector3.forward * 0.2f);

        
        
        //TODO disappear over time



        StartCoroutine(LaserExplosion());  //After colliding, disable rocket model and make explosion, then disable the whole rocket object for reusage
    }

    void OnEnable()
    {
        StartCoroutine(nameof(LifeTime));
        
    }

    private WaitForSeconds lifeTime = new WaitForSeconds(0.5f);

    IEnumerator LifeTime()
    {
        yield return lifeTime;
        gameObject.SetActive(false);
    }

    IEnumerator LaserExplosion()
    {
        

        rigidbody.velocity = Vector3.zero;          //Since rocket's collider is not trigger (cuz we need collision normal), on collision with something it will reflect, that's why we manually stop it
        rigidbody.angularVelocity = Vector3.zero;


        laserModel.SetActive(false);   //Disable rocket
        explosion.SetActive(true);      //Make explosion
        yield return explosionTime;     //Wait for it to end
        explosion.SetActive(false);     //Disable explosion
        laserModel.SetActive(true);    //Enable rocket (for future reusage)
        gameObject.SetActive(false);    //Disable the rocket so it can be used as another rocket

    }

	
}
