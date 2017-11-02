using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Armor will mean player mass which also gets calculated in knock force


public class Rocket : MonoBehaviour
{
    
    public float FirePower; //Rocket parameter for damage and knockback to apply. Gets set in PlayerShooting when pressing a rocket button

    public int otherPlayerLayer;    //To decide in OnCollisionEnter if we should apply force to hit object (Other player)

    private float forceCoeff = 0.2f;    //Coefficient to convert FirePower to Force

    //The knockback vector is getting combined from
    private float normalForceRatio = 0.3f;   //The normal to the tank collider hit 
    private float upForceRatio = 0.3f;      //The knockup force
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

        }
        


        gameObject.SetActive(false); //Disable the rocket so it can be used as another rocket
    }

    

	
}
