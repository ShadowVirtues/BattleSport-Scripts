using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Announcer : MonoBehaviour
{

    [SerializeField] private AudioClip score;
    [SerializeField] private AudioClip possession;
    [SerializeField] private AudioClip[] interception;
    [SerializeField] private AudioClip[] missClose;
    [SerializeField] private AudioClip rejected;
    [SerializeField] private AudioClip shotLong;


    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void Score() => audioSource.PlayOneShot(score);
    public void Possession() => audioSource.PlayOneShot(possession);
    public void Interception() => audioSource.PlayOneShot(interception[Random.Range(0, 2)]);
    public void MissClose() => audioSource.PlayOneShot(missClose[Random.Range(0, 2)]);
    public void Rejected() => audioSource.PlayOneShot(rejected);
    public void ShotLong() => audioSource.PlayOneShot(shotLong);




}
