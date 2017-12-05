using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image GameFader;     //Black image across the whole screen that fades the game to black, showing UI over that black screen (we access it in the GameController when the initial countdown ends),
    [SerializeField] private Image UIFader; //Same black image, but used to fade actual UI, when GameFader has faded the game (another approach could be fading individual panels on UI, for now using fader)

    public GameObject CountdownPanel;   //The panel that shows when starting the game (gets destroyed in the end of countdown, since we don't need it anymore for the play time)
    public Text Countdown;              //Countdown text to change it every second in GameController until the game starts

    [SerializeField] private GameObject periodPanel;    //Panel with all Periods (to enable it)
    [SerializeField] private GameObject periodCirclePrefab; //The prefab to instantiate
    [SerializeField] private RectTransform periodContainer; //Container for period circles in PeriodsUI to instantiate period circles there when initializing the scene

    [SerializeField] private Text PlayerOneName;
    [SerializeField] private Text PlayerTwoName;    //Player names in Periods UI
    [SerializeField] private Text PlayerOneScore;   
    [SerializeField] private Text PlayerTwoScore;   //Player scores in Periods UI

    [SerializeField] private AudioClip periodHorn;          //Period Horn when it ends
    [SerializeField] private AudioClip[] periodSound;       //Sounds during periods UI how many periods left, sound is different if the score is tied
    [SerializeField] private AudioClip[] periodSoundTied;

    private AudioSource audioSource;
    
    private GameObject[] periodCircles;     //Red period circles reference array to enable them each period

    //TODO Starting Sequence (TUGUSH)

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();  //AudioSource is attached to GameUI object
    }

    void Start()
    {
        number = GameController.Controller.NumberOfPeriods; //Shorter reference

        if (number < 2) return;     //We don't have period UI if the game is score-based (periods = 0), or there is only one period

        PlayerOneName.text = GameController.Controller.PlayerOne.PlayerName;    //Set player names in Period UI
        PlayerTwoName.text = GameController.Controller.PlayerTwo.PlayerName;
               
        periodCircles = new GameObject[number]; //Initialize array of red period circles

        for (int j = 0; j < number; j++)    //For all periods
        {
            float x = periodContainer.rect.x;    //Get the left position of period container rect
            float w = periodContainer.rect.width;//Get the width of the container rect
            float pos = x + w * (j + 1) / (number + 1);     //Transform those into circle positions on UI         
            GameObject circle = Instantiate(periodCirclePrefab, periodContainer);   //Instantiate period circles, parented to the container
            circle.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos, 0);    //Set theirs position in the container
            periodCircles[j] = circle.transform.GetChild(0).gameObject; //Fill into array the red circle, which is parented to black period circle in prefab (we enable them as new periods start)
            circle.transform.GetChild(1).GetComponent<Text>().text = (j + 1).ToString();    //Set text below the circle to the corresponding period number
        }

        periodCircles[0].SetActive(true);   //Set active first red period circle
    }
    
    private int period;     //Current period number (when Periods UI gets shown, this is the number of 'next' period //Shorter references from GameController
    private int number;     //Number of periods

    public void EndPeriod()     //Function that gets called from GameController when period countdown comes to 0
    {
        period = ++GameController.Controller.CurrentPeriod; //Increment a period in GameController and assign it to this variable

        gameObject.SetActive(true); //Enable GameUI object (it gets disabled in the end of period sequence of countdown sequence)

        if (period <= GameController.Controller.NumberOfPeriods)    //If we are still in playable number of periods
        {
            StartCoroutine(EndPeriodSequence());    //Start Period sequence
        }
        else
        {          
            //TODO IF IT IS TIED
            StartCoroutine(EndGameSequence());
        }
        
    }

    private float time = 1.5f;  //Fade time of everything in period UI and also in end game UI

    IEnumerator EndPeriodSequence()
    {
        GameController.Controller.Pause();      //Pause the game
       
        audioSource.PlayOneShot(periodHorn);    //Play period horn
        UIFader.color = Color.clear;            //Make sure UIFader is transparent (so we don't get black screen)
        GameFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true);   //Fade out the game (SetUpdate makes tween use unscaled time, since the game is paused with Time.timeScale = 0)

        int P1Score = GameController.Controller.PlayerOne.playerStats.Score;    //Shorter references
        int P2Score = GameController.Controller.PlayerTwo.playerStats.Score;

        PlayerOneScore.text = P1Score.ToString();   //Set player scores on Periods UI
        PlayerTwoScore.text = P2Score.ToString();       

        yield return new WaitForSecondsRealtime(time);      //We have timeScale set to 0, so need to use unscaled time to wait

        UIFader.color = Color.black;    //After fading out the game, set UI fader to black, so by further fading it out, it fades in Periods UI
        periodPanel.SetActive(true);    //Enable Periods UI
        UIFader.DOFade(0, time).SetUpdate(UpdateType.Normal, true); //Fade out UIFader so Periods UI fades in

        yield return new WaitForSecondsRealtime(time);      //Wait until it fades in

        GameController.Controller.SetEverythingBack();      //Run a big function to reset the whole scene to initial state

        if (P1Score == P2Score) //If scores are tied, play the sound "Tie game, [number of periods]"
        {
            audioSource.PlayOneShot(periodSoundTied[period - number + 3]);  //That's how you get corresponding audioClip from the array depending on current period
        }
        while (InputManager.GetButtonDown("Start", PlayerID.One) == false && InputManager.GetButtonDown("Start", PlayerID.Two) == false)    //Until some user presses 'Start', wait
        {                                               
            yield return null;
        }
        if (P1Score != P2Score) //If players are not tied, say "Next period" after player presses the button
        {
            if (number == 4 && period == 4)
                audioSource.PlayOneShot(periodSound[period - 1]);
            else
                audioSource.PlayOneShot(periodSound[period - 2]);
        }

        for (int i = 0; i < period; i++)    //After pressing the button activate next red circle
        {
            periodCircles[i].SetActive(true);
        }

        UIFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true); //Fade in UIFader so Periods UI fades out

        yield return new WaitForSecondsRealtime(time);  //Wait until it fades

        System.GC.Collect();    //Collect all garbage before the game starts

        GameFader.color = Color.clear;  //Instantly make GameFader transparent
        periodPanel.SetActive(false);   //Disable periods panel
        gameObject.SetActive(false);    //And disable the whole UI
       
        GameController.Controller.Pause();  //Unpause the game and players proceed to play
    }

    IEnumerator EndGameSequence()
    {
        yield return null;
        SceneManager.LoadScene("MainMenu"); //Load Main Menu scene
        //TODO
        
    }










}
