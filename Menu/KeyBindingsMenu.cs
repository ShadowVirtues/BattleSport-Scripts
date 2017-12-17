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
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject[] keyBindingsPanels;
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private GameObject keyboardMousePanel;
    [SerializeField] private GameObject gamepadPanel;

    [Header("Keyboard Panel")]
    [SerializeField] private GameObject turnLeft;
    [SerializeField] private Selectable shootBallKeyboard;
    [SerializeField] private Text cancelButtonKeyboard;
    [SerializeField] private Text submitButtonKeyboard;

    [Header("Keyboard+Mouse Panel")]
    [SerializeField] private ValueSelector sensitivityMouse;
    [SerializeField] private Selectable shootBallMouse;
    [SerializeField] private Text cancelButtonMouse;
    [SerializeField] private Text submitButtonMouse;


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
    

    void Awake()
    {
        deviceSelectable = device.GetComponent<Selectable>();        
    }

    void OnEnable()
    {

        DisableAllPanelsAndEnableOne(settingPanel);

        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {           
            playerLabel.text = "PLAYER ONE CONTROLS";
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {           
            playerLabel.text = "PLAYER TWO CONTROLS";
        }

        LoadKeyBindingsValues();

        CustomInputModule.Instance.Menu = true;

    }

    void OnDisable()
    {
        CustomInputModule.Instance.Menu = false;
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


            if (GameController.Controller.PausedPlayer == PlayerID.One)
            {
                cancelButtonKeyboard.text = "Escape";
                submitButtonKeyboard.text = "Enter";
            }
            else if (GameController.Controller.PausedPlayer == PlayerID.Two)
            {
                cancelButtonKeyboard.text = "Backspace";
                submitButtonKeyboard.text = "KeypadEnter";
            }


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

            if (GameController.Controller.PausedPlayer == PlayerID.One)
            {
                cancelButtonMouse.text = "Escape";
                submitButtonMouse.text = "Return";
            }
            else if (GameController.Controller.PausedPlayer == PlayerID.Two)
            {
                cancelButtonMouse.text = "Backspace";
                submitButtonMouse.text = "KeypadEnter";
            }
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
    public void DisableAllPanelsAndEnableOne(GameObject toEnable)   //This is 'general implementation' of menu switching, this function and the next one are applied to every menu-switching button, having the respective parameter 
    {                                                               //In this case it is the menu panel that gets shown (enabled) after selecting this menu        
        foreach (GameObject panel in keyBindingsPanels)              //Disabling all settings panels
        {
            panel.SetActive(false);
        }
        
        if (toEnable != null) toEnable.SetActive(true);     //And enable the panel that we navigate to (function accepts no menu to enable, so don't enable anything if nothing was passed into function)
    }

    public void ApplyControls()
    {
        //CHECK ALL CLEARING if you need to zero axis and set "Button" and shit
        //TODO Set jumpSingleButton for PlayerMovement

        if (device.GetIndex == 0 || device.GetIndex == 1)
        {
            AxisConfiguration cancelButton = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Pause");
            AxisConfiguration submitButton = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Start");
            if (GameController.Controller.PausedPlayer == PlayerID.One)
            {
                cancelButton.positive = KeyCode.Escape;
                submitButton.positive = KeyCode.Return;                
            }
            else if (GameController.Controller.PausedPlayer == PlayerID.Two)
            {
                cancelButton.positive = KeyCode.Backspace;
                submitButton.positive = KeyCode.KeypadEnter;               
            }

            AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");
            playerDevice.description = "Keyboard";

            if (device.GetIndex == 1)
            {
                AxisConfiguration turning = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");
                turning.type = InputType.MouseAxis;
                turning.axis = 0;
                //TODO Apply sensitivity

                playerDevice.description = "Keyboard+Mouse";
            }
        }       
        else
        {
            //TODO Clear jump button???
            //TODO DeadZone for player-specific menu controls

            int joystickIndex = device.GetIndex - 2;

            AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");

            AxisConfiguration throttling = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Throttle");
            AxisConfiguration turning = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");
            AxisConfiguration strafing = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Strafing");

            AxisConfiguration jump = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Jump");
            AxisConfiguration LB = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "LB");
            AxisConfiguration RB = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "RB");

            AxisConfiguration rocket = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Rocket");
            AxisConfiguration laser = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Laser");
            AxisConfiguration shootBall = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "ShootBall");
           
            AxisConfiguration cancelButton = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Pause");
            AxisConfiguration submitButton = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Start");

            playerDevice.description = "Joystick" + joystickIndex;      //CHECK WTF .ToString()

            if (turningThrottling.GetIndex == 0)    //Stick
            {
                throttling.type = InputType.AnalogAxis;
                throttling.axis = 1;
                throttling.invert = true;
                
                turning.type = InputType.AnalogAxis;
                turning.axis = 0;                
            }
            else if (turningThrottling.GetIndex == 1)   //D-Pad
            {
                throttling.type = InputType.AnalogAxis;
                throttling.axis = 6;

                turning.type = InputType.AnalogAxis;
                turning.axis = 5;
            }
            throttling.joystick = joystickIndex;
            turning.joystick = joystickIndex;

            strafing.type = InputType.DigitalAxis;
            strafing.positive = (KeyCode)(355 + joystickIndex * 20);    //JoystickXButton5
            strafing.negative = (KeyCode)(354 + joystickIndex * 20);    //JoystickXButton4

            jump.positive = KeyCode.None;

            LB.positive = (KeyCode)(354 + joystickIndex * 20);   //JoystickXButton5
            RB.positive = (KeyCode)(355 + joystickIndex * 20);   //JoystickXButton4

            rocket.positive = (KeyCode)(351 + joystickIndex * 20); //JoystickXButton1
            laser.positive = (KeyCode)(350 + joystickIndex * 20);    //JoystickXButton0
            shootBall.positive = (KeyCode)(353 + joystickIndex * 20); //JoystickXButton3

            cancelButton.positive = (KeyCode)(356 + joystickIndex * 20);    //JoystickXButton6
            submitButton.positive = (KeyCode)(357 + joystickIndex * 20);    //JoystickXButton7
            
        }





    }

    public void DefaultWASD()
    {
        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");
        
        AxisConfiguration throttling = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Throttle");
        AxisConfiguration turning = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");
        AxisConfiguration strafing = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Strafing");

        AxisConfiguration jump = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Jump");
        AxisConfiguration LB = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "LB");
        AxisConfiguration RB = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "RB");

        AxisConfiguration rocket = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Rocket");
        AxisConfiguration laser = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Laser");
        AxisConfiguration shootBall = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "ShootBall");

        if (device.GetIndex == 0)   //Keyboard
        {
            playerDevice.description = "Keyboard";

            throttling.type = InputType.DigitalAxis;
            throttling.positive = KeyCode.W;
            throttling.negative = KeyCode.S;

            turning.type = InputType.DigitalAxis;
            turning.positive = KeyCode.D;
            turning.negative = KeyCode.A;

            strafing.type = InputType.DigitalAxis;
            strafing.positive = KeyCode.J;  
            strafing.negative = KeyCode.G;               

            jump.positive = KeyCode.Space;

            LB.positive = KeyCode.None;  
            RB.positive = KeyCode.None;         

            rocket.positive = KeyCode.LeftShift; 
            laser.positive = KeyCode.K;    
            shootBall.positive = KeyCode.LeftControl;  
        }
        else if (device.GetIndex == 1)  //Keyboard+Mouse
        {
            playerDevice.description = "Keyboard+Mouse";

            throttling.type = InputType.DigitalAxis;
            throttling.positive = KeyCode.W;
            throttling.negative = KeyCode.S;
                
            strafing.type = InputType.DigitalAxis;
            strafing.positive = KeyCode.D; 
            strafing.negative = KeyCode.A;         

            jump.positive = KeyCode.Space;

            LB.positive = KeyCode.None; 
            RB.positive = KeyCode.None;       

            rocket.positive = KeyCode.Mouse0; 
            laser.positive = KeyCode.LeftShift;  
            shootBall.positive = KeyCode.Mouse1;      
        }
    }

    public void DefaultArrows()
    {
        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");

        AxisConfiguration throttling = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Throttle");
        AxisConfiguration turning = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");
        AxisConfiguration strafing = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Strafing");

        AxisConfiguration jump = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Jump");
        AxisConfiguration LB = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "LB");
        AxisConfiguration RB = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "RB");

        AxisConfiguration rocket = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Rocket");
        AxisConfiguration laser = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Laser");
        AxisConfiguration shootBall = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "ShootBall");

        if (device.GetIndex == 0)   //Keyboard
        {
            playerDevice.description = "Keyboard";

            throttling.type = InputType.DigitalAxis;
            throttling.positive = KeyCode.UpArrow;
            throttling.negative = KeyCode.DownArrow;

            turning.type = InputType.DigitalAxis;
            turning.positive = KeyCode.RightArrow;
            turning.negative = KeyCode.LeftArrow;

            strafing.type = InputType.DigitalAxis;
            strafing.positive = KeyCode.Keypad6;  
            strafing.negative = KeyCode.Keypad4;        

            jump.positive = KeyCode.Keypad0;

            LB.positive = KeyCode.None; 
            RB.positive = KeyCode.None;    

            rocket.positive = KeyCode.RightShift;
            laser.positive = KeyCode.KeypadPlus;    
            shootBall.positive = KeyCode.RightControl;      
        }
        else if (device.GetIndex == 1)  //Keyboard+Mouse
        {
            playerDevice.description = "Keyboard+Mouse";

            throttling.type = InputType.DigitalAxis;
            throttling.positive = KeyCode.UpArrow;
            throttling.negative = KeyCode.DownArrow;
            
            strafing.type = InputType.DigitalAxis;
            strafing.positive = KeyCode.RightArrow;
            strafing.negative = KeyCode.LeftArrow;     

            jump.positive = KeyCode.Keypad0;

            LB.positive = KeyCode.None;
            RB.positive = KeyCode.None;              

            rocket.positive = KeyCode.Mouse0; 
            laser.positive = KeyCode.LeftShift; 
            shootBall.positive = KeyCode.Mouse1;    
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
