using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;

public class CountdownPanel : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private RectTransform mainBlock;
    [SerializeField] private RectTransform[] blocks;   

    [Header("Main Block")]
    [SerializeField] private Text arenaText;
    [SerializeField] private Text countdownText;
    [SerializeField] private Text countdownTimer;
    [SerializeField] private Text ballDescription;
    [SerializeField] private Image ballIcon;
    [SerializeField] private Text goalDescription;
    [SerializeField] private Image goalIcon;

    [Header("Blocks")]
    [SerializeField] private Text[] blockText;
    [SerializeField] private Image[] blockIcon;
    
    [Header("Flash")]
    [SerializeField] private RectTransform flash;
    [SerializeField] private Animator anim;


    private Arena arena;
    private AudioSource tugush;

    void Awake()
    {
        arenaText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        countdownTimer.gameObject.SetActive(false);
        ballDescription.gameObject.SetActive(false);
        ballIcon.gameObject.SetActive(false);
        goalDescription.gameObject.SetActive(false);
        goalIcon.gameObject.SetActive(false);

        //anim.gameObject.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            blockText[i].gameObject.SetActive(false);
            blockIcon[i].gameObject.SetActive(false);
        }

        tugush = GetComponent<AudioSource>();
        arena = StartupController.Controller.arena;
        
    }

    private int timer = 10;
    private int powerupCount;

    void Start()
    {
        arenaText.text = "ARENA " + arena.Number;
        countdownTimer.text = timer.ToString(); //Test

        string ballDesc = arena.ballDescription == String.Empty ? "NORMAL" : arena.ballDescription;
        ballDescription.text = arena.ballType.ToString().ToUpper() + "\r\n" + ballDesc;
        ballIcon.sprite = GameController.Controller.ball.Icon;

        if (arena.goalType == Goal.GoalType.OneSided)
        {
            goalDescription.text = "1 SIDED\r\n";
        }
        else if (arena.goalType == Goal.GoalType.TwoSided)
        {
            goalDescription.text = "2 SIDED\r\n";
        }
        else if(arena.goalType == Goal.GoalType.FourSided)
        {
            goalDescription.text = "4 SIDED\r\n";
        }

        goalDescription.text += arena.goalDescription == String.Empty ? "NORMAL" : arena.goalDescription;
        goalIcon.sprite = GameController.Controller.goal.Icon;

        powerupCount = arena.Powerups.Length > 4 ? 4 : arena.Powerups.Length;

        for (int i = 0; i < powerupCount; i++)
        {            
            blockText[i].text = arena.Powerups[i].Name;
            blockIcon[i].sprite = arena.Powerups[i].icon;
          
        }

        //Debug.Break();
        //TODO Start Sequence

        //StartCoroutine(CountdownSequence());
        CustomInputModule.Instance.PlayClick();

        sequence();
    }

    IEnumerator CountdownSequence()
    {
        yield return null;
        sequence();



    }

    Sequence seq;

    private void sequence()
    {
        seq = DOTween.Sequence();

        seq.SetUpdate(UpdateType.Normal, true);

        seq.AppendInterval(1);

        seq.Append(mainBlock.DOAnchorPosY(650, 1.25f).From().OnComplete(TUGUSH));
        float[] tugushTime = {1.2f, 1, 0.7f, 0.6f};
        int[] blockPos = { -600, -600, -375, -375 };
        for (int i = 0; i < powerupCount; i++)
        {
            seq.Append(blocks[i].DOAnchorPosY(blockPos[i], tugushTime[i]).From().OnComplete(TUGUSH));
            
        }
        seq.AppendCallback(() =>
        {
            arenaText.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(true);
            countdownTimer.gameObject.SetActive(true);
            ballDescription.gameObject.SetActive(true);
            ballIcon.gameObject.SetActive(true);
            goalDescription.gameObject.SetActive(true);
            goalIcon.gameObject.SetActive(true);
        });

        Vector2[] flashPos =
        {
            new Vector2(-523, -110),
            new Vector2(516, -117),
            new Vector2(-523, -374),
            new Vector2(516, -383)
        };

        //seq.AppendCallback(() => anim.gameObject.SetActive(true));

        for (int i = 0; i < powerupCount; i++)
        {
            int ii = i;

            seq.AppendCallback(() => flash.anchoredPosition = flashPos[ii]);
            seq.AppendCallback(() => anim.Play("Flash", -1, 0));

            seq.AppendInterval(1 / 5f);

            seq.AppendCallback(() =>
            {                
                blockText[ii].gameObject.SetActive(true);
                blockIcon[ii].gameObject.SetActive(true);
            });
            
        }

        seq.AppendCallback(() => anim.gameObject.SetActive(false));

        seq.AppendInterval(0.5f);

        seq.AppendCallback(() =>
        {
            if (skipped == false)
            {
                GameController.announcer.LoadingCommentLongest();
                isPlaying = true;
            }
            
        }); 

        seq.Play();

    }

    private void TUGUSH()
    {
        tugush.Play();
    }

    private bool skipped = false;
    private bool isPlaying = false;

    void Update()
    {
        if (skipped == false && (InputManager.GetButtonDown("Start", PlayerID.One) || InputManager.GetButtonDown("Start", PlayerID.Two)))
        {
            skipped = true;
            seq.Complete(true);

            if (isPlaying)
            {
                GameController.announcer.Stop();
            }
            else
            {
                GameController.announcer.LoadingCommentShort();
            }
           
                          
        }


    }



}
