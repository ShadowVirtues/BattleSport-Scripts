using UnityEngine;

public class Announcer : MonoBehaviour
{
    //Announcer script component is attached to AudioManager object 

    [Header("Game Sounds")]
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

    [Header("Loading Sounds")]
    [SerializeField] private AudioClip[] loadingComments;
 
    [Header("Audio Sources")]
    [SerializeField] private AudioSource announcerSource;       //Interruptible announcer source
    [SerializeField] private AudioSource announcerSourceScore;  //Non-interruptible announcer source
    
    public void Score()     //"He scores!" (its clip is attached to this source)
    {
        announcerSourceScore.Play();
    }

    public void Possession()    //When the player gets the ball from the ground
    {
        announcerSource.clip = possession;
        announcerSource.Play();
    }

    public void Interception()  //When the player shoots the ball and the other player is in the way and intercepts it
    {
        announcerSource.clip = interception[Random.Range(0, 2)];
        announcerSource.Play();
    }

    public void MissClose()  //Missed close to the goal
    {
        announcerSource.clip = missClose[Random.Range(0, 2)];
        announcerSource.Play();
    }

    public void MissLong()  //Missed far away from goal
    {
        announcerSource.clip = missLong[Random.Range(0, 2)];
        announcerSource.Play();
    }

    public void Rejected()  //When the player hits the solid part of the goal
    {
        announcerSource.clip = rejected;
        announcerSource.Play();
    }

    public void ShotShort() //Ball shot being close to the goal: "He Shoots"
    {
        announcerSource.clip = shotShort;
        announcerSource.Play();
    }

    public void ShotLong()  //Ball shot from far away: "From Downtown" (non-interruptible)
    {
        announcerSourceScore.PlayOneShot(shotLong);
    }

    public void Fumble()    //When the other player fumbles the ball from the holder
    {
        announcerSource.clip = fumble;
        announcerSource.Play();
    }

    public void Kill()      //When some player dies (non-interruptible)
    {       
        announcerSourceScore.PlayOneShot(kill[Random.Range(0, 3)]);
    }

    public void Violation() //When the shot clock timer goes all the way down and player loses the ball
    {
        announcerSource.clip = violation;
        announcerSource.Play();
    }

    public void LoadingComment()    //Some comment when the countdown sequence is going
    {
        announcerSource.clip = loadingComments[Random.Range(0, loadingComments.Length)];
        announcerSource.Play();        
    }
    
    public void Stop()      //Stop the announcer talking (used when skipping the countdown sequence)
    {
        announcerSource.Stop();
    }

    [Header("Period Sounds")]
    [SerializeField] private AudioClip[] periodSound;       //Sounds during periods UI how many periods left, sound is different if the score is tied
    [SerializeField] private AudioClip[] periodSoundTied;
    [SerializeField] private AudioClip overtimeSound;            //Sound "This game is going into overtime"

    public void PeriodSound(int index)  //Play period sound with specific index depending on the current period
    {
        announcerSourceScore.PlayOneShot(periodSound[index]);
    }

    public void PeriodSoundTied(int index)  //Same for tied case
    {
        announcerSourceScore.PlayOneShot(periodSoundTied[index]);
    }

    public void OvertimeSound()
    {
        announcerSourceScore.PlayOneShot(overtimeSound);
    }







    //============Prototype of Interruption matrix to define which sound can interrupt which (in case we fall back to this system)==================

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
    //Then this number gets compared with "interruptionMatrix" knowing what sound number is already playing in existing sources, if there is true, we interrupt playing sound
    //and if there is "true", we interrup...









}

