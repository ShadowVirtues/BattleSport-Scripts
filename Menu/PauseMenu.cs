using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels/Menus")]
    [SerializeField] private GameObject mainPanel;
    //[SerializeField] private GameObject settingsPanel;
    //[SerializeField] private GameObject soundSetPanel;
    //[SerializeField] private GameObject gameSetPanel;
    //[SerializeField] private GameObject sureQuit;
    //[SerializeField] private GameObject sureExit;
    [SerializeField] private GameObject[] allPanels;

    [Header("Other")]
    [SerializeField] private GameObject resume; //TODO Maybe not GameObject
    //[SerializeField] private GameObject settings;
    //[SerializeField] private GameObject gameSettings;

    [SerializeField] private AudioSource UISource;
    [SerializeField] private AudioClip select;

    private RectTransform rectTransform;                //Rect transform of pause menu is used to position pause menu on specific player screen side when we show it

    [HideInInspector] public EventSystem eventSystem;     //The event system is getting used only in pause menu, and once during end-game menu "Replay Game/Return to Menu". 
                                                          //We can't really use EventSystem.current, cuz we disable it during gameplay, and 'EventSystem.current = null' if it is disabled

    private TwoPlayerInputModule inputModule;               //Input module on event system, to swith its PlayerOne/Two flags to block specific player input

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();      //Get the reference
        eventSystem = EventSystem.current;                  //EventSystem gets instantiated in enabled state, and that's how we get a reference to it
        inputModule = eventSystem.GetComponent<TwoPlayerInputModule>();     //Get a reference to input Module
        eventSystem.enabled = false;                                        //We got all the references, now disable event system, it gets enabled when some player pauses the game
        gameObject.SetActive(false);                                        //Pause menu instantiates in GameUI in enabled state to run this Awake, we need to disable it in the end of it so its OnEnable doesn't run 
    }

    void OnEnable()     //Runs when some player pauses the game and the menu shows
    {
        if (GameController.Controller.PausedPlayer == PlayerID.One) //If it was player one who paused the game
        {
            rectTransform.anchoredPosition = new Vector2(-480, 0);  //Place the pause menu on his side of the screen
            inputModule.PlayerOne = true;  //Disable player2 input and enable player1 input through EventSystem
            inputModule.PlayerTwo = false;
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)    //Same, but opposite
        {
            rectTransform.anchoredPosition = new Vector2(480, 0);
            inputModule.PlayerOne = false;
            inputModule.PlayerTwo = true;
        }
        
        eventSystem.enabled = true;     //Enable event system, so players can navigate the menu

        UISource.PlayOneShot(select);       //COMM

        DisableAllPanelsAndEnableOne(mainPanel);

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(resume);

    }

    public void ResumeGame()
    {
        eventSystem.enabled = false;        //Disable event system during gameplay
        gameObject.SetActive(false);
        GameController.Controller.gameUI.gameObject.SetActive(false);
        GameController.Controller.UnPause();
    }


    public void DisableAllPanelsAndEnableOne(GameObject toEnable)
    {
        foreach (GameObject panel in allPanels)
        {
            panel.SetActive(false);
        }
        if (toEnable != null) toEnable.SetActive(true);
    }

    public void PlaySoundAndSelectOption(GameObject toSelect)
    {
        UISource.PlayOneShot(select);
        eventSystem.SetSelectedGameObject(toSelect);
    }

    public void QuitMatch()
    {
        eventSystem.enabled = false;    //Disable event system, so during fading out animation, player couldn't navigate the menu        
        GameController.Controller.gameUI.QuitMatch();
    }

    //[SerializeField] private Button yes;
    //[SerializeField] private Button no;

    //IEnumerator Uninteract()
    //{
    //    yes.interactable = false;
    //    no.interactable = false;
    //    yield return new WaitForSecondsRealtime(2);
    //    yes.interactable = true;
    //    no.interactable = true;
    //    DisableAllPanelsAndEnableOne(null);
    //}

    //public void SettingsPanel()
    //{
    //    UISource.PlayOneShot(select);

    //    settingsPanel.SetActive(true);
    //    mainPanel.SetActive(false);
    //    EventSystem.current.SetSelectedGameObject(gameSettings);


    //}

    //public void BackToMainMenu(GameObject toSelect)
    //{
    //    UISource.PlayOneShot(select);

    //    settingsPanel.SetActive(false);
    //    mainPanel.SetActive(true);
    //    EventSystem.current.SetSelectedGameObject(toSelect);

    //}


    void Update()
    {
        //print(EventSystem.current.currentSelectedGameObject);
        
        //print(EventSystem.current.IsPointerOverGameObject());

        //EventSystem.RaycastAll()
    }







    public void ExitGame()
    {
        //TODO ARE YOU SURE?
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }


}
