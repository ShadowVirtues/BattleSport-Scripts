using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image GameFader;
    [SerializeField] private Image UIFader;

    public GameObject CountdownPanel;
    public Text Countdown;

    [SerializeField] private GameObject periodPanel;

    [SerializeField] private GameObject periodCirclePrefab;

    [SerializeField] private RectTransform periodContainer;

    [SerializeField] private Text PlayerOneName;
    [SerializeField] private Text PlayerTwoName;
    [SerializeField] private Text PlayerOneScore;
    [SerializeField] private Text PlayerTwoScore;

    [SerializeField] private AudioClip periodHorn;
    [SerializeField] private AudioClip[] periodSound;
    [SerializeField] private AudioClip[] periodSoundTied;
    
    //TODO Starting Sequence (TUGUSH)

        //NEXT IS TO MANAGE PERIOD SOUND ORDER AND MAKE END PERIOD SEQUENCE

    private GameObject[] periodCircles;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        number = GameController.Controller.NumberOfPeriods; //Shorter reference

        if (number < 2) return;

        PlayerOneName.text = GameController.Controller.PlayerOne.PlayerName;
        PlayerTwoName.text = GameController.Controller.PlayerTwo.PlayerName;
        
        
        periodCircles = new GameObject[number];

        for (int j = 0; j < number; j++)
        {
            float x = periodContainer.rect.x;    //Get the left position of container rect
            float w = periodContainer.rect.width;//Get the width of the container rect
            float pos = x + w * (j + 1) / (number + 1);     //Transform those into circle positions on UI         
            GameObject circle = Instantiate(periodCirclePrefab, periodContainer);   //Instantiate period circles, parented to the container
            circle.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos, 0);    //Set theis position in the container
            periodCircles[j] = circle.transform.GetChild(0).gameObject; //Fill into array the red circle, which is parented to black period circle in prefab (we enable them as new periods start)
            circle.transform.GetChild(1).GetComponent<Text>().text = (j + 1).ToString();    //Set text below the circle to the corresponding period number
        }

        periodCircles[0].SetActive(true);
    }

    //private readonly WaitForSeconds second = new WaitForSeconds(1);

    private int period;
    private int number;

    public void EndPeriod()
    {
        period = ++GameController.Controller.CurrentPeriod;

        gameObject.SetActive(true);

        if (period <= GameController.Controller.NumberOfPeriods)
        {
            StartCoroutine(EndPeriodSequence());
        }
        else
        {           
            StartCoroutine(EndGameSequence());
        }
        
    }

    private float time = 1.5f;

    IEnumerator EndPeriodSequence()
    {
        GameController.Controller.Pause();
        
        
        audioSource.PlayOneShot(periodHorn);       
        GameFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true);

        int P1Score = GameController.Controller.PlayerOne.playerStats.Score;
        int P2Score = GameController.Controller.PlayerTwo.playerStats.Score;

        PlayerOneScore.text = P1Score.ToString();
        PlayerTwoScore.text = P2Score.ToString();
        //Set Period circles

        yield return new WaitForSecondsRealtime(time);

        UIFader.color = Color.black;
        periodPanel.SetActive(true);
        UIFader.DOFade(0, time).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(time);

        if (P1Score == P2Score)
        {
            audioSource.PlayOneShot(periodSoundTied[period - number + 3]);
        }
        while (!InputManager.GetButtonDown("Start"))
        {
            yield return null;
        }
        if (P1Score != P2Score)
        {
            audioSource.PlayOneShot(periodSound[period - 2]);
        }

        for (int i = 0; i < period; i++)
        {
            periodCircles[i].SetActive(true);
        }

        UIFader.DOFade(1, time).SetUpdate(UpdateType.Normal, true);

        yield return new WaitForSecondsRealtime(time);

        System.GC.Collect();

        GameFader.color = Color.clear;
        periodPanel.SetActive(false);
        gameObject.SetActive(false);

        //SET EVERYTHING BACK
        //Set Players to their positions
        //Zero players velocity
        //Clear messages
        //Set their health back
        //Set Ball to its position
        //Zero its velocity, angularVelocity, reset possession stuff (check possession UI)
        //Disable all rockets and lasers
        //TODO Check what happens with powerups, if they remain in the same place or start spawning from none. Also check if effects persist on players
        //Set goal to original position
        //Set period time back, launch it again
        //Set scoreboard time back, set periods on it
        //Check what happens to PossessionTime of player if he possesses when period ends, if it doesnt increase because of timeScale = 0
        //Check what happens if you are dead during period end (consider camera animation)
        //Check what happens if just-scored-ui is going, if ball is transparent
        //Check what happens if either tank is flashing, or goal is flashing
        //Check what happens if explosion of rocker/laser is going on

        //Play the game normally and see what else can be going during period end


        //And finally unpause the game
    }


    IEnumerator EndGameSequence()
    {

        yield return null;
    }










}
