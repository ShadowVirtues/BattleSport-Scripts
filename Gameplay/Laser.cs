using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private GameObject laserModel;    //Reference to laser model and explosion to disable-enable them when laser hits
    [SerializeField] private GameObject explosion;
    private readonly WaitForSeconds explosionTime = new WaitForSeconds(0.5f);   //Explosion duration

    public float FirePower; //Laser parameter for damage to apply. Gets set in PlayerShooting when pressing a laser button
    public int otherPlayerLayer;    //To decide in OnCollisionEnter if we should run public Hit function on other player

    private new Rigidbody rigidbody; //Cache rockets rigidbody to set its velocity to zero when the laser hits

    void Awake()    //Laser Prefab has all its components enabled by default, so all Awake caching runs at scene start and not when laser actually gets shot, this is so we dont get frame drop at first laser shot
    {
        rigidbody = GetComponent<Rigidbody>();  //Cache rockets rigidbody
        explosion.SetActive(false);
        gameObject.SetActive(false);        //And disable laser explosion and laser itself (they are pooled)
    }

    void OnEnable() //When we enable the laser to shoot it, start the countdown, so the laser fades after some time (0.5 sec)
    {
        StartCoroutine(nameof(LifeTime));
    }

    void OnCollisionEnter(Collision other)  //When the laser collides with something    
    {
        StopCoroutine(nameof(LifeTime));    //If we hit something, stop the countdown to fade the laser after some time

        if (other.gameObject.layer == otherPlayerLayer) //If the collided object is other player
        {
            if (otherPlayerLayer == 8)  //PlayerOne layer, then hit player one (getting the reference from GameController)
            {
                GameController.Controller.PlayerOne.Hit(FirePower / 6, Weapon.Laser);
            }
            else if (otherPlayerLayer == 9) //PlayerTwo layer
            {
                GameController.Controller.PlayerTwo.Hit(FirePower / 6, Weapon.Laser);
            }
            
        }

        //Since the laser bolt is the rigidbody with non-trigger collider, when OnCollisionEnter hits the bolt will have bounced off the surface it hit (actually this applies only if hit the player for some reason, probably because it's rigidbody)
        //That's why to enable the explosion in the proper place, we return the laser bolt to the position where it collided with the obstacle:
        transform.position = other.contacts[0].point;
        
        StartCoroutine(LaserExplosion());  //After colliding, disable laser model and make explosion, then disable the whole laser object for reusage
    }
    
    private readonly WaitForSeconds lifeTime = new WaitForSeconds(0.75f); //Laser life time

    IEnumerator LifeTime()
    {
        yield return lifeTime;  //Wait for lifetime 
        gameObject.SetActive(false);    //And disable the laser
    }

    IEnumerator LaserExplosion()
    {
        GameController.audioManager.LaserHit();    //Laser hits are not "Self-interrupting" so this will PlayOneShot them

        rigidbody.velocity = Vector3.zero;          //Since laser's collider is not trigger, on collision with something it will reflect, that's why we manually stop it
        rigidbody.angularVelocity = Vector3.zero;
        
        laserModel.SetActive(false);   //Disable laserbolt
        explosion.SetActive(true);      //Make explosion
        yield return explosionTime;     //Wait for it to end
        explosion.SetActive(false);     //Disable explosion
        laserModel.SetActive(true);    //Enable laser (for future reusage)
        gameObject.SetActive(false);    //Disable the laser so it can be used as another laser

    }

	
}
