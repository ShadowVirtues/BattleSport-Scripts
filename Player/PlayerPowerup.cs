using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPowerup : MonoBehaviour  //Script is attached to the player, most of the stuff related to powerups is handled here
{
    public bool BallAttractor;
    public bool BallGuidance;
    public bool DoubleDamage;       //Most of the bools indicating that some powerup is active for that player
    public bool FlightSystem;
    public bool FumbleProtection;
    //TurboLasers just changes things
    //Health just gets applied
    public bool Shielding;
    //SuperSpeed just changes value;
    //Blind just changes things
    public bool Invisibility;
    public bool Invinsibility;
    public bool Stabilizers;

    public GameObject blinder;            //Grey image across the whole player screen to blind them with                 

    [SerializeField] private Slider[] powerupSliders;   //4 radial sliders that gets shown on player UI when picked some powerup, indicating its remaining duration
    [SerializeField] private Image[] backgrounds;       //Background and fill images on the slider to change them to specific powerup tooltip to show on the slider
    [SerializeField] private Image[] fills;

    [HideInInspector] public Player player;
    [HideInInspector] public PlayerShooting playerShooting; //References to other player components for some powerups to change their values
    [HideInInspector] public PlayerMovement playerMovement;
    
    private Dictionary<Powerups, Coroutine> activePowerups; //Dictionary storing the powerups that are active for this player. It is used to be able to see if the same picked powerup is already running to refresh its duration

    void Awake()
    {
        player = GetComponent<Player>();
        playerShooting = GetComponent<PlayerShooting>();    //Get those components
        playerMovement = GetComponent<PlayerMovement>();
        
        activePowerups = new Dictionary<Powerups, Coroutine>(4);    //Initializing the Dictionary with default length of 4, which is the USUAL max amound of powerups
        //Key is Powerups enum, Value is Coroutine instance that is running for this powerup

        foreach (Slider slider in powerupSliders)   //Disable all sliders on startup (enabling them as the player picks powerups)
        {
            slider.gameObject.SetActive(false);
        }
    }

    //TODO Replace from blender DoubleDamage and TurboLazers
    

    public void PickPowerup(Powerup powerup)    //Function that gets run from OnTriggerEnter of powerup in the arena
    {
        player.pickup.Play();   //Play the pickup sound
        
        if (activePowerups.ContainsKey(powerup.type))   //If the picked powerup is already running for this player (contains in the Dictionary)
        {
            StopCoroutine(activePowerups[powerup.type]);    //Stop its already running coroutine
            activePowerups[powerup.type] = StartCoroutine(pickPowerup(powerup));           //Replace the coroutine with the same new one
            DOTween.Restart(new { powerup.type, player });  //Restart the tween 'progressing' the slider for powerup duration   //CHECK This doesn't consider if the same powerup instances have different duration
            //No idea how this crazy idea of passing anonymous class as an ID for tween worked D: We couldn't set ID only from 'powerup.type' because that would restart the tween for the second player as well if he had the powerup running
            //So the other thing that makes the ID unique would be the actual player, and there was no way of setting some single ID object from existing stuff, we would have to implement some other object
            //No idea how it works is because of 'new' keyword that should create a whole new object, but somehow it treats them as the same one
            
            print("Restarted " + powerup.type); //DELETE
        }
        else    //Otherwise just run a new powerup
        {
            activePowerups.Add(powerup.type, StartCoroutine(pickPowerup(powerup))); //Add the Coroutine to the dictionary

            for (var i = 0; i < powerupSliders.Length; i++) //Iterate through the array of sliders to find the disabled one
            {                
                if (powerupSliders[i].gameObject.activeSelf == false)   //If we found the disabled slider
                {
                    Slider slider = powerupSliders[i];  //Cache the slider for shorter reference, and also make it a separate variable, just i case the OnComplete lambda in the tween doesn't get some random slider with random 'i' of this loop

                    backgrounds[i].sprite = powerup.icon;   //Replace the sprite of backround and fill in the slider
                    fills[i].sprite = powerup.icon;
                    slider.gameObject.SetActive(true);      //Enable slider
                    slider.transform.SetAsLastSibling();    //Set it as last sibling in the hierarchy, so the tooltip appears last in the list (and also that Vertical Layout Group actually refreshes)
                    slider.value = 1;                       //Set the starting value of the tweening slider to 1

                    slider.DOValue(0, powerup.duration).SetId(new { powerup.type, player }) //Tween the radial slider for the powerup duration, setting the ID to powerup type and player, disabling the slider on completion
                        .OnComplete(() => { slider.gameObject.SetActive(false); });
                    
                    break;  //Don't look further for disabled sliders if we found one
                }
            }
            
            print("Started " + powerup.type);   //DELETE
        }
        
        
        
    }

    IEnumerator pickPowerup(Powerup powerup)    //Coroutine that runs when picked some powerup
    {
        if (string.IsNullOrEmpty(powerup.MessageIn) == false)   //If the pick message is not empty
        {
            player.ShowMessage(powerup.MessageIn);  //Show it
        }

        powerup.actionIn(this); //Run the action on pick
        yield return powerup.timer; //Wait for powerup duration
        powerup.actionOut(this);    //Run the action on expiration

        if (string.IsNullOrEmpty(powerup.MessageOut) == false)  //Same stuff for expiration message
        {
            player.ShowMessage(powerup.MessageOut);
        }

        activePowerups.Remove(powerup.type);    //If the powerup reached its duration till the end, remove it from the Dictionary
        print("Removed " + powerup.type);       //DELETE
    }

    
    
    


}
