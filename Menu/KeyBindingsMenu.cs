using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TeamUtility.IO.Examples;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyBindingsMenu : MonoBehaviour
{
    [SerializeField] private Text playerLabel;
    [SerializeField] private DeviceSelector device;

    [Header("Panels")]
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private GameObject gamepadPanel;

    [Header("Keyboard Panel")]
    [SerializeField] private GameObject turnLeft;
    [SerializeField] private GameObject turnRight;
    [SerializeField] private GameObject turningMouse;
    [SerializeField] private ValueSelector sensitivityMouse;
    [SerializeField] private Selectable throttleUp;
    [SerializeField] private Selectable shootBall;

    [Header("Gamepad Panel")]
    [SerializeField] private StringSelector turningThrottling;
    [SerializeField] private ValueSelector sensitivityStick;
    [SerializeField] private ValueSelector deadZone;
    [SerializeField] private Text throttlingValue;

    [SerializeField] private GameObject[] stickSelectors;
    
    [Header("Back")]
    [SerializeField] private Selectable back;


    //[SerializeField] private Navigation nav;

    private Selectable deviceSelectable;

    private CustomInputModule inputModule;
    

    void Awake()
    {
        deviceSelectable = device.GetComponent<Selectable>();
        inputModule = EventSystem.current.GetComponent<CustomInputModule>();
        
    }

    void OnEnable()
    {
        //RebindInput[] rebinds = GetComponentsInChildren<RebindInput>(true);

        //if (GameController.Controller.PausedPlayer == PlayerID.One)
        //{
        //    foreach (RebindInput rebind in rebinds)
        //    {
        //        rebind._inputConfigName = "PlayerOneConfiguration";   //TODO
        //    }
        //    playerLabel.text = "PLAYER ONE CONTROLS";
        //}
        //else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        //{
        //    foreach (RebindInput rebind in rebinds)
        //    {
        //        rebind._inputConfigName = "PlayerTwoConfiguration";   //TODO
        //    }
        //    playerLabel.text = "PLAYER TWO CONTROLS";
        //}

        inputModule.Menu = true;

    }

    void OnDisable()
    {
        inputModule.Menu = false;
    }


    public void LoadKeyBindingsValues()
    {




        //device.SetIndex();

    }

    //TODO ChangeDevice() when entering menu
    public void ChangeDevice()
    {
        if (device.GetIndex == 0)   //Keyboard
        {
            keyboardPanel.SetActive(true);
            gamepadPanel.SetActive(false);
            
            setSelectableDown(deviceSelectable, turnLeft);            
            setSelectableUp(throttleUp, turnRight);
            setSelectableUp(back, shootBall);
            
            turnLeft.SetActive(true);
            turnRight.SetActive(true);
            turningMouse.SetActive(false);
            sensitivityMouse.gameObject.SetActive(false);
            //Switch options selectables navigation
        }
        else if (device.GetIndex == 1)  //Keyboard+Mouse
        {
            keyboardPanel.SetActive(true);
            gamepadPanel.SetActive(false);
            
            setSelectableDown(deviceSelectable, sensitivityMouse);           
            setSelectableUp(throttleUp, sensitivityMouse);
            setSelectableUp(back, shootBall);

            turnLeft.SetActive(false);
            turnRight.SetActive(false);
            turningMouse.SetActive(true);
            sensitivityMouse.gameObject.SetActive(true);
            //Switch options selectables navigation
        }
        else
        {
            keyboardPanel.SetActive(false);
            gamepadPanel.SetActive(true);
            
            setSelectableDown(deviceSelectable, turningThrottling);
        }

        
    }

    //TODO manage changing it when device changed
    public void TurningThrottlingChange()
    {
        if (turningThrottling.GetIndex == 0)    //Stick
        {
            foreach (GameObject text in stickSelectors)
            {
                text.SetActive(true);
            }

            setSelectableDown(turningThrottling.GetComponent<Selectable>(), sensitivityStick);
            setSelectableUp(back, deadZone);

            throttlingValue.text = "LEFT STICK Y Axis";
        }
        else if (turningThrottling.GetIndex == 1)   //D-Pad
        {
            foreach (GameObject text in stickSelectors)
            {
                text.SetActive(false);
            }

            setSelectableDown(turningThrottling.GetComponent<Selectable>(), back);
            setSelectableUp(back, turningThrottling);

            throttlingValue.text = "D-PAD UP/DOWN";
        }
        
    }








    //=========================================

    private void setSelectableDown(Selectable selectable, Component select)
    {
        Navigation nav = selectable.navigation;
        nav.selectOnDown = select.GetComponent<Selectable>();
        selectable.navigation = nav;

    }
    private void setSelectableDown(Selectable selectable, GameObject select)
    {
        Navigation nav = selectable.navigation;
        nav.selectOnDown = select.GetComponent<Selectable>();
        selectable.navigation = nav;
    }

    private void setSelectableUp(Selectable selectable, Component select)
    {
        Navigation nav = selectable.navigation;
        nav.selectOnUp = select.GetComponent<Selectable>();
        selectable.navigation = nav;

    }
    private void setSelectableUp(Selectable selectable, GameObject select)
    {
        Navigation nav = selectable.navigation;
        nav.selectOnUp = select.GetComponent<Selectable>();
        selectable.navigation = nav;
    }










}
