using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels/Menus")]
    [SerializeField] private GameObject mainPanel;          //Reference specifically to Main panel to enable it when invoking pause menu
    [SerializeField] private GameObject[] settingsPanels;       //Reference to settings panels specifically to enable-disable them in Awake here, so MenuSelector's Awakes also run
    [SerializeField] private GameObject[] allOtherPanels;        //Reference to all other menu panels to disable all of them every time switching some menu (for 'general implementation' of menu switching)
 
    [Header("Other")]
    [SerializeField] private GameObject resume; //Reference specifically to "Resume Game" button to select it when invoking pause menu (all other ones are getting selecting from 'general implementation' - see further)   

    [Header("Game Settings Selectors")]
    [SerializeField] private ValueSelector UIScale;     //Reference to selectors in the Game Settings Menu
    [SerializeField] private ValueSelector radarScale;
    [SerializeField] private ValueSelector iconsScale;
    [SerializeField] private ValueSelector opacity;

    [Header("Sound Settings Selectors")]
    [SerializeField] private ValueSelector masterVol;     //Reference to selectors in the Sound Settings Menu
    [SerializeField] private ValueSelector musicVol;
    [SerializeField] private ValueSelector SFXVol;
    [SerializeField] private ValueSelector announcerVol;

    [Header("Audio")]
    [SerializeField] private AudioSource UISource;      //AudioSource to play when clicking through or selecting menus
    [SerializeField] private AudioClip select;          //'Select' menu sound to PlayOneShot it
    [SerializeField] private AudioMixer mixer;          //AudioMixer to set volumes for it

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

        foreach (GameObject panel in settingsPanels)   //Unity has no good feature to run 'Awake' on disabled objects so we are going for this
        {
            panel.SetActive(true);
            panel.SetActive(false);
        }

        LoadGameSettingsValues();       //Load all game settings values from PlayerPrefs
        ApplyUIScale();                 //And apply them
        ApplyRadarScale();
        ApplyRadarIconsScale();
        ApplyRadarOpacity();

        LoadSoundSettingsValues();      //Load all sound settings values from PlayerPrefs
        ApplyMasterVolume();            //And apply this
        ApplyMusicVolume();             //NOTE: sounds settings don't get applied when starting injected scenes (when settings are supposed to be applied right at the moment of pressing Play in Editor)
        ApplySFXVolume();
        ApplyAnnouncerVolume();
        
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
        mainPanel.SetActive(false);                               //Disabling main panel
        foreach (GameObject panel in settingsPanels)              //Disabling all settings panels
        {
            panel.SetActive(false);
        }
        foreach (GameObject panel in allOtherPanels)              //Disabling all other possible panels of Pause Menu
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

    //==============GAME SETTINGS====================

    private const string GameSettings_UIScale = "GameSettings_UIScale";
    private const string GameSettings_RadarScale = "GameSettings_RadarScale";   //PlayerPrefs keys for game settings
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

    //===================SOUND SETTINGS====================

    public void SoundSettings()     //Function to call when entering sound settings menu
    {
        GameController.audioManager.music.UnPause();    //Playing the music so the user can hear volume corellations
    }

    public void BackFromSoundSettings()     //Function to call when exiting sound settings menu
    {
        GameController.audioManager.music.Pause();     //Pause back the music
    }

    private const string SoundSettings_Master = "SoundSettings_Master";
    private const string SoundSettings_Game_Music = "SoundSettings_Game_Music";     //PlayerPrefs keys and AudioMixer exposed parameters (I made so they have same names)
    private const string SoundSettings_Game_SFX = "SoundSettings_Game_SFX";
    private const string SoundSettings_Game_Announcer = "SoundSettings_Game_Announcer";

    public void LoadSoundSettingsValues()    //Load settings from PlayerPrefs and set selector values to them
    {
        masterVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Master, 100));
        musicVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Game_Music, 100));
        SFXVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Game_SFX, 100));
        announcerVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Game_Announcer, 100));
    }

    public void SaveSoundSettings()      //Saving to PlayerPrefs, tied to "Back" button in Sound Settings
    {
        PlayerPrefs.SetInt(SoundSettings_Master, masterVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Game_Music, musicVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Game_SFX, SFXVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Game_Announcer, announcerVol.Option);
    }
    
    public void ApplyMasterVolume() //Apply volume when starting the scene, or switched the selector in the settings
    {
        float db = percentToDB(masterVol.Option);
        mixer.SetFloat(SoundSettings_Master, db);
    }

    public void ApplyMusicVolume()
    {
        float db = percentToDB(musicVol.Option);
        mixer.SetFloat(SoundSettings_Game_Music, db);
    }

    public void ApplySFXVolume()
    {
        float db = percentToDB(SFXVol.Option);
        mixer.SetFloat(SoundSettings_Game_SFX, db);       
    }

    public void ApplyAnnouncerVolume()
    {
        float db = percentToDB(announcerVol.Option);
        mixer.SetFloat(SoundSettings_Game_Announcer, db);        
    }

    private float percentToDB(int percent)  //Function to convert Percents (0 - 100) to dB (-80 - 0)
    {
        int y = percent;
        return y == 0 ? -80 : 30 * Mathf.Log10(y / 100f);
    }

    public void SFXSample()     //Additional function to tie when changing SFX volume (to hear the sound corellation between other selectors)
    {
        GameController.audioManager.Explosion();    //Play explosion sound to corellate between other options
    }

    public void AnnouncerSample()   //Same for announcer selector changing
    {
        GameController.announcer.Interception();    //I chose to play interception sound here, as these are the coolest
    }

    //===================VIDEO SETTINGS=====================















    //==========================================================

    public void ExitGame()
    {       
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }


}
