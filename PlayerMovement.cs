﻿using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Player player; //Reference to Player component
    private PlayerID playerNumber; //From TeamUtility InputManager "Add-on". Sets which player is getting controlled by this script and which controls to take from this InputManager

    public float acceleration;  //TODO tank-specific acceleration
    public float maxSpeed;      //TODO tank-specific top speed   
    public float dragCoeff;     //Coefficient of linear drag that all tanks stop from (like air friction) TODO make private

    private float rotationSpeed = 125; //Rotation speed when turning the tank with A-D or equivalent on gamepad
    private float jumpVelocity = 10; //Jump velocity applied to Y axis when pressing Jump button

    private new Rigidbody rigidbody;    //Caching rigidbody

    private bool grounded = false;      //Variable to check for if grounded to be able to jump

    private const string throttleAxisName = "Throttle";
    private const string strafingAxisName = "Strafing";    //Caching axis names for input
    private const string turningAxisName = "Turning";
    private const string jumpButtonName = "Jump";

    void Awake ()
    {
        player = GetComponent<Player>();
        playerNumber = player.PlayerNumber;
        rigidbody = GetComponent<Rigidbody>();     //Caching rigidbody        

#if UNITY_EDITOR
        //QualitySettings.vSyncCount = 0;  // Tests for different fps
        //Application.targetFrameRate = 45;
#endif
    }
   
    void Update()
    {
        float rotationY = InputManager.GetAxisRaw(turningAxisName, playerNumber) * rotationSpeed * Time.deltaTime * (0.005f / Time.deltaTime + 0.75f); //To rotate depending on input axis (rotation around Y axis)
        float tankRotation = transform.localEulerAngles.y + rotationY;      //Add the rotation to current rotation of the tank
                
        rigidbody.MoveRotation(Quaternion.Euler(0, tankRotation, 0));   //Apply this rotation (X=0, Z=0 constraint the tank to not tilt when hitting objects)     
        //Rotating with rigidbody.MoveRotation in Update, because this was the only was I could make rotating and moving the tank not have jitter (because of 50fps FixedUpdate and 144- fps Update)
        
        //Next commented line was making smooth rotation and smooth interpolated movement, but it would make rigidbody stop when rotating the tank
        //transform.Rotate(0, InputManager.GetAxisRaw(turningAxisName, playerNumber) * rotationSpeed * Time.deltaTime, 0); //CHECK THIS AFTER UNITY UPDATE IF IT STILL STOPS MOVING OF INTERPOLATED RIGIDBODY

    }

    void FixedUpdate () 
	{       
        float throttle = InputManager.GetAxisRaw(throttleAxisName, playerNumber) * acceleration;   //Get throttle input (W-S)
        float strafing = InputManager.GetAxisRaw(strafingAxisName, playerNumber) * acceleration;   //Get strafing input (Q-E) for now

        Vector3 velocity = new Vector3(strafing, 0, throttle);  //Make a velocity vector from input

	    Vector3 velocityGlobal = transform.TransformDirection(velocity);    //rigidbody.velocity vector is in global space, 'velocity' vector here is in local space, so transform it to global, relative to the player rotation

	    Vector3 vel = rigidbody.velocity;   //Cache it, cuz we use it a lot further

        //Player can reach the tank's top speed normally, and you shouldn't be able to accelerate more than that, but you can reach speeds more than top speed if the other player hits you with the rocket.
        //If you are thrown over the top speed, the efficiency of your acceleration should be 0 in the direction of where you are moving currently with the speed over top speed. 
        //And there should be full efficiency of acceleration in the opposite direction of this speed. So we calculate the 'dot product' of your 'rigidbody.velocity' vector and 'velocityGlobal' vector,
        //Which outputs 1 if you try to accelerate to the same direction your velocity is pointing to, 0 - if perpendicular, 1 - if opposite (values in between also exist).
        float dot = Vector2.Dot(new Vector2(vel.x, vel.z).normalized, new Vector2(velocityGlobal.x, velocityGlobal.z).normalized); //'normalized' cuz dot product is not normalized by default

	    if (magnitude2D(vel) > maxSpeed)    //Then if your velocity magnitude is higher than top speed
	    {
	        velocity = 0.5f * (-dot + 1) * velocity;    //Convert 'dot'-output, so it modifies the velocity to be 0 if you are trying to accelerate to the same direction of your speed, 0.5*acceleration if perpendicular to it, 1 if opposite
	    }
        
        rigidbody.AddRelativeForce(velocity, ForceMode.Acceleration); //Add force to your tank from the acceleration vector. 'Relative' because 'velocity' is also relative

	    //Applying 'linear drag' to the player by subtracting the constant coeficient from his speed, with 'normalized' coeficient to take into account the direction (example 0.7 speed to X direction, 0.3 to Z direction)
        if (sqrMagnitude2D(vel) > 0.03) //Since we are subtracting the speed, it may go over 0, so don't do that if the velocity.magnitude is higher than 0.001
            rigidbody.velocity = new Vector3(vel.x - normalizedX(vel) * Time.fixedDeltaTime * dragCoeff, vel.y, vel.z - normalizedZ(vel) * Time.fixedDeltaTime * dragCoeff);
        else
            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0); //If the velocity.magnitude^2 is lower than 0.03, set it to 0

        //print($"{rigidbody.velocity.x} {rigidbody.velocity.y} {rigidbody.velocity.z} {rigidbody.velocity.magnitude} {rigidbody.velocity.sqrMagnitude}");


        //if (grounded && InputManager.GetButton(jumpButtonName, PlayerNumber))    //We can jump only if we are on the ground
        //{
        //    rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);     //Just apply the Y velocity to jump      
        //}

        if (grounded)    //We can jump only if we are on the ground
	    {
	        if (playerNumber == PlayerID.One)   //TODO probably only for now player one jumps with set button, and player two hard-coded from here by pressing LB+RB on a gamepad
	        {
	            if (InputManager.GetButton(jumpButtonName, playerNumber))
	                rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);     //Just apply the Y velocity to jump  
            }                
            else if (playerNumber == PlayerID.Two)
            { 
                if (Input.GetKey(KeyCode.Joystick1Button4) && Input.GetKey(KeyCode.Joystick1Button5))                                  
                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);     //Just apply the Y velocity to jump  
                
            }
                
                    



        }

        
        grounded = false; //Set that we are not grounded every frame after jumping code so it will be overrided by collisions
        
    }


    void GroundCheck(Collision other)   //Checking if the player is on the ground  
    {
        foreach (ContactPoint contact in other.contacts)    //Takes all contact points from player collider collision    //OPTIMIZE generates garbage (only in the Editor???)
        {
            //if (contact.normal == Vector3.up)
            if (Vector3.Angle(contact.normal, Vector3.up) < 45) //If the contact to normal has angle lower than 45 degrees with UPWARDS direction means we are standing on something, and you can jump
            { 
                grounded = true;
                break;              //If we found that some contact has the right normal, no need to check other contacts
            }
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

    float magnitude2D(Vector3 vector) => Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);

    float sqrMagnitude2D(Vector3 vector) => vector.x * vector.x + vector.z * vector.z;















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
