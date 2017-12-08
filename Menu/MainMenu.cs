using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioSource music; //Reference to music audiosource of Main Menu
    [SerializeField] private AudioSource sfx;   //To SFX source
    [SerializeField] private AudioClip select;  //Clip to play one shot when pressed some button
    [SerializeField] private GameObject blockInputPanel;    //"Panel" over the whole screen to block mouse input when needed

    void Awake()
    {       
        Time.timeScale = 1;         //Just in case, set timeScale to 1, if we didn't do it while quitting the game (that gets paused)

        blockInputPanel.SetActive(false);   //Disable it in case if was active for some reason
        Destroy(GameObject.Find(nameof(StartupController)));    //Destroy StartupController of this scene (cuz it gets DontDestroyOnLoad in its Awake)
       
        music.Play();           //Play the music TODO if it's not disabled in options

        //EventSystem.current.GetComponent<TwoPlayerInputModule>().PlayerOne = true;     
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
        EventSystem.current.GetComponent<TwoPlayerInputModule>().PlayerOne = false; //Disable both player controls in EventSystem
        EventSystem.current.GetComponent<TwoPlayerInputModule>().PlayerTwo = false;
        blockInputPanel.SetActive(true);                                            //Enable the panel in front of everything that blocks mouse input
    }

}
