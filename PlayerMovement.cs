using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;





/*
    
    Consider that we have default Physic Material and there needs to be ball bouncing out of everything     
    
         
         
         
*/





public class PlayerMovement : MonoBehaviour
{

    private float rotationSpeed = 2; //Rotation speed when turning the tank with A-D or equivalent on gamepad
    public float acceleration;  //TODO tank-specific acceleration
    public float maxSpeed;      //TODO tank-specific top speed
    private float jumpVelocity = 10; //Jump velocity applied to Y axis when pressing Jump button
    public float rocketHitForce;//TODO maybe different for tanks parameter "firepower"
    public float dragCoeff;     //Coefficient of linear drag that all tanks stop from (like air friction) TODO make private

    private new Rigidbody rigidbody;    //Caching rigidbody

    private bool grounded = false;      //Variable to check for if grounded to be able to jump
      
    void Awake ()
    {
        rigidbody = GetComponent<Rigidbody>();     //Caching rigidbody
    }
   
    float normalizedX(Vector3 vector) //Auxiliary function to get normalized X component of a vector when taking into account only X and Z components. Because Jumping speed is independent of speed in X and Z directions of a tank
    {
        float magnigude2D = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);  //Get magnitude of quasi-Vector2 from X and Z components of Vector3

        if (magnigude2D != 0)   //If X and Z components are 0, normalization would return 0/0=NaN, in this case the normalized component is 0.
            return vector.x / magnigude2D;
        else
            return 0;
    }

    float normalizedZ(Vector3 vector)   //Auxiliary function to get normalized Z component of a vector when taking into account only X and Z components
    {
        float magnigude2D = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);

        if (magnigude2D != 0)
            return vector.z / magnigude2D;
        else
            return 0;
    }
    
    void FixedUpdate ()
	{
	    float rotationY = Input.GetAxisRaw("Horizontal") * rotationSpeed; //To rotate depending on input axis (rotation around Y axis)
	    float tankRotation = transform.localEulerAngles.y + rotationY;      //Add the rotation to current rotation of the tank
	    transform.localEulerAngles = new Vector3(0, tankRotation, 0);   //Apply this rotation (X=0, Z=0 constraint the tank to not tilt when hitting objects)

        //transform.Rotate(0, Input.GetAxisRaw("Horizontal") * rotationSpeed, 0);

        //rigidbody.rotation = rotationBeforePhysics;

        //rotationBeforePhysics = rigidbody.rotation;

        //KOSTIL' LOOK AT child transform of the tank

        //transform.rotation = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward), Vector3.up);

        //Vector3 eulerAngles = transform.rotation.eulerAngles;
        //eulerAngles = new Vector3(0, eulerAngles.y, 0);
        //transform.rotation = Quaternion.Euler(eulerAngles);

        //////////////////////

        float throttle = Input.GetAxisRaw("Vertical") * acceleration;   //Get throttle input (W-S)
        float strafing = Input.GetAxisRaw("Strafing") * acceleration;   //Get strafing input (Q-E) for now

        Vector3 velocity = new Vector3(strafing, 0, throttle);  //Make a velocity vector from input

	    Vector3 velocityGlobal = transform.TransformDirection(velocity);    //rigidbody.velocity vector is in global space, 'velocity' vector here is in local space, so transform it to global, relative to the player rotation

	    Vector3 vel = rigidbody.velocity;   //Cache it, cuz we use it a lot further

        //Player can reach the tank's top speed normally, and you shouldn't be able to accelerate more than that, but you can reach speeds more than top speed if the other player hits you with the rocket.
        //If you are thrown over the top speed, the efficiency of your acceleration should be 0 in the direction of where you are moving currently with the speed over top speed. 
        //And there should be full efficiency of acceleration in the opposite direction of this speed. So we calculate the 'dot product' of your 'rigidbody.velocity' vector and 'velocityGlobal' vector,
        //Which outputs 1 if you try to accelerate to the same direction your velocity is pointing to, 0 - if perpendicular, 1 - if opposite (values in between also exist).
        float dot = Vector2.Dot(new Vector2(vel.x, vel.z).normalized, new Vector2(velocityGlobal.x, velocityGlobal.z).normalized); //'normalized' cuz dot product is not normalized by default

	    if (vel.magnitude > maxSpeed)    //Then if your velocity magnitude is higher than top speed
	    {
	        velocity = 0.5f * (-dot + 1) * velocity;    //Convert 'dot'-output, so it modifies the velocity to be 0 if you are trying to accelerate to the same direction of your speed, 0.5*acceleration if perpendicular to it, 1 if opposite
	    }
        
        rigidbody.AddRelativeForce(velocity, ForceMode.Acceleration); //Add force to your tank from the acceleration vector. 'Relative' because 'velocity' is also relative

        //rigidbody.velocity = ClampMagnitude2D(rigidbody.velocity, moveSpeed);

	    //print(rigidbody.velocity + "   " + rigidbody.velocity.magnitude + "    " + velocity);


	    //Applying 'linear drag' to the player by subtracting the constant coeficient from his speed, with 'normalized' coeficient to take into account the direction (example 0.7 speed to X direction, 0.3 to Z direction)
        if (vel.sqrMagnitude > 0.001) //Since we are subtracting the speed, it may go over 0, so don't do that if the velocity.magnitude is higher than 0.001
            rigidbody.velocity = new Vector3(vel.x - normalizedX(vel) * Time.fixedDeltaTime * dragCoeff, vel.y, vel.z - normalizedZ(vel) * Time.fixedDeltaTime * dragCoeff);
        else
            rigidbody.velocity = Vector3.zero; //If the velocity.magnitude is lower than 0.001, set it to 0
	    
        if (grounded && Input.GetButton("Jump"))    //We can jump only if we are on the ground
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);     //Just apply the Y velocity to jump      
        }
        grounded = false; //Set that we are not grounded every frame after jumping code so it will be overrided by collisions


        if (Input.GetKeyDown(KeyCode.C))    //Simulation of rocket hit
        {
            rigidbody.AddRelativeForce(rocketHitForce * Vector3.forward, ForceMode.VelocityChange);
        }

    }


    void GroundCheck(Collision other)   //Checking if the player is on the ground
    {
        foreach (ContactPoint contact in other.contacts)    //Takes all contact points from player collider collision
        {
            //if (contact.normal == Vector3.up)
            if (Vector3.Angle(contact.normal, Vector3.up) < 45) //If the contact to normal has angle lower than 45 degrees with UPWARDS direction means we are standing on something, and you can jump
                grounded = true;

            //Debug.DrawRay(contact.point, contact.normal, Color.white);
            //print(Vector3.Angle(contact.normal, Vector3.up) + "    " + grounded);
        }
    }


    void OnCollisionEnter(Collision other) //Check ground both at the frame when entered collider and stayed on the collider
    {
        GroundCheck(other);
    }



    void OnCollisionStay(Collision other)  //Check ground both at the frame when entered collider and stayed on the collider
    {
        GroundCheck(other);
    }






















    //Vector3 ClampMagnitude2D(Vector3 vector, float maxLength)   //Not needed now, to clamp only the magnitude along X and Z components
    //{
    //    Vector3 result;
    //    if (vector.x * vector.x + vector.z * vector.z > maxLength * maxLength)
    //    {
    //        float magnigude2D = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);

    //        result.x = vector.x / magnigude2D * maxLength;
    //        result.z = vector.z / magnigude2D * maxLength;
    //        result.y = vector.y;
    //    }
    //    else
    //    {
    //        result = vector;
    //    }
    //    return result;
    //}
}
