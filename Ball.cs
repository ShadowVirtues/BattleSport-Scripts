using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    private new Rigidbody rigidbody;


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

    }

    void Start()
    {
        rigidbody.maxAngularVelocity = 200;
        //rigidbody.AddForce(Vector3.forward * 1000);


    }


    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Joystick1Button2))
    //    {

    //        rigidbody.rotation = Quaternion.LookRotation(GameController.Controller.playerTwo.transform.TransformDirection(Vector3.forward));
    //        rigidbody.angularVelocity = GameController.Controller.playerTwo.transform.TransformDirection(new Vector3(20, 0, 0));

    //    }


    //}

    private bool possession;
    private bool firstPlayerPossessed;
    private bool secondPlayerPossessed;

    private void ballPossess(Collider other)
    {
        other.gameObject.GetComponentInParent<Player>().Possession();

        possession = true;

        rigidbody.velocity = Vector3.zero;

        rigidbody.useGravity = false;
        transform.position = new Vector3(0, -20, 2);
        transform.rotation = Quaternion.identity;
        rigidbody.angularVelocity = Vector3.up * -12;


        //gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {       
        if (other.gameObject.layer == 19)   //GoalScore
        {
            if (possession && firstPlayerPossessed)
            {
                //TODO Score player One
            }
            else if (possession && secondPlayerPossessed)
            {
                //TODO Score player Two
            }

        }
        else if (other.gameObject.layer == 8) //Player One
        {
            if (possession && secondPlayerPossessed)
            {
                GameController.announcer.Interception();
            }
            else
            {
                GameController.announcer.Possession();
            }
            firstPlayerPossessed = true;
            ballPossess(other);
            
        }
        else if (other.gameObject.layer == 9) //Player Two
        {
            if (possession && firstPlayerPossessed)
            {
                GameController.announcer.Interception();
            }
            else
            {
                GameController.announcer.Possession();
            }
            secondPlayerPossessed = true;
            ballPossess(other);


        }

    }

    private void losePossession()
    {
        possession = false;
        firstPlayerPossessed = false;
        secondPlayerPossessed = false;
    }

    void OnCollisionEnter(Collision other)
    {
        if (possession)
        {
            if (other.gameObject.layer == 13)   //If hit level geometry after shooting
            {
                GameController.announcer.MissClose();
                //TODO MISS

                losePossession();
            }
            else if (other.gameObject.layer == 18) //If we hit the solid part of the goal
            {
                GameController.announcer.Rejected();
                //TODO REJECTED

                losePossession();
            }
            



            

        }



    }




}
