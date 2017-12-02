using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip cloak;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioClip laserHit;

    [SerializeField] private AudioSource audioSource;
    public AudioSource music;

    public void Cloak() => audioSource.PlayOneShot(cloak);
    public void Explosion()
    {
        audioSource.Play();
    }

    public void LaserHit() => audioSource.PlayOneShot(laserHit);

    void Awake()
    {
        music.clip = GameController.Controller.Music;
        music.Play();
        music.Pause();

    }









}
