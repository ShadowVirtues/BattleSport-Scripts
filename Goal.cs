using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Goal : MonoBehaviour
{    
    public enum GoalType { OneSided, TwoSided, FourSided }  //GoalType to decide score/reject detection by normals when the ball collides with the goal

    public GoalType goalType;   //Variable for each goal

    [HideInInspector] public Collider ballSolidCollider;    //When the ball hits score-registering parts of the goal, we need to pass that ball through the goal, that' why we disable goals collider for the ball (collider for the player still works)

    private Material goalMaterial;

    void Awake()
    {
        ballSolidCollider = GetComponent<Collider>();

        goalMaterial = GetComponent<Renderer>().material;
    }

    public void FlashGoalOnScore()
    {
        goalMaterial.EnableKeyword("_EMISSION");    

        float init = 0.1f;
        Color initial = new Color(init, init, init);
        goalMaterial.SetColor("_EmissionColor", initial);

        float fin = 0.4f;
        Color final = new Color(fin, fin, fin);

        goalMaterial.DOColor(final, "_EmissionColor", 0.15f).SetLoops(8, LoopType.Yoyo).OnComplete(() => { goalMaterial.DisableKeyword("_EMISSION"); });
    }

    
    
}
