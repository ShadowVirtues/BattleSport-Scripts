using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using TeamUtility.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class KeySelector : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, ISelectHandler          //UI Interfaces
{
    [Header("Controls")]
    [SerializeField] private Text OptionName;     //Name of the option like "Arena", "PlayerOne"
    [SerializeField] protected Text OptionValue;    //The value we change, we use it in derived classes as well, to set default values on menu load for it
    
    public void OnPointerEnter(PointerEventData eventData)  //This is so when we hover over the selectable, it gets focused and selected
    {
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(gameObject);  //Set this selectable as current selected item (EventSystem automatically deselects previously selected selectable)
    }

    [Header("Colors")]
    [SerializeField]
    private Color optionSelected = Color.green;
    [SerializeField] private Color optionDeselected = Color.white;  //Menu selector colors. Colors for option and value text when they are and are not selected
    [SerializeField] private Color valueSelected = Color.white;
    [SerializeField] private Color valueDeselected = Color.blue;

    public void OnSelect(BaseEventData eventData)           //When this selectable gets selected
    {
        OptionName.color = optionSelected;         //Set OptionName color to green when it's selected
        OptionValue.color = valueSelected;        //OptionValue color to white
        
    }

    public void OnDeselect(BaseEventData eventData)     //WHen the selectable gets deselected
    {
        OptionName.color = optionDeselected;
        OptionValue.color = valueDeselected;     //Set back all the colors and disable the arrows
        
    }


}
