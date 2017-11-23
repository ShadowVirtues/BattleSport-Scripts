using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ramp : MonoBehaviour
{   
    void OnCollisionEnter(Collision other)
    {
        other.rigidbody.useGravity = false;       
    }

    void OnCollisionExit(Collision other)
    {
        other.rigidbody.useGravity = true;       
    }

}
