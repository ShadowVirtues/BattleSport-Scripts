using System.Collections;
using System.Collections.Generic;
using DG.Tweening.Plugins;
using UnityEngine;

public class Announcer : MonoBehaviour
{
    //COMM EVERYTHING

    [SerializeField] private AudioClip possession;
    [SerializeField] private AudioClip[] interception;
    [SerializeField] private AudioClip[] missClose;
    [SerializeField] private AudioClip[] missLong;
    [SerializeField] private AudioClip rejected;
    [SerializeField] private AudioClip shotShort;
    [SerializeField] private AudioClip shotLong;
    [SerializeField] private AudioClip fumble;
    [SerializeField] private AudioClip[] kill;
    [SerializeField] private AudioClip violation;
 
    [SerializeField] private AudioSource announcerSource;
    [SerializeField] private AudioSource announcerSourceScore;

    void Awake()
    {
        
    }


    public void Score()
    {
        announcerSourceScore.Play();
    }

    public void Possession()
    {
        announcerSource.clip = possession;
        announcerSource.Play();
    }

    public void Interception()
    {
        announcerSource.clip = interception[Random.Range(0, 2)];
        announcerSource.Play();
    }

    public void MissClose()
    {
        announcerSource.clip = missClose[Random.Range(0, 2)];
        announcerSource.Play();
    }

    public void MissLong()
    {
        announcerSource.clip = missLong[Random.Range(0, 2)];
        announcerSource.Play();
    }

    public void Rejected()
    {
        announcerSource.clip = rejected;
        announcerSource.Play();
    }

    public void ShotShort()
    {
        announcerSource.clip = shotShort;
        announcerSource.Play();
    }

    public void ShotLong()
    {
        announcerSourceScore.PlayOneShot(shotLong);
    }

    public void Fumble()
    {
        announcerSource.clip = fumble;
        announcerSource.Play();
    }

    public void Kill()
    {
        //announcerSource.clip = kill[Random.Range(0, 3)];
        //announcerSource.Play();
        announcerSourceScore.PlayOneShot(kill[Random.Range(0, 3)]);
    }

    public void Violation()
    {
        announcerSource.clip = violation;
        announcerSource.Play();
    }

    



    //When the player gets the ball from the ground
    //When the player shoots the ball and the other player is in the way and intercepts it
    //COMM
    //COMM
    //When the player hits the solid part of the goal
    //COMM
    //When the other player fumbles the ball from the holder
    //When some player dies
    //When the shot clock timer goes all the way down and player loses the ball









    //================================================

    //private bool[,] interruptionMatrix = new bool[12, 12];
    //private string[] respection = new string[12];
    //private AudioClip[][] audioResp = new AudioClip[12][];


    //void Start()
    //{
    //    audioResp[0] = score;
    //    audioResp[1] = possession;
    //    audioResp[2] = interception;
    //}

    //Announcer has two AudioSources. They both get decided if which plays together with the second on or intrerrupts already playing one.
    //Launching announcer sound from outer code is all the same, just function like Score() are gonna be "Score() => PlayAnnouncer(0)", where 0 is the index of respective announcer clip from "respection" array
    //Then this number gets compared with "interruptionMatrix" knowing what sound number is already playing in existing sources, if there is true, we interrupt playing soun
    //and if there is "true", we interrup









}

