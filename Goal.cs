using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Goal : MonoBehaviour
{    
    public enum GoalType { OneSided, TwoSided, FourSided }  //GoalType to decide score/reject detection by normals when the ball collides with the goal

    public GoalType goalType;   //Variable for each goal

    [HideInInspector] public Collider ballSolidCollider;    //When the ball hits score-registering parts of the goal, we need to pass that ball through the goal, that' why we disable goal's collider for the ball (collider for the player still works)

    private Material goalMaterial;  //To flash the goal on score

    void Awake()
    {
        ballSolidCollider = GetComponent<Collider>();       //Getting dose references
        goalMaterial = GetComponent<Renderer>().material;
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

    
    
}
