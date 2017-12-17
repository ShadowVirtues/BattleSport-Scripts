using System;
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
    [SerializeField] private GameObject keyboardMousePanel;
    [SerializeField] private GameObject gamepadPanel;

    [Header("Keyboard Panel")]
    [SerializeField] private GameObject turnLeft;
    [SerializeField] private Selectable shootBallKeyboard;

    [Header("Keyboard+Mouse Panel")]
    [SerializeField] private ValueSelector sensitivityMouse;
    [SerializeField] private Selectable shootBallMouse;
    


    [Header("Gamepad Panel")]
    [SerializeField] private StringSelector turningThrottling;
    [SerializeField] private ValueSelector sensitivityStick;
    [SerializeField] private ValueSelector deadZone;
    [SerializeField] private Text throttlingValue;

    [SerializeField] private GameObject[] stickSelectors;
    
    [Header("Bottom")]  
    [SerializeField] private Selectable defaultWASD;
    [SerializeField] private Selectable defaultArrows;
    [SerializeField] private Selectable back;




    private Selectable deviceSelectable;

    private CustomInputModule inputModule;
    

    void Awake()
    {
        deviceSelectable = device.GetComponent<Selectable>();
        inputModule = EventSystem.current.GetComponent<CustomInputModule>();
        
    }

    void OnEnable()
    {

        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {           
            playerLabel.text = "PLAYER ONE CONTROLS";
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {           
            playerLabel.text = "PLAYER TWO CONTROLS";
        }

        LoadKeyBindingsValues();

        inputModule.Menu = true;

    }

    void OnDisable()
    {
        inputModule.Menu = false;
    }


    private void LoadKeyBindingsValues()
    {
        device.UpdateDevices();

        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");
        
        if (playerDevice.description == "Keyboard")
        {
            device.SetIndex(0);
            turningThrottling.SetIndex(0);  //So when some player with joystick sets their turning, it doesnt transfer to keyboard player when he switches to gamepad (so for keyboard player default option is "Stick", and not whatever 'joystick player' set last)
        }
        else if (playerDevice.description == "Keyboard+Mouse")
        {
            device.SetIndex(1);
            turningThrottling.SetIndex(0);
        }
        else
        {
            int selectedGamepad = Int32.Parse(playerDevice.description.Substring(playerDevice.description.Length - 1, 1));
            
            if (Input.GetJoystickNames().Length < selectedGamepad + 1)
            {
                device.SetIndex(0);
            }
            else
            {
                device.SetIndex(2 + selectedGamepad);

                AxisConfiguration turningAxis = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");

                if (turningAxis.axis == 5)
                    turningThrottling.SetIndex(1);
                else if (turningAxis.axis == 0)
                    turningThrottling.SetIndex(0);

                TurningThrottlingChange();
            }
        }

        
        ChangeDevice();
        



        




    }

    //TODO ChangeDevice() when entering menu
    public void ChangeDevice()
    {
        if (device.GetIndex == 0)   //Keyboard
        {
            keyboardPanel.SetActive(true);
            keyboardMousePanel.SetActive(false);
            gamepadPanel.SetActive(false);

            defaultWASD.gameObject.SetActive(true);
            defaultArrows.gameObject.SetActive(true);

            setSelectableDown(deviceSelectable, turnLeft);                       
            setSelectableUp(back, defaultArrows);
            setSelectableUp(defaultWASD,shootBallKeyboard);
        }
        else if (device.GetIndex == 1)  //Keyboard+Mouse
        {
            keyboardMousePanel.SetActive(true);
            keyboardPanel.SetActive(false);           
            gamepadPanel.SetActive(false);

            defaultWASD.gameObject.SetActive(true);
            defaultArrows.gameObject.SetActive(true);

            setSelectableDown(deviceSelectable, sensitivityMouse);
            setSelectableUp(back, defaultArrows);
            setSelectableUp(defaultWASD, shootBallMouse);            
        }
        else                            //All gamepads
        {
            keyboardPanel.SetActive(false);
            keyboardMousePanel.SetActive(false);
            gamepadPanel.SetActive(true);

            defaultWASD.gameObject.SetActive(false);
            defaultArrows.gameObject.SetActive(false);

            setSelectableDown(deviceSelectable, turningThrottling);
            setSelectableUp(back, turningThrottling);
            
            TurningThrottlingChange();
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

    //TODO Are you sure - reset WASD/Arrows
    





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
