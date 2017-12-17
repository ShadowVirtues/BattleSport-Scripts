using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioSource music; //Reference to music audiosource of Main Menu
    [SerializeField] private AudioSource sfx;   //To SFX source
    [SerializeField] private AudioClip select;  //Clip to play one shot when pressed some button
    [SerializeField] private GameObject blockInputPanel;    //"Panel" over the whole screen to block mouse input when needed

    [SerializeField] private AudioMixer mixer;       //AudioMixer to set volume on startup

    private static bool firstLaunch = true; //A flag so we apply settings only when firstly launched the game, not when we come back to main menu from either scene

    void Awake()
    {          
        if (firstLaunch) ApplySettingsOnStartup();  //Only if the game is initially launched, apply the settings
        
        Time.timeScale = 1;         //Just in case, set timeScale to 1, if we didn't do it while quitting the game (that gets paused)

        blockInputPanel.SetActive(false);   //Disable it in case if was active for some reason
        Destroy(GameObject.Find(nameof(StartupController)));    //Destroy StartupController of this scene (cuz it gets DontDestroyOnLoad in its Awake)
       
        music.Play();           //Play the music 

        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {

            string[] asdf = Input.GetJoystickNames();
            for (int i = 0; i < asdf.Length; i++)
            {
                Debug.LogError(i + ". " + asdf[i]);
            }


        }

        //print(Input.GetAxisRaw("joy_0_axis_4"));

    }

    public void InstantActionSetup()    //When pressed "Instant Action Setup"
    {      
        StartCoroutine(MenuSelect());
    }

    private IEnumerator MenuSelect()    //This launches when pressed TODO any menu
    {
        music.Stop();       //Stop the music playing
        //yield return new WaitForSecondsRealtime(0.5f);
        sfx.PlayOneShot(select);    //Play "select" sound
        disableInput();

        yield return new WaitForSecondsRealtime(0.5f);  //Wait 

        SceneManager.LoadScene("InstantActionSetup");

    }

    private void disableInput() //Function to disable player input after he clicked some button to go to next menu (since we have a small delay after pressing this button, when we don't want player to select something else)
    {       
        EventSystem.current.GetComponent<CustomInputModule>().Enabled = false;        //Block key input with EventSystem
        blockInputPanel.SetActive(true);                                            //Enable the panel in front of everything that blocks mouse input
    }

    private void ApplySettingsOnStartup()
    {
        //Resolution and windowed state get saved automatically by unity
        QualitySettings.vSyncCount = PlayerPrefs.GetInt(PauseMenu.VideoSettings_VSync, 1);                                  //Apply VSync from PlayerPrefs
        QualitySettings.antiAliasing = (int)Mathf.Pow(2, PlayerPrefs.GetInt(PauseMenu.VideoSettings_AntiAliasing, 1));      //Anti-Aliasing
        Application.runInBackground = PlayerPrefs.GetInt(PauseMenu.VideoSettings_RunInBackground, 0) != 0;                  //Run-in-Background option

        float db = percentToDB(PlayerPrefs.GetInt(PauseMenu.SoundSettings_Master, 100));        //Set master volume
        mixer.SetFloat(PauseMenu.SoundSettings_Master, db);
        
        //TODO Menu sound options

        firstLaunch = false;        //After applying all the settings, set the flag
    }

    private float percentToDB(int percent)  //Function to convert Percents (0 - 100) to dB (-80 - 0)
    {
        int y = percent;
        return y == 0 ? -80 : 30 * Mathf.Log10(y / 100f);
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
