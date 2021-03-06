using System;
using TeamUtility.IO;
using UnityEngine;

public partial class Const  //Auxiliary class to keep constants in (partial, because some stuff is valuable only for specific scripts in which the constants are defined)
{
    public const float MouseFactor = 0.6f;  //Factors to lead the "sensitivity" of stick and mouse to appropriate values
    public const float ButtonFactor = 1;
    public const float JoystickFactor = 4;
}

public class PlayerMovement : MonoBehaviour
{
    private Player player; //Reference to Player component
    private PlayerID playerNumber; //From TeamUtility InputManager "Add-on". Sets which player is getting controlled by this script and which controls to take from this InputManager

    private float acceleration;  //Acceleration to apply (gets calculated from tanks characteristic)
    private float maxSpeed;      //Same for Max Speed  
    private float initialSpeed; //Save initial speed, so we can get it back to it when SuperSpeed powerup expires
    private float dragCoeff = 15;     //Coefficient of linear drag that all tanks stop from (like air friction)

    private float rotationSpeed = 125; //Rotation speed when turning the tank with A-D or equivalent on gamepad
    private float jumpVelocity = 10; //Jump velocity applied to Y axis when pressing Jump button

    private new Rigidbody rigidbody;    //Caching rigidbody

    private bool grounded = false;      //Variable to check for if grounded to be able to jump
    public bool airbourne = false;    //Bool to play "He's Airbourne" once after player starts flying from picking up FlightSystem powerup

    private const string throttleAxisName = "Throttle";
    private const string strafingAxisName = "Strafing";    //Caching axis names for input
    private const string turningAxisName = "Turning";
    private const string jumpButtonName = "Jump";

    private const string LB = "LB";     //Those are for two-button jumping
    private const string RB = "RB";

    public bool jumpSingleButton;           //If true, jumping with single button specified in controls, depends on if this single button is bound, if not - two button controls that Joystick users use by default
    public float analogTurning = Const.ButtonFactor;           // = 1 if digital turning, = 0.6 or 4 if analog
    
    void Start()    //Again, no Awake, cuz no references
    {
        player = GetComponent<Player>();
        playerNumber = player.PlayerNumber;
        rigidbody = GetComponent<Rigidbody>();     //Caching rigidbody        

        Tank tank = GetComponentInChildren<Tank>();

        acceleration = tank.Acceleration * 0.41f + 8.6f;   //Calculate movement parameters from tank characteristics
        initialSpeed = tank.TopSpeed * 0.2f + 6;        //Saving initial speed. Why those formulas? Cuz.
        maxSpeed = initialSpeed;    //Setting the speed to initial

        AxisConfiguration jump = InputManager.GetAxisConfiguration(playerNumber, jumpButtonName);   //Check control configuration for the player to see if there is one- or two-button jumping
        if (jump.positive != KeyCode.None || jump.axis != 0)  //If some button is bound to "Jump", or axis for jump isn't set to default 0 (for default left trigger jumping scheme) then it's one-button jumping
        {
            jumpSingleButton = true;
        }

        AxisConfiguration device = InputManager.GetAxisConfiguration(playerNumber, "DEVICE");           //Get devide and turning axis configurations to set up sensitivity modifier
        AxisConfiguration turning = InputManager.GetAxisConfiguration(playerNumber, turningAxisName);

        //This is to lead the "sensitivity" of stick and mouse to appropriate values

        if (device.description == "Keyboard")   //If player device is Keyboard
        {
            analogTurning = Const.ButtonFactor;          //Set controls sensitivity modifier to 1
        }
        else if (device.description == "Keyboard+Mouse")
        {
            analogTurning = Const.MouseFactor;       //If mouse, it's 0.6
        }
        else
        {
            if (turning.axis == 5)  //D-Pad
            {
                analogTurning = Const.ButtonFactor;
            }
            else if (turning.axis == 0 || turning.axis == 3) //Left or right stick X axis
            {
                analogTurning = Const.JoystickFactor;      //4 for stick
            }
        }


#if UNITY_EDITOR
        //QualitySettings.vSyncCount = 0;  // Tests for different fps
        //Application.targetFrameRate = 30;
#endif
    }

    //private float accum = 0;
    //private bool isAxisInUse = false; //Stuff for testing turning speed
    //private float pressDownTime = 0;
    //private bool triggered = false;

