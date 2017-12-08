using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//This is just to get normal buttons in the menu have the same selection interaction as our "MenuSelector"s
public class MenuButton : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)  //This is so when we hover over the selectable, it gets focused and selected
    {
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(gameObject);  //Set this selectable as current selected item (EventSystem automatically deselects previously selected selectable)
    }


    //private Text button;

    //void Awake()
    //{
    //    button = GetComponentInChildren<Text>();
    //}

    //public void OnDeselect(BaseEventData eventData)     //WHen the selectable gets deselected
    //{


    //    button.color = Color.white;

    //}
}

