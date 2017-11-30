using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Goal : MonoBehaviour
{    
    public enum GoalType { OneSided, TwoSided, FourSided }  //GoalType to decide score/reject detection by normals when the ball collides with the goal

    public GoalType goalType;   //Variable for each goal

    [HideInInspector] public Collider ballSolidCollider;    //When the ball hits score-registering parts of the goal, we need to pass that ball through the goal, that' why we disable goal's collider for the ball (collider for the player still works)

    private Material goalMaterial;  //To flash the goal on score

    [HideInInspector] public Transform ballCollider; //Reference to child object of goal "BallCollider", which changes its position for the moving goal (the parent with this script remains at the same place)
    //We need it in PlayerRadar.cs for position, in Ball.cs to get where it is facing and to calculate misses, in Player.cs to get how far from the goal the player shot the ball

    private const string emissionColor = "_EmissionColor";    //Caching those strings for no garbage
    private const string emissionKeyword = "_EMISSION";

    void Awake()
    {
        ballCollider = transform.Find("BallCollider");  //Get the child object

        ballSolidCollider = ballCollider.GetComponent<Collider>();       //Getting dose references  
        goalMaterial = ballCollider.GetComponent<Renderer>().material;  //(Parent goal object has only the script component attached)
    }

    public void FlashGoalOnScore()
    {
        goalMaterial.EnableKeyword(emissionKeyword);    //We enable and disable emission in hopes that it actually improves performance

        float init = 0.1f;
        Color initial = new Color(init, init, init);
        goalMaterial.SetColor(emissionColor, initial);   //Set "low" flash color before proceeding to flash the goal

        float fin = 0.4f;
        Color final = new Color(fin, fin, fin);             //This is "high" flash color
        goalMaterial.DOColor(final, emissionColor, 0.15f).SetLoops(8, LoopType.Yoyo).OnComplete(() => { goalMaterial.DisableKeyword(emissionKeyword); });    //Flash the goal 4 times (one time forth and one time back X4), in the end disable emission
    }

    //==================================

    public bool teleporting = false;    //Script component has a bool variable in the inspector if the goal is teleporting on the arena

    void Start()
    {
        if (teleporting) StartCoroutine(Teleporting()); //So when it's ticked when the round starts, the goal starts teleporting
    }

    private readonly WaitForSeconds teleportDelay = new WaitForSeconds(9);  //Goal teleports every 10 seconds, 9 seconds nothing happens, then over 1 second it fades out and appears somewhere else
    private readonly WaitForSeconds fadeDelay = new WaitForSeconds(1);

    IEnumerator Teleporting()   //A sequence of teleporting
    {
        while (teleporting) //Not sure if this is needed, maybe will be useful in period switching idk
        {
            yield return teleportDelay; //Wait 9 seconds
            GameController.audioManager.Cloak();   //Play the sound of cloaking
            goalMaterial.DOFade(0, 1);          //Start fading tween over 1 second
            yield return fadeDelay;             //Wait one second
            goalMaterial.color = Color.white;   //Set the color back to opaque
            Vector2 rand = FindRandomPosition();    //Launch function to find random position on the map where we could insert the goal

            transform.position = new Vector3(rand.x, transform.position.y, rand.y); //Place the goal to this position on it's initial height (Y component)

        }

    }

    private Collider[] spawnCheckColliders = new Collider[2];   //Array where raycast would yield the results

    Vector3 FindRandomPosition()    //Function to find random position on the map where we could insert the goal (pretty much the same as for when we respawn the player)
    {
        //The algorithm is checking the cylinder from the ground to the highest point of the arena where there is some object in the random X-Z point 
        //of the arena if this cylinder has something but the floor in it, if it has, then find another point where there is only a floor in the cylinder
        //We use OverlapCapsuleNonAlloc for this, which requires the Collider[] array to store found colliders in this cylinder being checked

        int offset = 4;             //Offset from the arena border so goal doesn't spawn right next to a wall
        float levelDimension = GameController.Controller.arenaDimension / 2;  //Get arena dimension (total X=Y length) from GameController, to convert it into max coordinate need to divide by 2

        int iter = 0;   //A way to stop the infinite loop of finding the random spawn point if we can't find it

        while (true)    //There is the infinite loop
        {
            Array.Clear(spawnCheckColliders, 0, spawnCheckColliders.Length);    //Clear the array of colliders just in case before performing the next check 

            Vector2 coord = new Vector2(Random.Range(-levelDimension + offset, levelDimension - offset), Random.Range(-levelDimension + offset, levelDimension - offset)); //Get random point of the map with RNG
            //Capsule starts from Y=0 (floor) to 10 (highest point where arena objects are) TODO increase this when made the highest arena          
            Physics.OverlapCapsuleNonAlloc(new Vector3(coord.x, 0, coord.y), new Vector3(coord.x, 10, coord.y), 2, spawnCheckColliders, ~0, QueryTriggerInteraction.Ignore);   
            for (int i = 0; i < 2; i++) //The spawnCheckColliders array has only the length of 2, because the condition when there is only the floor found is when its the first collider found and the second collider is null
            {                           //If the first collider isn't the floor, or second collider isn't the floor as well, then its the wrong condition
                if (spawnCheckColliders[0].gameObject.layer == 14 && spawnCheckColliders[1] == null)
                {
                    return new Vector2(coord.x, coord.y);    //Return position to spawn goal at
                }
            }
            iter++;
            if (iter > 100)
            {
                Debug.LogError("Couldn't find the spawn site"); //If we performed 100 checks and haven't found a spawn site, there must be something wrong
                return new Vector2(0, 0);   //Spawn the goal in the middle if there is an error finding
            }


        }
    }




}
