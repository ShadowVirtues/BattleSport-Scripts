using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Base class for all menu selectors. Can't be applied to object itself, because doesn't have a specific "Options" field with all the options that you can select in the selector, that's why 'abstract'

public abstract class MenuSelector : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, ISelectHandler          //UI Interfaces
{
    [SerializeField] private Text OptionName;     //Name of the option like "Arena", "PlayerOne"
    [SerializeField] protected Text OptionValue;    //The value we change, we use it in derived classes as well, to set default values on menu load for it
    [SerializeField] private Button Left;           //Button for the Previous Item for mouse clicks
    [SerializeField] private Button Right;          //Button for the Next Item for mouse clicks

    protected int index;                            //Index of currently selected item, used in derived classes

    public int GetIndex => index;                   //'Get' roperty to get the index, SetIndex is implemented in each selector as a method

    //protected abstract string Option { get; }       //TODO Delete this, if in the end we don't need general functionality for Option defined in this class somewhere   //Abstract fields that have to get implemented in derived classes, used to actually get and select data from "Options" container of derived classes
    protected abstract string NextOption { get; }       //String, because every value has at least a string value to show on screen, the other UI fields that get changed by switching options are implemented in actual derived classes
    protected abstract string PreviousOption { get; }   //The usage for those properties is all the same and is defined here, in the base class, but getting this value is different for each derived selector

    private const string turningAxisName = "Turning";   //"Turning" buttons will switch between options for keyboard and controller input

    void Start()
    {
        Left.gameObject.SetActive(false);   //For all the selectors, disable their arrows, they get enabled only for currently "selected" object that has focus
        Right.gameObject.SetActive(false);       
    }

    private bool isAxisInUse = false;     //Variable needed for processing GetAxis presses like ButtonDown
    private bool quickSwitching = false;   

    private float repeatDelay = 0.5f;
    private float switchDelay = 0.1f;
    private float prevActionTime;
    private float pressDownTime;

    void Update()
    {        
        if (EventSystem.current.currentSelectedGameObject == gameObject)    //If this selector is the active one, this is the selector we choose options for with Left-Right buttons on keyboard or controller
        {
            //Mergning both players input into one, so input is always either -1, 0, 1, even it both players are pressing buttons simultaneously
            float axis = Math.Sign(InputManager.GetAxisRaw(turningAxisName, PlayerID.One) + InputManager.GetAxisRaw(turningAxisName, PlayerID.Two));    //Using Math, not Mathf, so when value is 0, Sign returns 0
            
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

            //PREVIOUS IMPLEMENTATION, JUST IN CASE FOR NOW
            //if (axis != 0)  //If axis is actually pressed
            //{
            //    if (isAxisInUse == false) //Only if axis wasn't previously pressed and held down    
            //    {
            //        if (axis == -1) //If axis is negative (Pressed Left)
            //        {
            //            //PreviousItem(); //Switch to previous item
            //            Left.onClick.Invoke();
            //        }
            //        else if (axis == 1) //If axis is positive (Pressed Right)
            //        {
            //            //NextItem();     //Switch to next item
            //            Right.onClick.Invoke();
            //        }

            //        isAxisInUse = true; //Until we release the axis button, this will remain true
            //    }
            //}
            //if (axis == 0)  //And if the button is not pressed, or when it gets released
            //{
            //    isAxisInUse = false;    //Set the flag so the next press will switch the item
            //}


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
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(gameObject);  //Set this selectable as current selected item (EventSystem automatically deselects previously selected selectable)
    }

    public void OnSelect(BaseEventData eventData)           //When this selectable gets selected
    {
        OptionName.color = Color.green;         //Set OptionName color to green when it's selected
        OptionValue.color = Color.white;        //OptionValue color to white
        Left.gameObject.SetActive(true);        //SetActive arrows for mouse selection
        Right.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData)     //WHen the selectable gets deselected
    {
        OptionName.color = Color.white;
        OptionValue.color = Color.blue;     //Set back all the colors and disable the arrows
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);

        quickSwitching = false;
        isAxisInUse = false;    
    }















}
