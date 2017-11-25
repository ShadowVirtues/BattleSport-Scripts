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

    [HideInInspector] public Transform ballCollider; //COMM

    void Awake()
    {
        ballCollider = transform.Find("BallCollider");

        ballSolidCollider = ballCollider.GetComponent<Collider>();       //Getting dose references  //COMM
        goalMaterial = ballCollider.GetComponent<Renderer>().material;
    }

    public void FlashGoalOnScore()
    {
        goalMaterial.EnableKeyword("_EMISSION");    //We enable and disable emission in hopes that it actually improves performance

        float init = 0.1f;
        Color initial = new Color(init, init, init);
        goalMaterial.SetColor("_EmissionColor", initial);   //Set "low" flash color before proceeding to flash the goal

        float fin = 0.4f;
        Color final = new Color(fin, fin, fin);             //This is "high" flash color
        goalMaterial.DOColor(final, "_EmissionColor", 0.15f).SetLoops(8, LoopType.Yoyo).OnComplete(() => { goalMaterial.DisableKeyword("_EMISSION"); });    //Flash the goal 4 times (one time forth and one time back X4), in the end disable emission
    }

    //==================================

    public bool teleporting = false;

    void Start()
    {
        if (teleporting) StartCoroutine(Teleporting());

    }

    private readonly WaitForSeconds teleportDelay = new WaitForSeconds(9);
    private readonly WaitForSeconds fadeDelay = new WaitForSeconds(1);

    IEnumerator Teleporting()
    {
        while (teleporting)
        {
            yield return teleportDelay;
            GameController.announcer.Cloak();
            goalMaterial.DOFade(0, 1);
            yield return fadeDelay;
            goalMaterial.color = Color.white;

            Vector2 rand = FindRandomPosition();

            transform.position = new Vector3(rand.x, transform.position.y, rand.y);



        }



    }

    private Collider[] spawnCheckColliders = new Collider[2];

    Vector3 FindRandomPosition()
    {
        int offset = 4;             //Offset from the arena border so player doesn't spawn right next to a wall
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
                    return new Vector2(coord.x, coord.y);    //Return position to spawn tank at. Y=5 is the height from where the tank drops
                }
            }
            iter++;
            if (iter > 100)
            {
                Debug.LogError("Couldn't find the spawn site"); //If we performed 100 checks and haven't found a spawn site, there must be something wrong
                return new Vector2(0, 0);
            }


        }
    }




}
