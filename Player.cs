using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;



/*
    ==================GLOBAL CONCEPTS========================
    EVERYTHING PLAYER-PLAYER RELATED HAS TO REVOLVE AROUND WHICH PLAYER NUMBER IS SET UP TO EACH ONE ON TOP OF HIERARCHY IN THIS SCRIPT

    Ideally i should make ONE SINGLE "PlayerX" prefab with everything setup, and then when instantiating, just set its parameters for different players.
    ==========================================================

    Next thing to do: come up with damage system with firepower, armor which affects mass.



    Try to fix this bullshit when the tank moves by itself





    Consider that we have default Physic Material and there needs to be ball bouncing out of everything     
    

    Adjust thrusters lifetime so you don't see it while moving backwards. MAYBE MAKE CULLING SHIT TO CAMERA, SO WITH INSANE FOVS PLAYERS COULDNT SEE THEIR OWN TANK
         
    When putting a chosen tank in a scene's container "PlayerOne/Two", set the layer of the tank object to respective one. Care about "PlayerExplosion" layer






    ADDITIONAL IDEAS:
    When hit, slider slowly going down with red trail like in LOL






*/



[System.Serializable]
public class PlayerStats
{
    public int Score;
    public int ShotsOnGoal;

    public int MissilesFired;
    public int MissilesHit;

    public int Fumbles;
    public int Interceptions;

    public int Kills;

    public float Possession;
    //public float GameTime; //TODO probably will be in GameController or something
}

public class Player : MonoBehaviour
{
    public PlayerID PlayerNumber; //From TeamUtility InputManager "Add-on". Sets which player it is

    public PlayerStats playerStats;

    [Header("Health")]
    public float Health = 100;
    [SerializeField] private Slider healthSlider;

    [Header("Explosion")]
    public ParticleSystem particleSmoke;
    public ParticleSystem particleExplosion;

    [SerializeField] private GameObject explosion;
    [SerializeField] private GameObject tankModel;
    [SerializeField] private GameObject deathScreen;
    [SerializeField] private Text deathTimer;
    [SerializeField] private Animator cameraAnim;


    private int explodedTime = 2;
    private readonly WaitForSeconds secondOfDeath = new WaitForSeconds(1);

    //====================OTHER=====================

    private Rigidbody playerRigidbody;
    private PlayerMovement movement;


    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
    }

    private IEnumerator Explode()
    {
        
        
        
        
        movement.enabled = false;
        playerRigidbody.drag = 3;
        tankModel.SetActive(false);
        explosion.SetActive(true);

        cameraAnim.enabled = true;
        cameraAnim.Play("ExplodeCamera",-1,0);
        yield return new WaitForSeconds(0.5f);
        deathScreen.SetActive(true); 
        
        //TODO make it explode into pieces

        for (int i = explodedTime; i > 0; i--)
        {
            deathTimer.text = i.ToString();
            yield return secondOfDeath;

        }
        
        cameraAnim.enabled = false;

        explosion.SetActive(false);
        //TODO SPAWN PLAYER SOMEWHERE ON THE MAP

        movement.enabled = true;
        playerRigidbody.drag = 0;

        //Health = 100;
        //healthSlider.value = Health;

        tankModel.SetActive(true);

        deathScreen.SetActive(false);
    }



    private void IncreaseDestroyedTime()
    {
        explodedTime += 1;

        ParticleSystem.MainModule moduleSmoke = particleSmoke.main;
        ParticleSystem.MainModule moduleExplosion = particleExplosion.main;
        
        moduleSmoke.duration = explodedTime - 0.5f;
        moduleExplosion.duration = explodedTime;

        moduleSmoke.startLifetimeMultiplier = explodedTime;
        moduleExplosion.startLifetimeMultiplier = explodedTime;
    }

    public void Hit(float damage)
    {
        Health -= damage;

        

        

        if (Health < 0)
        {
            Health = 0;
            healthSlider.value = Health;

            playerStats.Kills++;
            if (playerStats.Kills == 5 || playerStats.Kills == 10) IncreaseDestroyedTime();
            StartCoroutine(Explode());

            //TODO Destroy tank

        }
        else
        {
            healthSlider.value = Health;
        }
        

    }

    private void FindRandomPosition()
    {
        




    }



}
