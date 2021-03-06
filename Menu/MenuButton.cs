using UnityEngine;
using UnityEngine.EventSystems;

//This is just to get normal buttons in the menu have the same selection interaction as our "MenuSelector"s
public class MenuButton : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)  //This is so when we hover over the selectable, it gets focused and selected
    {
        if (EventSystem.current != null)
            if (!EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(gameObject);  //Set this selectable as current selected item (EventSystem automatically deselects previously selected selectable)
    }
    
}

