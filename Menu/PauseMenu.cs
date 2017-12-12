using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels/Menus")]
    [SerializeField] private GameObject mainPanel;          //Reference specifically to Main panel to enable it when invoking pause menu
    [SerializeField] private GameObject[] allPanels;        //Reference to all menu panels to disable all of them every time switching some menu (for 'general implementation' of menu switching)

    [Header("Other")]
    [SerializeField] private GameObject resume; //Reference specifically to "Resume Game" button to select it when invoking pause menu (all other ones are getting selecting from 'general implementation' - see further)   

    [Header("Game Settings Selectors")]
    [SerializeField] private ValueSelector UIScale;     //Reference to selectors in the Game Settings Menu
    [SerializeField] private ValueSelector radarScale;
    [SerializeField] private ValueSelector iconsScale;
    [SerializeField] private ValueSelector opacity;

    [Header("Audio")]
    [SerializeField] private AudioSource UISource;      //AudioSource to play when clicking through or selecting menus
    [SerializeField] private AudioClip select;          //'Select' menu sound to PlayOneShot it

    private RectTransform rectTransform;                //Rect transform of pause menu is used to position pause menu on specific player screen side when we show it

    [HideInInspector] public EventSystem eventSystem;     //The event system is getting used only in pause menu, and other than that once during end-game menu "Replay Game/Return to Menu". 
                                                          //We can't use EventSystem.current, cuz we disable it during gameplay, and it returns 'EventSystem.current = null' if it is disabled

    private TwoPlayerInputModule inputModule;               //Input module on event system, to swith its PlayerOne/Two flags to block specific player input
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();      //Get the reference
        eventSystem = EventSystem.current;                  //EventSystem gets instantiated in enabled state, and that's how we get a reference to it
        inputModule = eventSystem.GetComponent<TwoPlayerInputModule>();     //Get a reference to input Module
        eventSystem.enabled = false;                                        //We got all the references, now disable event system, it gets enabled when some player pauses the game
        
        LoadGameSettingsValues();       //Load all game settings values from PlayerPrefs
        ApplyUIScale();                 //And apply them
        ApplyRadarScale();
        ApplyRadarIconsScale();
        ApplyRadarOpacity();

        gameObject.SetActive(false);                   //Pause menu instantiates in GameUI in enabled state to run this Awake, we need to disable it in the end of it so its OnEnable doesn't run 
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

        UISource.PlayOneShot(select);       //When invoking pause menu, play 'Select' sound

        DisableAllPanelsAndEnableOne(mainPanel);    //Disable all panels if the menu was saved with some random one active, and enable the main panel

        eventSystem.SetSelectedGameObject(null);    //Without this, when invoking pause menu, the "Resume" button wouldn't highlight for some reason (but it still would be selected for navigation)
        eventSystem.SetSelectedGameObject(resume);  //Yeah, select "Resume" button
    }

    public void ResumeGame()
    {
        eventSystem.enabled = false;        //Disable event system during gameplay
        gameObject.SetActive(false);        //Disable pause menu
        GameController.Controller.gameUI.gameObject.SetActive(false);   //Disable the whole Game UI canvas
        GameController.Controller.UnPause();                    //And finally unpause the game
    }
    
    public void DisableAllPanelsAndEnableOne(GameObject toEnable)   //This is 'general implementation' of menu switching, this function and the next one are applied to every menu-switching button, having the respective parameter 
    {                                                               //In this case it is the menu panel that gets shown (enabled) after selecting this menu
        foreach (GameObject panel in allPanels)              //We disable all possible panels of Pause Menu
        {
            panel.SetActive(false);
        }
        if (toEnable != null) toEnable.SetActive(true);     //And enable the panel that we navigate to (function accepts no menu to enable, so don't enable anything if nothing was passed into function)
    }

    public void PlaySoundAndSelectOption(GameObject toSelect)   //Second function of 'general implementation', it plays the 'Select' sound when selecting the menu, and selects/highlights the respective menu option
    {
        UISource.PlayOneShot(select);
        eventSystem.SetSelectedGameObject(toSelect);
    }

    public void QuitMatch()             //Function that is tied to button "Yes" in the respective menu
    {
        eventSystem.enabled = false;    //Disable event system, so during fading out animation, player couldn't navigate the menu        
        GameController.Controller.gameUI.QuitMatch();   //Run a function on the side of Game UI to fade the screen and all the rest
    }

    //==============SETTINGS====================

    private const string GameSettings_UIScale = "GameSettings_UIScale";
    private const string GameSettings_RadarScale = "GameSettings_RadarScale";   //PlayerPrefs keys
    private const string GameSettings_IconsScale = "GameSettings_IconsScale";
    private const string GameSettings_Opacity = "GameSettings_Opacity";     
    
    public void LoadGameSettingsValues()    //Load settings from PlayerPrefs and set selector values to them
    {
        UIScale.SetValue(PlayerPrefs.GetInt(GameSettings_UIScale, 200));            
        radarScale.SetValue(PlayerPrefs.GetInt(GameSettings_RadarScale, 100));
        iconsScale.SetValue(PlayerPrefs.GetInt(GameSettings_IconsScale, 100));      
        opacity.SetValue(PlayerPrefs.GetInt(GameSettings_Opacity, 100));                                                               
    }

    public void SaveGameSettings()      //Saving to PlayerPrefs, tied to "Back" button in Game Settings
    {
        PlayerPrefs.SetInt(GameSettings_UIScale, UIScale.Option);
        PlayerPrefs.SetInt(GameSettings_RadarScale, radarScale.Option);
        PlayerPrefs.SetInt(GameSettings_IconsScale, iconsScale.Option);
        PlayerPrefs.SetInt(GameSettings_Opacity, opacity.Option);
    }

    public void ApplyUIScale()
    {
        GameController.Controller.PlayerOne.playerRadar.canvasScaler.referenceResolution = new Vector2(UIScale.Option * -19.20f + 5760, UIScale.Option * -10.80f + 3240);   //Changing canvas scaler reference resolution for easy UI scaling
        GameController.Controller.PlayerTwo.playerRadar.canvasScaler.referenceResolution = new Vector2(UIScale.Option * -19.20f + 5760, UIScale.Option * -10.80f + 3240);   //Formula to convert 200% to 1920x1080 resolution, and 100% to 3840x2160
    }

    public void ApplyRadarScale()
    {
        GameController.Controller.PlayerOne.playerRadar.ApplyRadarScale(radarScale.Option / 100f);  //Passing control to function on the side of PlayerRadar, with converting 150% to 1.5
        GameController.Controller.PlayerTwo.playerRadar.ApplyRadarScale(radarScale.Option / 100f);       
    }

    public void ApplyRadarIconsScale()
    {
        GameController.Controller.PlayerOne.playerRadar.ApplyIconsScale(iconsScale.Option / 100f);  //Passing control to function on the side of PlayerRadar
        GameController.Controller.PlayerTwo.playerRadar.ApplyIconsScale(iconsScale.Option / 100f);
    }

    public void ApplyRadarOpacity()
    {
        GameController.Controller.PlayerOne.playerRadar.radarBackground.GetComponent<Image>().color = new Color(1, 1, 1, opacity.Option / 100f);    //Directly setting opacity from here
        GameController.Controller.PlayerTwo.playerRadar.radarBackground.GetComponent<Image>().color = new Color(1, 1, 1, opacity.Option / 100f);
    }


    //TODO Make general navigating in menu, like pressing Escape or Cancel button on controller to go to previous menu








    public void ExitGame()
    {       
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }


}
