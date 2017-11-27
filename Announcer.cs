using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Announcer : MonoBehaviour
{

    [SerializeField] private AudioClip score;
    [SerializeField] private AudioClip possession;
    [SerializeField] private AudioClip[] interception;
    [SerializeField] private AudioClip[] missClose;
    [SerializeField] private AudioClip[] missLong;
    [SerializeField] private AudioClip rejected;
    [SerializeField] private AudioClip shotLong;
    [SerializeField] private AudioClip fumble;
    [SerializeField] private AudioClip[] kill;
    [SerializeField] private AudioClip violation;

    [SerializeField] private AudioClip cloak;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void Score() => audioSource.PlayOneShot(score);                                      //When the player scores
    public void Possession() => audioSource.PlayOneShot(possession);                            //When the player gets the ball from the ground
    public void Interception() => audioSource.PlayOneShot(interception[Random.Range(0, 2)]);    //When the player shoots the ball and the other player is in the way and intercepts it
    public void MissClose() => audioSource.PlayOneShot(missClose[Random.Range(0, 2)]);          //COMM
    public void MissLong() => audioSource.PlayOneShot(missLong[Random.Range(0, 2)]);          //COMM
    public void Rejected() => audioSource.PlayOneShot(rejected);                                //When the player hits the solid part of the goal
    public void ShotLong() => audioSource.PlayOneShot(shotLong);                                //COMM
    public void Fumble() => audioSource.PlayOneShot(fumble);                                    //When the other player fumbles the ball from the holder
    public void Kill() => audioSource.PlayOneShot(kill[Random.Range(0, 3)]);                    //When some player dies
    public void Violation() => audioSource.PlayOneShot(violation);                              //When the shot clock timer goes all the way down and player loses the ball

    public void Cloak() => audioSource.PlayOneShot(cloak);

}
