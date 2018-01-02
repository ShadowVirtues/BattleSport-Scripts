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
    [SerializeField] private RectTransform mainBlock;   //Main block with ball and goal stuff
    [SerializeField] private RectTransform[] blocks;    //Array of blocks with powerups stuff

    [Header("Main Block")]
    [SerializeField] private Text arenaText;            //Text "ARENA XX" on top
    [SerializeField] private Text countdownText;        //"Starting in" text, reference to disable-enable it
    [SerializeField] private Text countdownTimer;       //Countdown timer going from 15 to 0
    [SerializeField] private Text ballDescription;      //Description of the ball like "Burning Normal"
    [SerializeField] private Image ballIcon;            //Ball icon, how it looks like
    [SerializeField] private Text goalDescription;      //Like "4-sided teleporting"
    [SerializeField] private Image goalIcon;            //Goal icon, how it looks like

    [Header("Blocks")]
    [SerializeField] private Text[] blockText;          //Arrays of text and icon for each block with the same index
    [SerializeField] private Image[] blockIcon;
    
    [Header("Flash")]
    [SerializeField] private RectTransform flash;   //Two references to the same object with flash sprite animation, rect to loop it through all powerup icon positions
    [SerializeField] private Animator anim;         //And animator to launch the animation
    
    private Arena arena;                //Just to store here a shorter reference to arena from StartupController
    private AudioSource tugush;     //Countdown panel will have its own AudioSource with the clip of TUGUSH sound and
    [SerializeField] private AudioClip tick;    //Will play countdown tick as PlayOneShot

    void Awake()
    {
        arenaText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
        countdownTimer.gameObject.SetActive(false);
        ballDescription.gameObject.SetActive(false);    //Saving countdown panel with all stuff enabled, we disable it before showing it
        ballIcon.gameObject.SetActive(false);
        goalDescription.gameObject.SetActive(false);
        goalIcon.gameObject.SetActive(false);
        
        for (int i = 0; i < 4; i++)
        {
            blockText[i].gameObject.SetActive(false);
            blockIcon[i].gameObject.SetActive(false);
        }

        tugush = GetComponent<AudioSource>();   //And get references
        
        
    }
    
    private int powerupCount;   //Storing the number of powerups for the arena that is loading

    void Start()
    {
        arena = StartupController.Controller.arena;

        arenaText.text = "ARENA " + arena.Number;   //Getting all the stuff into the panel, before running the animation (all fields still remain disabled). Getting the values for all the stuff from arena object in StartupController
        
        string ballDesc = arena.ballDescription == String.Empty ? "NORMAL" : arena.ballDescription; //Generating the second line of ball description, which is "Normal" if the description is empty, if not, getting the actual description
        ballDescription.text = arena.ballType.ToString().ToUpper() + "\r\n" + ballDesc; //Put the ball description into the field
        ballIcon.sprite = GameController.Controller.ball.Icon;      //Putting the icon, that is attached to the goal in the arena

        if (arena.goalType == Goal.GoalType.OneSided)
        {
            goalDescription.text = "1 SIDED\r\n";   //Depending on goal type, set the first line of goal description
        }
        else if (arena.goalType == Goal.GoalType.TwoSided || arena.goalType == Goal.GoalType.TwoSidedPhantom)
        {
            goalDescription.text = "2 SIDED\r\n";
        }
        else if(arena.goalType == Goal.GoalType.FourSided)
        {
            goalDescription.text = "4 SIDED\r\n";
        }

        goalDescription.text += arena.goalDescription == String.Empty ? "NORMAL" : arena.goalDescription;   //Add the description if any specified in Arena file, if not, write "Normal"
        goalIcon.sprite = GameController.Controller.goal.Icon;  //Goal icon

        powerupCount = arena.Powerups.Length > 4 ? 4 : arena.Powerups.Length;   //Get the amount of powerups (on countdown panel there are only 4 blocks for them, so if we specified more than 4 powerups to the arena, only first 4 will show)

        for (int i = 0; i < powerupCount; i++)
        {            
            blockText[i].text = arena.Powerups[i].Name; //Get the names and icons of powerups to the blocks
            blockIcon[i].sprite = arena.Powerups[i].icon;         
        }

        if (powerupCount < 4)   //If there are less than 4 powerups, disable redundant blocks
        {
            for (int i = powerupCount; i < 4; i++)
            {
                blocks[i].gameObject.SetActive(false);
            }
        }
        
        CustomInputModule.Instance.PlayClick(); //To play click when the arena scene loads (it sound cool)

        sequence(); //Generate countdown animation sequence and launch it

        StartCoroutine(Countdown());    //Start timer countdown before starting the game (the arena scene loads in a paused state, when all the countdown stuff runs)
    }

    private int count;  //Counter for the countdown (in outer scope, so we can skip the countdown sequence and get the counter to 4 seconds, instead of default 18)

    IEnumerator Countdown()
    {
        int timer = 18;  //How many seconds to count

        for (count = timer; count >= 0; count--)    //For loop of decreasing the countdown value
        {
            countdownTimer.text = $"{count}";   //Set the text to current countdown value

            if (count <= 3)     //When the counter is below 3
            {
                skipped = true;                 //Block the ability to skip the sequence     
                tugush.PlayOneShot(tick);       //And play countdown tick beat for 3,2,1,0 before the game starts
            }

            yield return new WaitForSecondsRealtime(1);         //Since the game is paused at that point, use unscaled time
        }
        GameController.Controller.StartGame();       //Start the game in the end of the countdown (function on the side of GameController)
    }

    Sequence seq;       //DOTween sequence (in outer scope, because we can skip the sequence midway)

    private void sequence()
    {
        seq = DOTween.Sequence();   //Initializing

        seq.SetUpdate(UpdateType.Normal, true); //Set so the sequence uses unscaled time, because the game is paused at that point

        seq.AppendInterval(1);  //Wait 1 sec, before starting the sequence

        seq.Append(mainBlock.DOAnchorPosY(650, 1.25f).From().OnComplete(TUGUSH));   //Move the main block from top and play the TUGUSH sound on complete (everything initially is in its final position, so using From tweener)

        float[] tugushTime = {1.2f, 1, 0.7f, 0.6f}; //Array of times it takes for each block to connect
        int[] blockPos = { -600, -600, -375, -375 };    //Array of coordinates for each block to start moving from

        for (int i = 0; i < powerupCount; i++)  //For the amount of powerups
        {
            seq.Append(blocks[i].DOAnchorPosY(blockPos[i], tugushTime[i]).From().OnComplete(TUGUSH));           //Move the blocks one after other from blockPos over tugushTime   
        }
        seq.AppendCallback(() =>    //After connecting all blocks, enable all main panel fields
        {
            arenaText.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(true);
            countdownTimer.gameObject.SetActive(true);
            ballDescription.gameObject.SetActive(true);
            ballIcon.gameObject.SetActive(true);
            goalDescription.gameObject.SetActive(true);
            goalIcon.gameObject.SetActive(true);
        });

        Vector2[] flashPos =    //Coordinates for the flash animation to play at (using a single object for all 4 powerups and moving it after each animation)
        {
            new Vector2(-523, -110),
            new Vector2(516, -117),
            new Vector2(-523, -374),
            new Vector2(516, -383)
        };
        
        for (int i = 0; i < powerupCount; i++)  //For all powerups
        {
            int ii = i; //Storing the "i" for callback lambda, so it doesn't use some random 'i' that will happen to be in the loop ("closure" stuff)

            seq.AppendCallback(() => flash.anchoredPosition = flashPos[ii]);    //Move the flash to next powerup icon position
            seq.AppendCallback(() => anim.Play("Flash", -1, 0));                //Play the animation

            seq.AppendInterval(1 / 5f);     //Wait until the animation completes

            seq.AppendCallback(() =>
            {                
                blockText[ii].gameObject.SetActive(true);   //Enable powerup icon and text
                blockIcon[ii].gameObject.SetActive(true);
            });           
        }

        seq.AppendCallback(() => anim.gameObject.SetActive(false)); //Disable animation object

        seq.AppendInterval(0.5f);   //Wait 0.5 sec, before playing announcer line

        seq.AppendCallback(() =>
        {
            if (skipped == false)   //If we didn't skip the sequence before, play random announcer loading comment
            {
                GameController.announcer.LoadingComment();               
            }            
        }); 

        seq.Play();     //Launch the sequence after initializing it

    }

    private void TUGUSH()   //Function to play the sound
    {
        tugush.Play();
    }

    private bool skipped = false;   //Flag to see if we have skipped the sequence
   
    void Update()
    {
        if (skipped == false && (InputManager.GetButtonDown("Start", PlayerID.One) || InputManager.GetButtonDown("Start", PlayerID.Two) || Input.GetMouseButtonDown(0)))    //If we haven't skipped and someone pressed Start button or left mouse button
        {
            skipped = true; //Set the flag so we have skipped
            seq.Complete(true); //Complete the countdown sequence

            count = 4;      //Set countdown to 4 seconds
            countdownTimer.text = $"{count}";   //Refresh the countdown text field (or otherwise it would only update in the end of the current second)
            
            GameController.announcer.Stop();    //Stop announcer from saying his line if he was saying the loading comment
            
        }


    }



}
