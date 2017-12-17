using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    AudioManager has 4 AudioSources attached to it:
        ScoreAnnouncer: source to play announcer sounds that need to be not interrupted by other announcer sounds (has "He Scores!" clip attached to it)
        Announcer: announcer sounds that all interrupt each other when played   (no attached clip)
        AudioManager: for interruptible explosion sound and all other non-interruptibe sounds (has Explosion clip attached to it)
        Music: just for looping music that can be paused and unpaused           (gets music attached to it when the arena loads)
*/



public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip cloak;
    [SerializeField] private AudioClip laserHit;

    [SerializeField] private AudioClip periodHorn;          
    [SerializeField] private AudioClip finalHorn;          

    [SerializeField] private AudioSource audioSource;   //Source.AudioManager object attached to AudioManagerPrefab
    public AudioSource music;                           //Source.Music object attached to AudioManagerPrefab (public, cuz get used to pause the music)

    public void Cloak() => audioSource.PlayOneShot(cloak);  //Cloak sound when someone goes invisible or goal gets teleported

    public void Explosion() => audioSource.Play();      //Self-interruptible explosion sound

    public void LaserHit() => audioSource.PlayOneShot(laserHit);    //Non-self-interruptible laser sound (should technically be interruptible, but absense of it doesn't hurn at all)
    
    public void PeriodHorn() => audioSource.PlayOneShot(periodHorn);    //Period Horn when it ends

    public void FinalHorn() => audioSource.PlayOneShot(finalHorn);      //Final Horn when the game ends   

    void Awake()    //When loaded the scene
    {
        music.clip = GameController.Controller.Music;   //Set the clip to arena music
        music.Play();           //Launch playing of it
        music.Pause();          //And instantly pause, cuz the game technically starts in the paused state

    }









}
