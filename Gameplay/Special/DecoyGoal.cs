using System.Collections;
using DG.Tweening;
using UnityEngine;

//Script on every decoy goal prefab that gets instantiated by DecoyGoalController
public class DecoyGoal : MonoBehaviour
{
    public Vector3 movement;    //Movement vector of the goal moving in global space
    public float speed;         //Movement speed of goal (multiplies the movement vector)

    private new Transform transform;    //Caching transform, because we use it every frame
    private Material material;          //Goal material to fade it
    private new Collider collider;      //Collider to disable it when the goal is fadig (for players to be able to go through)

    void Awake()
    {
        material = GetComponent<Renderer>().material;   //Getting dose references
        transform = GetComponent<Transform>();
        collider = GetComponent<Collider>();
    }
    
    void OnEnable()
    {
        collider.enabled = true;        //Before we disable the goal when the ball passes through it, we disable collider and fade the material. Set them back when enabling goal
        material.color = Color.white;
    }

    private bool justReflected = false;     //Flag to not engage the reflecting of the goal from level bounds for 3 frames

    void Update()
    {
        if (Time.timeScale == 0) return;    //The goal won't move even if this is not written because of Time.deltaTime, but the CheckSphere should execute when the game is paused, so let's not

        transform.Translate(movement * speed * Time.deltaTime, Space.World);    //Every frame the goal moves towards the direction of movement vector

        if (justReflected == false && Physics.CheckSphere(transform.position, 1, 1 << 13))  //Every frame checking sphere with radius 1 around the goal for proximity with the LevelGeometry layer (being props or level walls)
        //if (Physics.CheckBox(transform.position, Vector3.one * 0.75f, Quaternion.identity, 1 << 13))
        {
            movement *= -1; //If so, reverse the movement vector
            StartCoroutine(reflectDelay());  //Wait for 3 frames before being able to reflect again (so the goals don't get stuck into walls changing movement vector every frame returning 'true' in CheckSphere
        }
    }

    private static readonly WaitForSeconds delay = new WaitForSeconds(0.1f);

    IEnumerator reflectDelay()
    {
        justReflected = true;
        yield return delay;
        justReflected = false;
    }

    void OnTriggerEnter(Collider other) //When the decoy goal triggers with the ball
    {
        if (other.gameObject.layer == 16)   //If triggered object is BallTrigger layer
        {   
            collider.enabled = false;               //Disable the collider instantly and fade it over one second, disable after that
            material.DOFade(0, 1).OnComplete(() => gameObject.SetActive(false));
        }
        
    }
    
}
