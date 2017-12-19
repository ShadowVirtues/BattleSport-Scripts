using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Base class for all menu selectors. Can't be applied to object itself, because doesn't have a specific "Options" field with all the options that you can select in the selector, that's why 'abstract'

public abstract class MenuSelector : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, ISelectHandler          //UI Interfaces
{
    [Header("Controls")]
    [SerializeField] private Text OptionName;     //Name of the option like "Arena", "PlayerOne"
    [SerializeField] protected Text OptionValue;    //The value we change, we use it in derived classes as well, to set default values on menu load for it
    [SerializeField] private Button Left;           //Button for the Previous Item for mouse clicks
    [SerializeField] private Button Right;          //Button for the Next Item for mouse clicks
    
    protected int index;                            //Index of currently selected item, used in derived classes

    public int GetIndex => index;                   //'Get' roperty to get the index, SetIndex is implemented in each selector as a method

    protected abstract string NextOption { get; }       //String, because every value has at least a string value to show on screen, the other UI fields that get changed by switching options are implemented in actual derived classes
    protected abstract string PreviousOption { get; }   //The usage for those properties is all the same and is defined here, in the base class, but getting this value is different for each derived selector

    private const string turningAxisName = "Turning";   //"Turning" buttons will switch between options for keyboard and controller input
    private const string strafingAxisName = "Strafing";   //"Turning" buttons will switch between options for keyboard and controller input


    [SerializeField] private UnityEvent unityEvent;     //Some Event that you invoke when you press any button (like applying some option instantly with changing it in settings). Will execute for the new value on the selector (to which it switches) 

    protected virtual void Awake()  //We need to call Awake from the base class, and derived ones. That's why it is virtual, so we can then override it in derived class and call base.Awake()
    {
        Left.gameObject.SetActive(false);   //For all the selectors, disable their arrows, they get enabled only for currently "selected" object that has focus
        Right.gameObject.SetActive(false);       

        Left.onClick.AddListener(PreviousItem);     //Add listener to switch the items
        Right.onClick.AddListener(NextItem);
        Left.onClick.AddListener(CustomInputModule.Instance.PlayClick);       //Add listener to play click sound
        Right.onClick.AddListener(CustomInputModule.Instance.PlayClick);
        Left.onClick.AddListener(unityEvent.Invoke);//Add listener to execute custom event for the selector (like applying some option in the settings)
        Right.onClick.AddListener(unityEvent.Invoke);
    }

    private bool isAxisInUse = false;     //Variable needed for processing GetAxis presses like ButtonDown

    private bool quickSwitching = false;  
    private float repeatDelay = 0.5f;
    private float switchDelay = 0.1f;       //Five variables to set up the system of holding down the button, which performs the 'press' once, and then starts quickly pressing it after some delay
    private float prevActionTime;
    private float pressDownTime;

