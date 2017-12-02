using System.Collections;
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

    public int GetIndex => index;       //COMM

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

    void Update()
    {        
        if (EventSystem.current.currentSelectedGameObject == gameObject)    //If this selector is the active one, this is the selector we choose options for with Left-Right buttons on keyboard or controller
        {
            float axis = InputManager.GetAxisRaw(turningAxisName, PlayerID.One);    //TODO manage multiple players controlling

            if (axis != 0)  //If axis is actually pressed
            {
                if (isAxisInUse == false) //Only if axis wasn't previously pressed and held down    //TODO manage holding down a button
                {
                    if (axis == -1) //If axis is negative (Pressed Left)
                    {
                        //PreviousItem(); //Switch to previous item
                        Left.onClick.Invoke();
                    }
                    else if (axis == 1) //If axis is positive (Pressed Right)
                    {
                        //NextItem();     //Switch to next item
                        Right.onClick.Invoke();
                    }

                    isAxisInUse = true; //Until we release the axis button, this will remain true
                }
            }
            if (axis == 0)  //And if the button is not pressed, or when it gets released
            {
                isAxisInUse = false;    //Set the flag so the next press will switch the item
            }

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
    }















}
