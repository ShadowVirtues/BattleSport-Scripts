using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;  //Whatever first selected button we choose to be when the scene load

    [SerializeField] private AudioSource music; //Reference to music audiosource of Main Menu
    
    [SerializeField] private GameObject blockInputPanel;    //"Panel" over the whole screen to block mouse input when needed
    
    private static bool firstLaunch = true; //A flag so we apply settings only when firstly launched the game, not when we come back to main menu from either scene
    
    [SerializeField] private GameObject mainMenu;       //Reference to MainMenu and SettingsMenu panels to enable-disable them when navigating menu
    [SerializeField] private GameObject settingsMenu;
    
    void Awake()
    {
        settingsMenu.SetActive(true);               //Enable settings menu panel, so its children actually get awaken       
        settingsMenu.SetActive(false);      //Disable it after
        
        if (firstLaunch) ApplySettingsOnStartup();  //Only if the game is initially launched, apply the settings
        
        Time.timeScale = 1;         //Just in case, set timeScale to 1, if we didn't do it while quitting the game (that gets paused)

        blockInputPanel.SetActive(false);   //Disable it in case if was active for some reason
        Destroy(GameObject.Find(nameof(StartupController)));    //Destroy StartupController of this scene (cuz it gets DontDestroyOnLoad in its Awake). We only need to destroy it when we go back to main menu
       
        music.Play();           //Play the music 

        CustomInputModule.Instance.Enabled = true;          //Make sure Menu input is enabled after we disable it for 0.5 delay after pressing some button
        CustomInputModule.Instance.GetComponent<EventSystem>().enabled = true;  //Enable event system if it was disabled for some reason. Before we get back to a menu from game, we disable event system (for 0.5 delay)
        Cursor.lockState = CursorLockMode.None;       //Enable cursor in case it was disabled
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(firstSelected);  //Select some button
        
    }

    void Update()       //DELETE
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {

            //string[] asdf = Input.GetJoystickNames();
            //for (int i = 0; i < asdf.Length; i++)
            //{
            //    Debug.LogError(i + ". " + asdf[i]);
            //}
            
        }
        
    }

    public void InstantActionSetup()    //When pressed "Instant Action Setup"
    {      
        StartCoroutine(MenuSelect());
    }

    private IEnumerator MenuSelect()    //This launches when pressed TODO any menu maybe
    {
        music.Stop();       //Stop the music playing
        
        CustomInputModule.Instance.PlaySelect();     //Play 'select' sound  
        disableInput();

        yield return new WaitForSecondsRealtime(0.5f);  //Wait 

        SceneManager.LoadScene("InstantActionSetup");

    }

    private void disableInput() //Function to disable player input after he clicked some button to go to next menu (since we have a small delay after pressing this button, when we don't want player to select something else)
    {
        CustomInputModule.Instance.Enabled = false;        //Block key input with EventSystem
        blockInputPanel.SetActive(true);                  //Enable the panel in front of everything that blocks mouse input
    }

    private void ApplySettingsOnStartup()
    {
        //Resolution and windowed state get saved automatically by Unity
        QualitySettings.vSyncCount = PlayerPrefs.GetInt(SettingsMenu.VideoSettings_VSync, 1);                                  //Apply VSync from PlayerPrefs
        QualitySettings.antiAliasing = (int)Mathf.Pow(2, PlayerPrefs.GetInt(SettingsMenu.VideoSettings_AntiAliasing, 1));      //Anti-Aliasing
        Application.runInBackground = PlayerPrefs.GetInt(SettingsMenu.VideoSettings_RunInBackground, 0) != 0;                  //Run-in-Background option

        //Sound options get applied by initializing settings menu
        
        InputManager.Load();    //Load keybindings
        
        firstLaunch = false;        //After applying all the settings, set the flag
    }

    public void BackFromSettings()          //Navigating in Main Menu settings uses functions from SettingsMenu.cs, but when exiting settings, we need custom function, which is this one
    {
        settingsMenu.SetActive(false);
        mainMenu.SetActive(true);     
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
    

}
