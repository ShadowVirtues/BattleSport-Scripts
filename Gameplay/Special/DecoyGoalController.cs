using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script that gets attached to the goal object in the arena, handles bogus/decoy goals
public class DecoyGoalController : MonoBehaviour
{
    [SerializeField] private bool random;   //If goals are gonna spawn in completely random places and move into random directions

    [SerializeField] private float spawnHeight;     //The height to spawn goals at (only for random)

    [Header("Not Random")]          //Fields for not random goal placements
    [SerializeField] private Transform[] guides;    //Guides empty transforms that define the lines along which goals move (may be multiple lines)
    [SerializeField] private float length;          //Length of the line in each direction to spawn goals along (if length is 30, means the goals are gonna spawn along 60-unit long line with guide transform position in the middle of that 60-unit long line)

    [Header("General")] //Settings for both random and not random goals
    
    [SerializeField] private float speed;       //Goal moving speed
    [SerializeField] private int decoyGoalCount;    //Count of decoy goals

    private DecoyGoal[] decoyGoals;             //Arrays in which we pool all decoy goal GameObjects an their DecoyGoal components
    private GameObject[] decoyGoalObjects;

    private Collider[] spawnCheckColliders = new Collider[2];   //Usual array for capsule cast

    [SerializeField] private GameObject decoyGoalPrefab;        //Prefab of the decoy goal to spawn

    void Start()
    {
        GameObject decoyGoalContainer = new GameObject("DecoyGoals");   //Make a container for pooled goals so they are not all on top of hierarchy

        decoyGoals = new DecoyGoal[decoyGoalCount];             //Initialize arrays
        decoyGoalObjects = new GameObject[decoyGoalCount];

        for (int i = 0; i < decoyGoalCount; i++)    //For whe whole count of decoy goals
        {
            decoyGoalObjects[i] = Instantiate(decoyGoalPrefab, decoyGoalContainer.transform);   //Insatantiate goal into container and fill it into array
            decoyGoals[i] = decoyGoalObjects[i].GetComponent<DecoyGoal>();      //Into other array fill DecoyGoal component of the spawned goal

            if (random) //If random goal placement
            {
                RandomizeGoal(i);   //Put a goal in random spot with random movement vector

                decoyGoals[i].speed = speed;    //Set the movement speed of the goal
            }
            else    //Otherwise, goal movement along defined lines
            {
                int randomGuide = Random.Range(0, guides.Length);   //Pick random guide number (if one guide, it will obviously be picked for all goals)
                int randomDirection = Random.Range(0, 2) * 2 - 1;   //Way of generating either "-1" or "1" number for random direction of goal movement
                float randomOnLength = Random.Range(-length, length);     //Generate a number on a length of the guide

                decoyGoalObjects[i].transform.position = guides[randomGuide].TransformPoint(Vector3.forward * randomOnLength);  //Put a goal in position on a line of a guide
                decoyGoals[i].speed = speed;    //Set goal movement speed

                Vector3 movement = guides[randomGuide].TransformDirection(Vector3.forward * randomDirection);   //Generate decoy goal movement vector to either of two directions
                
                decoyGoals[i].movement = movement.normalized;   //Set it in the DecoyGoal component, vector should be generated normalized initially, normalizing again just in case
                
            }

        }
        
        Messenger.AddListener(GameController.SetEverythingBackMessage, SetEverythingBack);  //Add listener so the function here can run from global message of setting everything back 
        //Tthis is done for all objects we don't have references to in GameController

    }

    private void RandomizeGoal(int i)   //We use the same code in 3 places, so make it into function
    {
        decoyGoalObjects[i].transform.position = GameController.FindRandomPosition(10, 2, spawnHeight, spawnCheckColliders);    //Use our badass function to find the position for goal to spawn       

        Vector3 movement = Random.onUnitSphere; //Method of generating the random direction on a circle. Unity doesn't have a specific function for it, so we use existing ones. First generate the random point on a sphere surface
        movement.y = 0; //Set Y component to 1
        decoyGoals[i].movement = movement.normalized;//Since generated point on a sphere was normalized, zeroing the Y component will make the the magnitude less than 0, so normalize it back and assign to movement direction of the decoy goal
    }

    public void Scored()    //Function that runs from Goal.cs when scoring, so we get back all goals that got disabled from throwing ball into them
    {       
        //TODO Return them back after testing
        for (int i = 0; i < decoyGoalCount; i++)    //For all goals, check if they for whatever reason got out of bounds (their reflecting off arena bounds isn't 100% stable)
        {
            if (decoyGoalObjects[i].transform.position.x > GameController.Controller.ArenaDimension / 2 ||
                decoyGoalObjects[i].transform.position.z > GameController.Controller.ArenaDimension / 2 ||
                decoyGoalObjects[i].transform.position.x < -GameController.Controller.ArenaDimension / 2 ||
                decoyGoalObjects[i].transform.position.z < -GameController.Controller.ArenaDimension / 2)
            {
                Debug.LogError($"DECOY GOAL {i} IS OUT OF BOUNDS!!!");  //Log error, if that happened, indicating the goal number that it happened with
                Debug.Break();
            }
        }
        
        if (random) //If random goal spawning
        {
            for (int i = 0; i < decoyGoalCount; i++)
            {
                if (decoyGoalObjects[i].activeSelf == false)    //Only do something for disabled goals
                {
                    RandomizeGoal(i);    //Put a goal in random spot with random movement vector
                    
                    decoyGoalObjects[i].SetActive(true);    //Finally enable the goal back
                }
            }
        }
        else    //If goals-on-lines
        {
            for (int i = 0; i < decoyGoalCount; i++)
            {               
                decoyGoalObjects[i].SetActive(true);            //Just enable back all decoy goals      
            }
        }

    }
    
    void OnDestroy()
    {
        Messenger.RemoveListener(GameController.SetEverythingBackMessage, SetEverythingBack);   //Have to remove listener, so no errors
    }

    private void SetEverythingBack()    //Setting everything back on period end
    {
        for (int i = 0; i < decoyGoalCount; i++)
        {         
            if (random)     //If random spawning, then for all goals, generate random positions again
            {
                RandomizeGoal(i);  
            }          

            decoyGoalObjects[i].SetActive(true);    //Enable all goals, even if not random
        }

    }


}