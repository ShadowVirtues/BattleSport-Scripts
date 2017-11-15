using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //private GameObject ballSolidCollider;
    //private GameObject ballScoreCollider;

    //private Collider ballCollider;

    public enum GoalType { OneSided, TwoSided, FourSided }

    public GoalType goalType;

    void Awake()
    {
        //ballCollider = GetComponent<Collider>();
        //ballSolidCollider = transform.Find("GoalBallSolidCollider").gameObject;
        //ballScoreCollider = transform.Find("BallScoreCollider").gameObject;
    }


    //void OnCollisionEnter(Collision other)
    //{
    //    if (other.gameObject.layer == 16)   //BallTrigger layer. Even though BallCollider actually collides. This is because the layer of the parent rigidbody is returned
    //    {

            






    //        //ballCollider.enabled = false;
    //        //print("Disabled Collider");
    //    }

    //}


    //void OnCollisionExit(Collision other)
    //{
    //    //if (other.gameObject.layer == 17)
    //    //{
    //    //    ballScoreCollider.SetActive(true);
    //    //    print("Enabled Collider");
    //    //}

    //}


    //void OnTriggerEnter(Collider other)
    //{

    //    if (other.gameObject.layer == 16)
    //    {
    //        ballSolidCollider.SetActive(false);
    //        print("Disabled");
    //    }
    //}


    //void OnTriggerExit(Collider other)
    //{

    //    if (other.gameObject.layer == 16)
    //    {
    //        ballSolidCollider.SetActive(true);
    //        print("Enabled");
    //    }
    //}




}
