using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPowerup : MonoBehaviour
{
    public bool BallAttractor;
    public bool BallGuidance;
    public bool DoubleDamage;
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

    public GameObject blinder;                             

    [SerializeField] private Slider[] powerupSliders;
    [SerializeField] private Image[] backgrounds;
    [SerializeField] private Image[] fills;

    [HideInInspector] public Player player;
    [HideInInspector] public PlayerShooting playerShooting;
    [HideInInspector] public PlayerMovement playerMovement;
    
    private Dictionary<Powerups, Coroutine> activePowerups;

    void Awake()
    {
        player = GetComponent<Player>();
        playerShooting = GetComponent<PlayerShooting>();
        playerMovement = GetComponent<PlayerMovement>();
        
        activePowerups = new Dictionary<Powerups, Coroutine>(4);

        foreach (Slider slider in powerupSliders)
        {
            slider.gameObject.SetActive(false);
        }
    }

    //TODO Replace from blender DoubleDamage and TurboLazers
    

    public void PickPowerup(Powerup powerup)
    {
        player.pickup.Play();

        //IEnumerator coroutine = pickPowerup(powerup);

        if (activePowerups.ContainsKey(powerup.type))
        {
            StopCoroutine(activePowerups[powerup.type]);
            activePowerups[powerup.type] = StartCoroutine(pickPowerup(powerup));           
            DOTween.Restart(new { powerup.type, player });
            
            print("Restarted " + powerup.type);
        }
        else
        {
            activePowerups.Add(powerup.type, StartCoroutine(pickPowerup(powerup)));

            for (var i = 0; i < powerupSliders.Length; i++)
            {
                Slider slider = powerupSliders[i];
                if (slider.gameObject.activeSelf == false)
                {
                    backgrounds[i].sprite = powerup.icon;
                    fills[i].sprite = powerup.icon;
                    slider.gameObject.SetActive(true);
                    slider.transform.SetAsLastSibling();
                    slider.value = 1;

                    slider.DOValue(0, powerup.duration).SetId(new { powerup.type, player })
                        .OnComplete(() => { slider.gameObject.SetActive(false); });
                    // new {powerup.type, player}
                    break;
                }
            }
            
            print("Started " + powerup.type);
        }
        
        
        
    }

    IEnumerator pickPowerup(Powerup powerup)
    {
        if (string.IsNullOrEmpty(powerup.MessageIn) == false)
        {
            player.ShowMessage(powerup.MessageIn);
        }

        powerup.actionIn(this);
        yield return powerup.timer;
        powerup.actionOut(this);

        if (string.IsNullOrEmpty(powerup.MessageOut) == false)
        {
            player.ShowMessage(powerup.MessageOut);
        }

        activePowerups.Remove(powerup.type);
        print("Removed " + powerup.type);
    }

    
    
    


}