    void Update()
    {
        if (Time.timeScale == 0) return;    //We don't want players to move during pause
        if (Time.deltaTime == 0) return;    //After unpausing, for one frame Time.deltaTime is still 0, which results in division by it in the next line, so don't run the function if that the case as well

        float rotationY = 0;    //Initializing the value, it then gets filled in 'if'

        if (analogTurning == 1) //If we are controlling with keyboard or d-pad, get only -1,0,1 values from the axis, so using Math.Sign for that
        {
            rotationY = Math.Sign(InputManager.GetAxis(turningAxisName, playerNumber)) * rotationSpeed * Time.deltaTime;     //To rotate depending on input axis (rotation around Y axis)
        }
        else    //For stick and mouse controls there are intermediate axis values, which we lead to appropriate values with "analogTurning"
        { 
            rotationY = InputManager.GetAxis(turningAxisName, playerNumber) * rotationSpeed * analogTurning * Time.deltaTime;  
        }
        
        float tankRotation = transform.localEulerAngles.y + rotationY * FPSModifier();      //Add the rotation to current rotation of the tank  

        rigidbody.MoveRotation(Quaternion.Euler(0, tankRotation, 0));   //Apply this rotation (X=0, Z=0 constraint the tank to not tilt when hitting objects)     
                                                                        //Rotating with rigidbody.MoveRotation in Update, because this was the only was I could make rotating and moving the tank not have jitter (because of 50fps FixedUpdate and 144-800 fps Update)

        //Next commented line was making smooth rotation and smooth interpolated movement, but it would make rigidbody stop when rotating the tank
        //transform.Rotate(0, InputManager.GetAxisRaw(turningAxisName, playerNumber) * rotationSpeed * Time.deltaTime, 0); //CHECK THIS AFTER UNITY UPDATE IF IT STILL STOPS MOVING OF INTERPOLATED RIGIDBODY. 2017.3 - NOPE

        //if (playerNumber == PlayerID.Two) print(rigidbody.velocity.magnitude);

        //================CODE FOR TESTING TURNING SPEED=============

        //float axis = InputManager.GetAxis(turningAxisName, PlayerID.Two);

        //if (axis != 0)  //If axis is actually pressed
        //{
        //    if (isAxisInUse == false) //Here we manage the initial button press (so we can still quick tap the button to switch)
        //    {                
        //        pressDownTime = Time.unscaledTime;  //Remember the time button got held down

        //        accum = transform.rotation.eulerAngles.y + 180;

        //        isAxisInUse = true; //Until we release the axis button, this will remain true
        //    }
        //    else if (triggered == false)
        //    {
        //        //accum += rotationY;
        //        if (transform.rotation.eulerAngles.y > accum)
        //        {
        //            Debug.LogError("Time: " + (Time.unscaledTime - pressDownTime) + ". FPS: " + (1.0f / Time.deltaTime));
        //            triggered = true;
        //        }
        //    }
            
        //}
        //if (axis == 0)  //And if the button is not pressed, or when it gets released
        //{           
        //    isAxisInUse = false;    //Set the flag so the next press will switch the item
        //    triggered = false;
        //    accum = 0;
        //}
        
    }

    float FPSModifier() //Hence our bullshit with setting  to rotate rigidbody in Update, turning speed would depend on FPS (good thing that linearly), so we multiply the turning speed by the factor, linearly depending on fps 
    {
        if (Time.deltaTime > 0.0088f)   //Below ~120 fps, the turning speed is ok, very noticeable for 300+ fps
        {
            return 1;
        }
        return 0.0016f * 1 / Time.deltaTime + 0.818f;   //Some BS formula, I know, but that's the linear dependence
    }