    void Update()
    {
        if (EventSystem.current == null) return;

        if (EventSystem.current.currentSelectedGameObject == gameObject)    //If this selector is the active one, this is the selector we choose options for with Left-Right buttons on keyboard or controller
        {
            float axis = 0; //Initial variable to output a move direction in the end(values -1,0,1)
            if (CustomInputModule.Instance.Menu)  //Means we are in menu and all input is universal
            {                
                if (InputManager.GetKey(KeyCode.D)) axis += 1;
                if (InputManager.GetKey(KeyCode.A)) axis += -1;             //Process keyboard keys
                if (InputManager.GetKey(KeyCode.RightArrow)) axis += 1;
                if (InputManager.GetKey(KeyCode.LeftArrow)) axis += -1;

                if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[0, 0]) > CustomInputModule.dead) axis += 1;
                if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[0, 0]) < -CustomInputModule.dead) axis += -1;    //Process joystick 1 input from which always gets processed
                if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[0, 1]) > CustomInputModule.dead) axis += 1;      //Get the axis names and deadzone from CustomInputModule
                if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[0, 1]) < -CustomInputModule.dead) axis += -1;

                if (CustomInputModule.joyNum > 1)     //In case more than one joystick is connected, process their input as well
                {
                    for (int i = 1; i < CustomInputModule.joyNum; i++)
                    {
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[i, 0]) > CustomInputModule.dead) axis += 1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[i, 0]) < -CustomInputModule.dead) axis += -1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[i, 1]) > CustomInputModule.dead) axis += 1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[i, 1]) < -CustomInputModule.dead) axis += -1;
                    }
                }

                axis = Math.Sign(axis); //In the end if 20000000 buttons are pressed resulting value of 20000000, make it a value of 1

            }
            else    //Otherwise we are in game, where we only process input from the player which paused the game (Selectors are only in pause menu during the game)
            {
                if (GameController.Controller.PausedPlayer == PlayerID.One)
                {
                    if (CustomInputModule.Instance.PlayerOneDevice == 0)
                    {
                        axis = Math.Sign(InputManager.GetAxisRaw(turningAxisName, GameController.Controller.PausedPlayer));    //Using Math, not Mathf, so when value is 0, Sign returns 0
                    }
                    else if (CustomInputModule.Instance.PlayerOneDevice == 1)
                    {
                        axis = Math.Sign(InputManager.GetAxisRaw(strafingAxisName, GameController.Controller.PausedPlayer));    //Using Math, not Mathf, so when value is 0, Sign returns 0
                    }
                    else
                    {
                        if (InputManager.GetAxisRaw(turningAxisName, PlayerID.One) > CustomInputModule.dead) axis += 1;
                        if (InputManager.GetAxisRaw(turningAxisName, PlayerID.One) < -CustomInputModule.dead) axis += -1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerOneDevice - 2, 0]) > CustomInputModule.dead) axis += 1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerOneDevice - 2, 0]) < -CustomInputModule.dead) axis += -1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerOneDevice - 2, 1]) > CustomInputModule.dead) axis += 1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerOneDevice - 2, 1]) < -CustomInputModule.dead) axis += -1;
                        axis = Math.Sign(axis);
                    }
                }
                else if (GameController.Controller.PausedPlayer == PlayerID.Two)
                {
                    if (CustomInputModule.Instance.PlayerTwoDevice == 0)
                    {
                        axis = Math.Sign(InputManager.GetAxisRaw(turningAxisName, GameController.Controller.PausedPlayer));    //Using Math, not Mathf, so when value is 0, Sign returns 0
                    }
                    else if (CustomInputModule.Instance.PlayerTwoDevice == 1)
                    {
                        axis = Math.Sign(InputManager.GetAxisRaw(strafingAxisName, GameController.Controller.PausedPlayer));    //Using Math, not Mathf, so when value is 0, Sign returns 0
                    }
                    else
                    {
                        if (InputManager.GetAxisRaw(turningAxisName, PlayerID.Two) > CustomInputModule.dead) axis += 1;
                        if (InputManager.GetAxisRaw(turningAxisName, PlayerID.Two) < -CustomInputModule.dead) axis += -1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerTwoDevice - 2, 0]) > CustomInputModule.dead) axis += 1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerTwoDevice - 2, 0]) < -CustomInputModule.dead) axis += -1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerTwoDevice - 2, 1]) > CustomInputModule.dead) axis += 1;
                        if (Input.GetAxisRaw(CustomInputModule.joyAxisNamesHor[CustomInputModule.Instance.PlayerTwoDevice - 2, 1]) < -CustomInputModule.dead) axis += -1;
                        axis = Math.Sign(axis);
                    }
                }
                
            }
            
            //Following code handles player holding down a button, so option switches once, waits 0.5 seconds, and then starts quickly switching further
            if (axis != 0)  //If axis is actually pressed
            {
                if (isAxisInUse == false) //Here be manage the initial button press (so we can still quick tap the button to switch)
                {
                    Press(axis);    //Press the corresponding button

                    pressDownTime = Time.unscaledTime;  //Remember the time button got held down

                    isAxisInUse = true; //Until we release the axis button, this will remain true
                }
                else if (quickSwitching)    //If quick switching got activated in the next section
                {
                    if (Time.unscaledTime > prevActionTime + switchDelay)   //Delay switching by 0.1 sec
                    {
                        Press(axis);    //Press the button

                        prevActionTime = Time.unscaledTime; //Remember previous time quick switch happened
                    }
                }
                else    //This section waits for 0.5 second after pressing down a button and before starting quick switching
                {
                    if (Time.unscaledTime > pressDownTime + repeatDelay)    //When the time reaches 0.5 sec after pressing down a button
                    {
                        quickSwitching = true;  //Set this to true so on the next frame we enter quick switching
                    }
                }
            }
            if (axis == 0)  //And if the button is not pressed, or when it gets released
            {
                quickSwitching = false; //Disable quick switching, cuz we released the button
                isAxisInUse = false;    //Set the flag so the next press will switch the item
            }
            
        }

    }

    private void Press(float axis)  //Function to press the corresponding button depending on if "left" or "right" button was pressed
    {        
        if (axis == -1) //If axis is negative (Pressed Left)
        {            
            Left.onClick.Invoke();  //We simulate selector button press, so it is the same action to press a button on keyboard, or actual button on the screen with mouse
        }
        else if (axis == 1) //If axis is positive (Pressed Right)
        {
            Right.onClick.Invoke();
        }
    }

    public void NextItem()      //Public funciton that gets called from pressing controller buttons in Update here, and is set on Button Event
    {
        OptionValue.text = NextOption;  //Set the showed value in the UI to the next option in the list (NextOption is a 'get' property, and that's where the index of chosen option gets changed)
    }

    public void PreviousItem()
    {
        OptionValue.text = PreviousOption;
    }

    public void OnPointerEnter(PointerEventData eventData)  //This is so when we hover over the selectable, it gets focused and selected
    {
        if (EventSystem.current != null)
            if (!EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(gameObject);  //Set this selectable as current selected item (EventSystem automatically deselects previously selected selectable)
    }

    [Header("Colors")]
    [SerializeField] private Color optionSelected = Color.green;
    [SerializeField] private Color optionDeselected = Color.white;  //Menu selector colors. Colors for option and value text when they are and are not selected
    [SerializeField] private Color valueSelected = Color.white;
    [SerializeField] private Color valueDeselected = Color.blue;

    public void OnSelect(BaseEventData eventData)           //When this selectable gets selected
    {
        OptionName.color = optionSelected;         //Set OptionName color to green when it's selected
        OptionValue.color = valueSelected;        //OptionValue color to white
        Left.gameObject.SetActive(true);        //SetActive arrows for mouse selection
        Right.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)     //WHen the selectable gets deselected
    {
        OptionName.color = optionDeselected;
        OptionValue.color = valueDeselected;     //Set back all the colors and disable the arrows
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);

        quickSwitching = false;             //If we switch selector during holding left/right button, refresh the flags
        isAxisInUse = false;    
    }

    public void OnDisable()             //COMM
    {
        OptionName.color = optionDeselected;
        OptionValue.color = valueDeselected;     //Set back all the colors and disable the arrows
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);

        quickSwitching = false;             //If we switch selector during holding left/right button, refresh the flags
        isAxisInUse = false;
    }

}
