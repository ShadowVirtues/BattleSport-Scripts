using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

    [SerializeField] private Slider[] powerupSliders;
    [SerializeField] private Image[] backgrounds;
    [SerializeField] private Image[] fills;

    [HideInInspector] public Player player;
    [HideInInspector] public PlayerShooting playerShooting;
    [HideInInspector] public PlayerMovement playerMovement;

    //private Dictionary<Powerups, IEnumerator> activePowerups;
    private Dictionary<Powerups, Coroutine> activePowerups;

    void Awake()
    {
        player = GetComponent<Player>();
        playerShooting = GetComponent<PlayerShooting>();
        playerMovement = GetComponent<PlayerMovement>();

        //activePowerups = new Dictionary<Powerups, IEnumerator>(4);
        activePowerups = new Dictionary<Powerups, Coroutine>(4);

        foreach (Slider slider in powerupSliders)
        {
            slider.gameObject.SetActive(false);
        }
    }
    //TODO Circular slider showing powerup remaining duration. Do it with vertical layout group, which will position powerups on screen in a column, collapsing when one powerup ends. 
    //TODO Powerups will have to have the icon attached to each one, to replace it in the slider
    //TODO Circle progressing is done with tween, when refreshing powerup duration, tween ID has to be set with dictionary key

    public void PickPowerup(Powerup powerup)
    {
        player.pickup.Play();

        //IEnumerator coroutine = pickPowerup(powerup);

        if (activePowerups.ContainsKey(powerup.type))
        {
            StopCoroutine(activePowerups[powerup.type]);
            activePowerups[powerup.type] = StartCoroutine(pickPowerup(powerup));
            DOTween.Restart(powerup.type);

            //StopCoroutine(activePowerups[powerup.type]);
            //StartCoroutine(activePowerups[powerup.type]);

            print("Restarted " + powerup.type);
        }
        else
        {
            activePowerups.Add(powerup.type, StartCoroutine(pickPowerup(powerup)));

            foreach (Slider slider in powerupSliders)
            {
                if (slider.gameObject.activeSelf == false)
                {
                    
                    slider.DOValue(0, powerup.duration).SetAutoKill(false).SetId(powerup.type)
                        .OnStart(() =>
                        {
                            slider.gameObject.SetActive(true);
                            slider.value = 1;
                            slider.transform.SetAsLastSibling();
                        })
                    
                        .OnComplete(() =>
                        {
                            slider.gameObject.SetActive(false);
                        });

                    break;
                }
            }

            //activePowerups.Add(powerup.type, pickPowerup(powerup));
            //StartCoroutine(activePowerups[powerup.type]);

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
        
    }






}