    void FixedUpdate () 
	{
        float throttle = InputManager.GetAxisRaw(throttleAxisName, playerNumber);   //Get throttle input (W-S)
	    float strafing = InputManager.GetAxisRaw(strafingAxisName, playerNumber);   //Get strafing input (Q-E)

	    Vector3 input = Vector3.ClampMagnitude(new Vector3(strafing, 0, throttle), 1);  //Make a vector from input, clamped to 1 (so tank doesn't accelerate faster diagonally

	    Vector3 inputVelocity = input * acceleration;  //Make a velocity vector from input

        Vector3 inputVelocityGlobal = transform.TransformDirection(inputVelocity);    //rigidbody.velocity vector is in global space, 'velocity' vector here is in local space, so transform it to global, relative to the player rotation

	    Vector3 vel = rigidbody.velocity;   //Cache current tank velocity, cuz we use it a lot further

        //Player can reach the tank's top speed normally, and you shouldn't be able to accelerate more than that, but you can reach speeds more than top speed if the other player hits you with the rocket.
        //If you are thrown over the top speed, the efficiency of your acceleration should be 0 in the direction of where you are moving currently with the speed over top speed. 
        //And there should be full efficiency of acceleration in the opposite direction of this speed. So we calculate the 'dot product' of your 'rigidbody.velocity' vector and 'velocityGlobal' vector,
        //Which outputs 1 if you try to accelerate to the same direction your velocity is pointing to, 0 - if perpendicular, 1 - if opposite (values in between also exist).
        
	    if (magnitude2D(vel) > maxSpeed)    //Then if your velocity magnitude is higher than top speed
	    {
	        float dot = Vector2.Dot(new Vector2(vel.x, vel.z).normalized, new Vector2(inputVelocityGlobal.x, inputVelocityGlobal.z).normalized); //'normalized' cuz dot product is not normalized by default
            inputVelocity = 0.5f * (-dot + 1) * inputVelocity;    //Convert 'dot'-output, so it modifies the velocity to be 0 if you are trying to accelerate to the same direction of your speed, 0.5*acceleration if perpendicular to it, 1 if opposite
	    }
        
        rigidbody.AddRelativeForce(inputVelocity, ForceMode.Acceleration); //Add force to your tank from the acceleration vector. 'Relative' because 'velocity' is also relative

	    //Applying 'linear drag' to the player by subtracting the constant coeficient from his speed, with 'normalized' coeficient to take into account the direction (example 0.7 speed to X direction, 0.3 to Z direction)
        if (sqrMagnitude2D(vel) > 0.03) //Since we are subtracting the speed, it may go over 0, so don't do that if the velocity.magnitude is higher than 0.001
            rigidbody.velocity = new Vector3(vel.x - normalizedX(vel) * Time.fixedDeltaTime * dragCoeff, vel.y, vel.z - normalizedZ(vel) * Time.fixedDeltaTime * dragCoeff);
        else
            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0); //If the velocity.magnitude^2 is lower than 0.03, set it to 0
        
	    if (player.powerup.FlightSystem)    //If the player has FlightSystem powerup
	    {
	        if (jumpSingleButton)   //If "Jump" button is defined - use it
	        {
	            if (InputManager.GetButton(jumpButtonName, playerNumber))
	            {
	                if (rigidbody.velocity.y <= 0)  //Player can engage flying only at the moments he is falling down from gravity
	                {
	                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);    //Set vertical velocity of player to 0, making him hover
	                    if (airbourne == false) //If the bool is false, play "He's Airbourne" and set it to true so for this powerup duration it doesn't get spelled
	                    {
	                        GameController.audioManager.HesAirbourne();
	                        airbourne = true;
	                    }
	                    rigidbody.useGravity = false;   //Without disabling gravity, zeroing vertical velocity would still drag the player down a bit
	                }
                    else
                    {
                        rigidbody.useGravity = true;    //If player is not falling, apply gravity
                    }
                }
	            else        //If player is not holding jump button while flying, also apply gravity
	            {
	                rigidbody.useGravity = true;
	            }                                   //If the player was flying when the powerup ended, we make sure gravity gets enabled back in the 'ActionOut' of expiring FlightSystem powerup
            }
	        else    //Otherwise use LB+RB (can be overridden manually editing control config XML if needed)
	        {
	            if (InputManager.GetButton(LB, playerNumber) && InputManager.GetButton(RB, playerNumber))   //Same stuff for two-button controls
	            {
	                if (rigidbody.velocity.y <= 0)
	                {
	                    rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
	                    if (airbourne == false)
	                    {
	                        GameController.audioManager.HesAirbourne();
	                        airbourne = true;
	                    }
                        rigidbody.useGravity = false;
	                }
                    else
                    {
                        rigidbody.useGravity = true;
                    }
                }
                else
                {
                    rigidbody.useGravity = true;
                }

            }

        }


        if (grounded)    //We can jump only if we are on the ground
	    {
	        if (jumpSingleButton)   //If "Jump" button is defined - use it
	        {
	            if (InputManager.GetButton(jumpButtonName, playerNumber))
	            {
	                rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);     //Just apply the Y velocity to jump  
                }	                
            }
	        else    //Otherwise use LB+RB (can be overridden manually editing control config XML if needed)
	        {
	            if (InputManager.GetButton(LB, playerNumber) && InputManager.GetButton(RB, playerNumber))
	            {
	                rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);     //Just apply the Y velocity to jump  
                }
	               
            }
            
        }

        
        grounded = false; //Set that we are not grounded every frame after jumping code so it will be overridden by collisions
        
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
    

    public void SetSuperSpeed(bool set) //Setting SuperSpeed like this, if bool is true - setting, if false - unsetting
    {
        if (set)
        {
            maxSpeed = initialSpeed * 2;
        }
        else
        {
            maxSpeed = initialSpeed;
        }
    }

    public void SetEverythingBack()
    {
        SetSuperSpeed(false);   //Set speed back
        rigidbody.useGravity = true;    //Enable gravity
    }

    //=======================HELPER FUNCTIONS===========================


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

    
}
