using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsMenu : PauseMenu
{
    [Header("Panels/Menus")]
    //[SerializeField] private GameObject mainPanel;          //This gets inherited from PauseMenu (everything else as well, despite being private tho ******UNITY******)
    
    [SerializeField] private GameObject[] settingsPanels;       //Reference to settings panels specifically to enable-disable them in Awake here, so MenuSelector's Awakes also run

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
    [SerializeField] private ValueSelector menuSFXVol;

    [SerializeField] private ValueSelector menuMusicVol;
    [SerializeField] private ValueSelector menuAnnouncerVol;

    [Header("Video Settings Selectors")]
    [SerializeField] private ResolutionSelector resolution;     //Reference to selectors in the Video Settings Menu
    [SerializeField] private StringSelector windowed;
    [SerializeField] private StringSelector vSync;
    [SerializeField] private StringSelector aa;
    [SerializeField] private StringSelector runInBackground;
    [SerializeField] private ValueSelector fov;

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer;          //AudioMixer to set volumes for it
    
    protected override void Awake()     //PauseMenu which is the base class has its own Awake, which we override so it doesn't run
    {
        if (EventSystem.current != null)
        {
            eventSystem = EventSystem.current;  //Getting event system reverence if its null. "eventSystem" is base class field
        }
        else
        {
            CustomInputModule.Instance.GetComponent<EventSystem>().enabled = true;
            eventSystem = EventSystem.current;
        }

        foreach (GameObject panel in settingsPanels)   //Unity has no good feature to run 'Awake' on disabled objects so we are going for this
        {
            panel.SetActive(true);
            panel.SetActive(false);
        }       

        LoadGameSettingsValues();       //Load all game settings values from PlayerPrefs
        LoadSoundSettingsValues();      //Load all sound settings values from PlayerPrefs
        LoadFOVValue();                 //From video settings, only FOV value gets set "on the fly"

        if (GameController.Controller != null)  //Apply settings only when in game
        {
            ApplyUIScale();                 
            ApplyRadarScale();
            ApplyRadarIconsScale();
            ApplyRadarOpacity();
            ApplyFOV();
        }
        
        ApplyMasterVolume();            //Those sound settings get appied both in game and in menu
        ApplyMusicVolume();             
        ApplySFXVolume();               //NOTE: sounds settings don't get applied when starting injected scenes (when settings are supposed to be applied right at the moment of pressing Play in Editor)
        ApplyAnnouncerVolume();
        ApplyMenuSFXVolume();

        if (GameController.Controller == null)  //Only apply those in main menu
        {
            ApplyMenuMusicVolume();
            ApplyMenuAnnouncerVolume();
        }
        
        gameObject.SetActive(false);                   //Pause menu instantiates in GameUI in enabled state to run this Awake, we need to disable it in the end of it so its OnEnable doesn't run 
    }

    protected override void OnEnable()      //Making sure PauseMenu's OnEnable doesn't run
    {
        
    }

    protected override void Update()    //When update from PauseMenu already runs (in game, when there are both PauseMenu and SettingsMenu), don't run this update
    {
        if (GameController.Controller == null)  //So only if we are not in game
        {
            base.Update();                      //Run update (it processes "Back" buttons in menu)
        }
    }
    
    public override void DisableAllPanelsAndEnableOne(GameObject toEnable)  //This is the function when navigating specifically in the settings menu, which enables settings menu, that has all other settings panels on it
    {                                                               
        mainPanel.SetActive(false);                         //Disable main panel
        foreach (GameObject panel in settingsPanels)        //Disable all settings panel to enable one
        {
            panel.SetActive(false);
        }        
        gameObject.SetActive(true);                          //Enabling settings menu, instead of disabling it in the original method
        if (toEnable != null) toEnable.SetActive(true);     //And then enabling some panel on settings menu
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
    private const string SoundSettings_Menu_SFX = "SoundSettings_Menu_SFX";

    private const string SoundSettings_Menu_Music = "SoundSettings_Menu_Music";
    private const string SoundSettings_Menu_Announcer = "SoundSettings_Menu_Announcer";

    public void LoadSoundSettingsValues()    //Load settings from PlayerPrefs and set selector values to them
    {
        masterVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Master, 100));
        musicVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Game_Music, 100));
        SFXVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Game_SFX, 100));
        announcerVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Game_Announcer, 100));
        menuSFXVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Menu_SFX, 100));

        if (GameController.Controller == null)  //We don't have those selectors in game, so only set them when in main menu
        {
            menuMusicVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Menu_Music, 100));
            menuAnnouncerVol.SetValue(PlayerPrefs.GetInt(SoundSettings_Menu_Announcer, 100));
        }          
    }

    public void SaveSoundSettings()      //Saving to PlayerPrefs, tied to "Back" button in Sound Settings
    {
        PlayerPrefs.SetInt(SoundSettings_Master, masterVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Game_Music, musicVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Game_SFX, SFXVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Game_Announcer, announcerVol.Option);
        PlayerPrefs.SetInt(SoundSettings_Menu_SFX, menuSFXVol.Option);

        if (GameController.Controller == null)      //We don't have those selectors in game, so only get their values when in main menu
        {
            PlayerPrefs.SetInt(SoundSettings_Menu_Music, menuMusicVol.Option);
            PlayerPrefs.SetInt(SoundSettings_Menu_Announcer, menuAnnouncerVol.Option);
        }       
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

    public void ApplyMenuSFXVolume() 
    {
        float db = percentToDB(menuSFXVol.Option);
        mixer.SetFloat(SoundSettings_Menu_SFX, db);
    }

    public void ApplyMenuMusicVolume()
    {
        float db = percentToDB(menuMusicVol.Option);
        mixer.SetFloat(SoundSettings_Menu_Music, db);
    }

    public void ApplyMenuAnnouncerVolume()
    {
        float db = percentToDB(menuAnnouncerVol.Option);
        mixer.SetFloat(SoundSettings_Menu_Announcer, db);
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

    public const string VideoSettings_VSync = "VideoSettings_VSync";
    public const string VideoSettings_AntiAliasing = "VideoSettings_AntiAliasing";     //PlayerPrefs keys and AudioMixer exposed parameters (I made so they have same names)
    public const string VideoSettings_RunInBackground = "VideoSettings_RunInBackground";
    public const string VideoSettings_FOV = "VideoSettings_FOV";

    public void LoadVideoSettings()     //Load video settings when entering respective settings menu
    {
        resolution.SetSelectorValue();      //Set selector to current resolution if it is one of the common ones, set to highest if its some random one (from manually resizing the window)
        windowed.SetIndex(Screen.fullScreen ? 1 : 0);   //Set selector to "Windowed" or "Borderless" (which Unity deems as fullscreen), depending on the window actual state

        vSync.SetIndex(PlayerPrefs.GetInt(VideoSettings_VSync, 1));     //0 - no vsync, 1 - vsync
        aa.SetIndex(PlayerPrefs.GetInt(VideoSettings_AntiAliasing, 3)); //Anti-Aliasing
        runInBackground.SetIndex(PlayerPrefs.GetInt(VideoSettings_RunInBackground, 0)); //0 - not run, 1 - run        
    }

    public void ApplyVideoSettings()        //When pressed "Apply" button in Video Settings
    {
        bool fullscreen = windowed.GetIndex != 0;   //Get the windowed option from windowed selector (it gets set with SetResolution). GetIndex = 0 when the options is windowed, 1 - when borderless
        Screen.SetResolution(resolution.Option.width, resolution.Option.height, fullscreen);    //Set resolution and windowed mode depending on resolution and windowed selectors
        QualitySettings.vSyncCount = vSync.GetIndex;
        QualitySettings.antiAliasing = (int)Mathf.Pow(2, aa.GetIndex);    //x2 = 2, x4 = 4, x8 = 8, but indexes are 0,1,2,3, that's why convert them by powering of 2
        Application.runInBackground = runInBackground.GetIndex != 0;    //0 - not run, 1 - run 

        PlayerPrefs.SetInt(VideoSettings_VSync, vSync.GetIndex);
        PlayerPrefs.SetInt(VideoSettings_AntiAliasing, aa.GetIndex);    //Save PlayerPrefs of only those, because resolution and windowed states are saved by Unity automatically
        PlayerPrefs.SetInt(VideoSettings_RunInBackground, runInBackground.GetIndex);
    }

    public void ApplyFOV()  //Function that runs when user changes FOV selector value
    {
        GameController.Controller.PlayerOne.camera.fieldOfView = fov.Option;    //Set FOVs for both players
        GameController.Controller.PlayerTwo.camera.fieldOfView = fov.Option;
    }

    public void LoadFOVValue()
    {
        fov.SetValue(PlayerPrefs.GetInt(VideoSettings_FOV, 60));       //Loads selector value from PlayerPrefs, 60 is default FOV
    }

    public void SaveFOVValue()
    {
        PlayerPrefs.SetInt(VideoSettings_FOV, fov.Option);  //Saves FOV Value when backing from settings
    }

    //Key bindings are handled in its own script
    
}
