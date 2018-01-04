using UnityEngine;

public class Ramp : MonoBehaviour
{   
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != 16) //Doesn't apply for the ball
        {
            other.rigidbody.useGravity = false;
        }
            
    }

    void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer != 16)
        {
            other.rigidbody.useGravity = true;
        }
           
    }

}
