using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels/Menus")]
    [SerializeField] protected GameObject mainPanel;          //Reference specifically to Main panel to enable it when invoking pause menu
    [SerializeField] private GameObject settingsMenu;           //Reference to a container for all settings panels    
    [SerializeField] private GameObject[] allPanels;        //Reference to all other menu panels to disable all of them every time switching some menu (for 'general implementation' of menu switching)
 
    [Header("Other")]
    [SerializeField] private Button resume; //Reference specifically to "Resume Game" button to select it when invoking pause menu (all other ones are getting selecting from 'general implementation' - see further)   

    private RectTransform rectTransform;                //Rect transform of pause menu is used to position pause menu on specific player screen side when we show it

    [HideInInspector] public EventSystem eventSystem;     //The event system is getting used only in pause menu, and other than that once during end-game menu "Replay Game/Return to Menu". 
                                                          //We can't use EventSystem.current, cuz we disable it during gameplay, and it returns 'EventSystem.current = null' if it is disabled
    
    protected virtual void Awake()                      //Settings menu inherits this class, and has its own Awake
    {
        rectTransform = GetComponent<RectTransform>();      //Get the reference
        eventSystem = EventSystem.current;                  //EventSystem gets instantiated in enabled state, and that's how we get a reference to it
        eventSystem.enabled = false;                                        //We got all the references, now disable event system, it gets enabled when some player pauses the game
        Cursor.lockState = CursorLockMode.Locked;       //Disable cursor (this happens when the arena scene loads)
        Cursor.visible = false;

        settingsMenu.SetActive(true);               //Enable settings menu panel, so its children actually get awaken      
        settingsMenu.SetActive(false);      //Disable it after
        
        gameObject.SetActive(false);                   //Pause menu instantiates in GameUI in enabled state to run this Awake, we need to disable it in the end of it so its OnEnable doesn't run 
    }

    protected virtual void OnEnable()     //Runs when some player pauses the game and the menu shows
    {
        if (GameController.Controller.PausedPlayer == PlayerID.One) //If it was player one who paused the game
        {
            CustomInputModule.Instance.PlayerOne = true;  //Disable player2 input and enable player1 input through EventSystem
            CustomInputModule.Instance.PlayerTwo = false;
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)    //Same, but opposite
        {
            CustomInputModule.Instance.PlayerOne = false;
            CustomInputModule.Instance.PlayerTwo = true;
        }
        
        SetPauseMenuPosition(); //Position pause menu

        eventSystem.enabled = true;     //Enable event system, so players can navigate the menu
        Cursor.lockState = CursorLockMode.None;       //Enable cursor when entering a pause menu
        Cursor.visible = true;
        
        CustomInputModule.Instance.PlaySelect();            //When invoking pause menu, play 'Select' sound

        DisableAllPanelsAndEnableOne(mainPanel);    //Disable all panels if the menu was saved with some random one active, and enable the main panel

        eventSystem.SetSelectedGameObject(null);    //Without this, when invoking pause menu, the "Resume" button wouldn't highlight for some reason (but it still would be selected for navigation)
        eventSystem.SetSelectedGameObject(resume.gameObject);  //Yeah, select "Resume" button

        SetCurrentBackButton(resume);       //Set so when pressing "Cancel" on keyboard or joystick, it "presses" resume button

        System.GC.Collect();        //Collect garbage when pressed pause
    }

    public void SetPauseMenuPosition()  //Function positioning pause menu dependion on which player invoked it and what splitscreen type is set. Public, because we invoke it when changing the splitscreen type from SettingsMenu
    {        
        if (GameController.Controller.PausedPlayer == PlayerID.One) //If it was player one who paused the game
        {
            if (GameController.Controller.IsSplitScreenVertical)    //If vertical split-screen
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);  //Set anchor in the middle of the screen
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = new Vector2(-480, 0);  //Position menu to the left of the middle
            }
            else            //If horizontal
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1);     //For player one, set anchor to top of the screen
                rectTransform.anchorMax = new Vector2(0.5f, 1);

                rectTransform.anchoredPosition = new Vector2(0, -340);  //Set position relative to top of the screen
            }           
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)    //Same, but opposite
        {
            if (GameController.Controller.IsSplitScreenVertical)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = new Vector2(480, 0);  //To the right
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0);     //Anchor to botton
                rectTransform.anchorMax = new Vector2(0.5f, 0);

                rectTransform.anchoredPosition = new Vector2(0, 340);  //Relative to bottom  
            }
        }
    }

    private const string cancelButton = "Pause";        //Caching string
    private static Button currentBackButton;            //Each menu you enter has its "Back" button, which gets invoked if user presses "Cancel" button, we set this button after entering each menu

    protected virtual void Update()   //Query "Cancel" button when the pause menu is active (to get back to previous menu from pressing it)
    {
        if (CustomInputModule.Instance.Menu && EventSystem.current != null)    //If the flag is set to "Menu", process any available cancel buttons
        {
            bool pressed = false;   //Flag to fill in true if some button got pressed

            pressed |= InputManager.GetKeyDown(KeyCode.Escape); //Evaluating all Cancel buttons
            pressed |= InputManager.GetKeyDown(KeyCode.Backspace);
            pressed |= InputManager.GetKeyDown(KeyCode.Joystick1Button6);
            if (CustomInputModule.joyNum > 1)   //And for additional joysticks, if any
            {
                for (int i = 1; i < CustomInputModule.joyNum; i++)
                {
                    pressed |= InputManager.GetKeyDown((KeyCode)(356 + i * 20));    //And additional joysticks if there are any
                }
            }

            if (pressed)    //In the end if some Cancel button got pressed
            {
                currentBackButton.onClick.Invoke(); //Invoke pressing a button that was specified
            }
        }
        else if (EventSystem.current != null && InputManager.GetButtonDown(cancelButton, GameController.Controller.PausedPlayer))   
        {   //If paused player pressed cancel button and if we process input (because sometimes we block it, when fading out menu and binding buttons)
            //Quering EventSystem first, cuz when binding keys in menu it's null, so the second condition here doesn't even get asked (so GameController doesn't raise an exception)
            currentBackButton.onClick.Invoke(); //Invoke pressing a button that was specified
        }
        
    }

    public void SetCurrentBackButton(Button toSet)  //Public function that is tied to each button in the menu, specifying which back button is current after pressing that button
    {
        currentBackButton = toSet;         
    }
    
    public void ResumeGame()
    {
        eventSystem.enabled = false;        //Disable event system during gameplay
        Cursor.lockState = CursorLockMode.Locked;       //And disable cursor
        Cursor.visible = false;

        gameObject.SetActive(false);        //Disable pause menu
        GameController.Controller.gameUI.gameObject.SetActive(false);   //Disable the whole Game UI canvas
        System.GC.Collect();
        GameController.Controller.UnPause();                    //And finally unpause the game
    }
    
    public virtual void DisableAllPanelsAndEnableOne(GameObject toEnable)   //This is 'general implementation' of menu switching, this function and the next one are applied to every menu-switching button, having the respective parameter 
    {                                                               //In this case it is the menu panel that gets shown (enabled) after selecting this menu
        mainPanel.SetActive(false);                               //Disabling main panel
        settingsMenu.SetActive(false);                          //Disabling settings menu       
        foreach (GameObject panel in allPanels)              //Disabling all other possible panels of Pause Menu
        {
            panel.SetActive(false);
        }

        if (toEnable != null) toEnable.SetActive(true);     //And enable the panel that we navigate to (function accepts no menu to enable, so don't enable anything if nothing was passed into function)
    }
    
    public void PlaySoundAndSelectOption(GameObject toSelect)   //Second function of 'general implementation', it plays the 'Select' sound when selecting the menu, and selects/highlights the respective menu option
    {
        CustomInputModule.Instance.PlaySelect();       
        eventSystem.SetSelectedGameObject(toSelect);
    }
    
    public void QuitMatch()             //Function that is tied to button "Yes" in the respective menu
    {
        eventSystem.enabled = false;    //Disable event system, so during fading out animation, player couldn't navigate the menu        
        GameController.Controller.gameUI.QuitMatch();   //Run a function on the side of Game UI to fade the screen and all the rest
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
