using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Goal : MonoBehaviour
{    
    public enum GoalType { OneSided, TwoSided, FourSided, TwoSidedPhantom }  //GoalType to decide score/reject detection by normals when the ball collides with the goal

    public GoalType goalType;   //Variable for each goal

    public Sprite Icon;         //Icon for countdown panel

    [HideInInspector] public Collider ballSolidCollider;    //When the ball hits score-registering parts of the goal, we need to pass that ball through the goal, that' why we disable goal's collider for the ball (collider for the player still works)

    private Material goalMaterial;  //To flash the goal on score

    private DecoyGoalController decoyGoal;  //References to DecoyGoalController and DecoyBallController if they exist
    private DecoyBallController decoyBall;

    [HideInInspector] public Transform ballCollider; //Reference to child object of goal "BallCollider", which changes its position for the moving goal (the parent with this script remains at the same place)
    //We need it in PlayerRadar.cs for position, in Ball.cs to get where it is facing and to calculate misses, in Player.cs to get how far from the goal the player shot the ball

    private const string emissionColor = "_EmissionColor";    //Caching those strings for no garbage
    private const string emissionKeyword = "_EMISSION";

    void Awake()    //Those all depend on actual goal which is already in the scene from the start, so no errors with objects not yet instantiated
    {
        ballCollider = transform.Find("BallCollider");  //Get the child object

        ballSolidCollider = ballCollider.GetComponent<Collider>();       //Getting dose references  
        goalMaterial = ballCollider.GetComponent<Renderer>().material;  //(Parent goal object has only the script component attached)

        decoyGoal = GetComponent<DecoyGoalController>();  //Try getting those if they exist
        decoyBall = GetComponent<DecoyBallController>();  
    }

    
    public void FlashGoalOnScore()
    {
        if (decoyGoal != null) decoyGoal.Scored();  //If DecoyBallController or DecoyGoal controller exist on the goal object, then launch the function when we score the ball
        if (decoyBall != null) decoyBall.Scored();  

        goalMaterial.EnableKeyword(emissionKeyword);    //We enable and disable emission in hopes that it actually improves performance

        float init = 0.1f;
        Color initial = new Color(init, init, init);
        goalMaterial.SetColor(emissionColor, initial);   //Set "low" flash color before proceeding to flash the goal

        float fin = 0.4f;
        Color final = new Color(fin, fin, fin);             //This is "high" flash color
        goalMaterial.DOColor(final, emissionColor, 0.15f).SetLoops(8, LoopType.Yoyo).OnComplete(() => { goalMaterial.DisableKeyword(emissionKeyword); });    //Flash the goal 4 times (one time forth and one time back X4), in the end disable emission
    }

    //==================================

    private Vector3 initPos;    //Initial position of the goal to set it back when the period ends (only for teleporting, moving one would get back to animation initial state)
    public bool teleporting = false;    //Script component has a bool variable in the inspector if the goal is teleporting on the arena    

    void Start()
    {
        if (teleporting)
        {
            StartCoroutine(nameof(Teleporting)); //So when it's ticked when the round starts, the goal starts teleporting
            initPos = transform.position;           //Only remember goal initial position if the goal is teleporting
        }

            
    }

    private readonly WaitForSeconds teleportDelay = new WaitForSeconds(9);  //Goal teleports every 10 seconds, 9 seconds nothing happens, then over 1 second it fades out and appears somewhere else
    private readonly WaitForSeconds fadeDelay = new WaitForSeconds(1);

    private Collider[] spawnCheckColliders = new Collider[2];   //Array where raycast would yield the results

    IEnumerator Teleporting()   //A sequence of teleporting
    {
        while (teleporting) //Not sure if this is needed, maybe will be useful in period switching idk
        {
            yield return teleportDelay; //Wait 9 seconds
            GameController.audioManager.Cloak();   //Play the sound of cloaking
            goalMaterial.DOFade(0, 1);          //Start fading tween over 1 second
            yield return fadeDelay;             //Wait one second
            goalMaterial.color = Color.white;   //Set the color back to opaque
            Vector3 rand = GameController.FindRandomPosition(10, 2, 0, spawnCheckColliders);    //Launch function to find random position on the map where we could insert the goal

            transform.position = new Vector3(rand.x, transform.position.y, rand.z); //Place the goal to this position on it's initial height (Y component)

        }

    }
    
    public void SetEverythingBack() //Another one of those for goal
    {
        goalMaterial.DisableKeyword(emissionKeyword);   //If the goal was flashing from scoring, disable it
        goalMaterial.color = Color.white;               //If the goal for some reason was teleporting when period ended, set its color to opaque

        if (teleporting)    //If the goal is set to be teleporting
        {
            StopCoroutine(nameof(Teleporting)); //Stop teleporting coroutine
            transform.position = initPos;       //Get the goal back to where it started in the start of the level
            StartCoroutine(nameof(Teleporting));    //Launch coroutine (it will not execute until the game gets unpaused)
        }

        Animator goalAnim = GetComponentInChildren<Animator>(); //Get goal animator (it's on the child object)
        if (goalAnim != null) goalAnim.Rebind();                //If there is animation for the goal in this arena, launch this mysterious function that will set animator to its initial state
       

    }


}
