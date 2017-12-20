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
    [SerializeField] private Text playerLabel;          //Label on top of the panel "Player X Controls"
    [SerializeField] private DeviceSelector device;     //Device selector. DUH

    [Header("Panels")]
    [SerializeField] private GameObject keySettingPanel;        //The panel where all the setting gets handled
    [SerializeField] private GameObject[] keyBindingsPanels;    //Different panels of KeyBinding panel, like "Are you sure" dialogs or error messages, includes KeySettingPanel as well
    [SerializeField] private GameObject keyboardPanel;          //Panel with keyboards setting
    [SerializeField] private GameObject keyboardMousePanel;     //Panel with keyboard and mouse setting
    [SerializeField] private GameObject gamepadPanel;           //Panel for gamepad setting

    [Header("Keyboard Panel")]      //All needed references on keyboard panel (to set some navigation stuff and to set the hardcoded Cancel and Enter buttons)
    [SerializeField] private GameObject turnLeft;
    [SerializeField] private Selectable shootBallKeyboard;
    [SerializeField] private Text cancelButtonKeyboard;
    [SerializeField] private Text submitButtonKeyboard;

    [Header("Keyboard+Mouse Panel")]    //All needed references on keyboard+mouse panel
    [SerializeField] private ValueSelector sensitivityMouse;
    [SerializeField] private Selectable shootBallMouse;
    [SerializeField] private Text cancelButtonMouse;
    [SerializeField] private Text submitButtonMouse;
    
    [Header("Gamepad Panel")]           //For gamepad
    [SerializeField] private StringSelector turningThrottling;
    [SerializeField] private ValueSelector sensitivityStick;
    [SerializeField] private ValueSelector deadZone;
    [SerializeField] private Text throttlingValue;

    [SerializeField] private GameObject[] stickSelectors;   //This is DeadZone and Sensitivity text fields to enable-disable them when switching D-Pad and Stick on selector
    
    [Header("Bottom")]              //References to bottom side of the menu buttons
    [SerializeField] private Selectable defaultWASD;
    [SerializeField] private Selectable defaultArrows;
    [SerializeField] private Selectable back;
    
    private Selectable deviceSelectable;    //Selectable component on device selector
    
    void Awake()
    {
        deviceSelectable = device.GetComponent<Selectable>();       //Getting it here
        
        keySettingPanel.SetActive(true);       
        keyboardMousePanel.SetActive(true);     //When the pause menu gets 'awaken' in the start of the scene with enable-disabling it in pause menu, do the same here (to initialize all selectors)
        gamepadPanel.SetActive(true);
        
        gamepadPanel.SetActive(false);
        keyboardMousePanel.SetActive(false);
        keySettingPanel.SetActive(false);

    }

    void OnEnable()     //When user enters key bindings menu
    {
        foreach (GameObject panel in keyBindingsPanels) //Disable all panels in case the menu prefab was saved with one active
        {
            panel.SetActive(false);
        }
        keySettingPanel.SetActive(true);    //Enable main key setting panel

        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {           
            playerLabel.text = "PLAYER ONE CONTROLS";   //Depending on paused player, set the label on top of the panel
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {           
            playerLabel.text = "PLAYER TWO CONTROLS";
        }

        LoadKeyBindingsValues();        //Run a function to load all values to the menu from settings

        CustomInputModule.Instance.Menu = true; //Make universal controls in the menu, so when players rebind the keys they don't get restricted to their previosly set buttons, or newly set
    }

    void OnDisable()    //When exiting key bindings menu, disable universal controls
    {
        CustomInputModule.Instance.Menu = false;
    }
    
    private void LoadKeyBindingsValues()    //Function to load all values to the menu from the settings
    {
        device.UpdateDevices();     //Update connected joysticks

        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");   //Getting those AxisConfigurations throughout the script, not gonna comment them
        //First parameter is the player for which to get the config, second is the "axis", which may be just a field like "DEVICE", or single button ((((Unity input system is absolute shit))))
        AxisConfiguration turningAxis = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");

        if (playerDevice.description == "Keyboard")     //The line says it clear
        {
            device.SetIndex(0);             //Set device selector to Keyboard
            turningThrottling.SetIndex(0);  //So when some player with joystick sets their turning, it doesnt transfer to keyboard player when he switches to gamepad (so for keyboard player default option is "Stick", and not whatever 'joystick player' set last)
            sensitivityStick.SetValue(25);  //Set default values for joystick panel, so when the player switched to them, they are not the last selected
            deadZone.SetValue(0);           //COMM FURTHER
            sensitivityMouse.SetValue(25);
        }
        else if (playerDevice.description == "Keyboard+Mouse")
        {
            device.SetIndex(1);
            turningThrottling.SetIndex(0);
            sensitivityStick.SetValue(25);
            deadZone.SetValue(0);
            sensitivityMouse.SetValue(Mathf.RoundToInt((turningAxis.sensitivity - 0.1f) * 100));
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
                
                if (turningAxis.axis == 5)  //D-Pad
                {
                    turningThrottling.SetIndex(1);
                    sensitivityStick.SetValue(25);
                    deadZone.SetValue(0);
                    sensitivityMouse.SetValue(25);
                }                  
                else if (turningAxis.axis == 0) //Stick
                {
                    turningThrottling.SetIndex(0);
                    sensitivityStick.SetValue(Mathf.RoundToInt((turningAxis.sensitivity - 0.5f) * 100));
                    deadZone.SetValue(Mathf.RoundToInt(turningAxis.deadZone * 100));
                    sensitivityMouse.SetValue(25);
                }
                    

                

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

    
    
    public void ApplyControls()
    {
        //CHECK ALL CLEARING if you need to zero axis and set "Button" and shit     
        
        if (device.GetIndex == 0 || device.GetIndex == 1)
        {
            AxisConfiguration throttling = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Throttle");
            throttling.invert = false;

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
            
            setJumpSingleButton(true);

            AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "DEVICE");
            playerDevice.description = "Keyboard";
            setPlayerDevice(0);

            setAnalogTurning(1);

            if (device.GetIndex == 1)
            {
                AxisConfiguration turning = InputManager.GetAxisConfiguration(GameController.Controller.PausedPlayer, "Turning");
                turning.type = InputType.MouseAxis;
                turning.axis = 0;
                setAnalogTurning(0.2f);
                turning.sensitivity = 0.1f + sensitivityMouse.Option / 100f;
                
                playerDevice.description = "Keyboard+Mouse";
                setPlayerDevice(1);
            }
        }       
        else
        {
            
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

            playerDevice.description = "Joystick" + joystickIndex;
            setPlayerDevice(joystickIndex + 2);

            if (turningThrottling.GetIndex == 0)    //Stick
            {
                throttling.type = InputType.AnalogAxis;
                throttling.axis = 1;
                throttling.invert = true;
                
                turning.type = InputType.AnalogAxis;
                turning.axis = 0;

                setAnalogTurning(3);

                turning.sensitivity = 0.5f + sensitivityStick.Option / 100f;
                turning.deadZone = deadZone.Option / 100f;

            }
            else if (turningThrottling.GetIndex == 1)   //D-Pad
            {
                throttling.type = InputType.AnalogAxis;
                throttling.axis = 6;
                throttling.invert = false;

                turning.type = InputType.AnalogAxis;
                turning.axis = 5;

                setAnalogTurning(1);
            }
            throttling.joystick = joystickIndex;
            turning.joystick = joystickIndex;

            strafing.type = InputType.DigitalAxis;
            strafing.positive = (KeyCode)(355 + joystickIndex * 20);    //JoystickXButton5
            strafing.negative = (KeyCode)(354 + joystickIndex * 20);    //JoystickXButton4

            jump.positive = KeyCode.None;
            setJumpSingleButton(false);

            LB.positive = (KeyCode)(354 + joystickIndex * 20);   //JoystickXButton5
            RB.positive = (KeyCode)(355 + joystickIndex * 20);   //JoystickXButton4

            rocket.positive = (KeyCode)(351 + joystickIndex * 20); //JoystickXButton1
            laser.positive = (KeyCode)(350 + joystickIndex * 20);    //JoystickXButton0
            shootBall.positive = (KeyCode)(353 + joystickIndex * 20); //JoystickXButton3

            cancelButton.positive = (KeyCode)(356 + joystickIndex * 20);    //JoystickXButton6
            submitButton.positive = (KeyCode)(357 + joystickIndex * 20);    //JoystickXButton7
            
        }

        InputManager.Save();



    }

    

    
    //TODO BESIDES CONTROLS, WE ALSO SET UP JUMP SINGLE BUTTON AND ANALOGTURNING IN PlayerMovement

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
            strafing.positive = KeyCode.Keypad3;  
            strafing.negative = KeyCode.Keypad1;        

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
            laser.positive = KeyCode.RightShift; 
            shootBall.positive = KeyCode.Mouse1;    
        }
    }

    [Header("Pressing Back")]
    [SerializeField] private Text[] allKeyboardKeys;
    [SerializeField] private Text[] allMouseKeys;   
    [SerializeField] private GameObject keyBindingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessage;
    [SerializeField] private Button okButton;
    [SerializeField] private Button backInSettings;


    public void BackFromKeyBindings()
    {
        bool denied = false;

        string cause = "YOU NEED TO SET\r\nALL THE KEYS\r\nBEFORE PROCEEDING!";

        if (device.GetIndex == 0)   //Keyboard
        {
            foreach (Text key in allKeyboardKeys)
            {
                if (key.text == "?")
                {
                    key.color = new Color(0.75f, 0, 0);
                    denied = true;
                }

            }
        }
        else if (device.GetIndex == 1)  //Keyboard+Mouse
        {
            foreach (Text key in allMouseKeys)
            {
                if (key.text == "?")
                {
                    key.color = new Color(0.75f, 0, 0);
                    denied = true;
                }

            }
        }
        else    //Any joystick
        {
            if (device.Option == "{Unplugged}")
            {
                denied = true;
                cause = "YEAH, NICE TRY!\r\nYOU CAN'T SELECT\r\nAN UNPLUGGED DEVICE!";
            }
        }
        

        if (denied)
        {
            keySettingPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(5000, 5000);
            //keySettingPanel.SetActive(false);
            errorMessage.text = cause;
            errorPanel.SetActive(true);
            CustomInputModule.Instance.PlaySelect();
            EventSystem.current.SetSelectedGameObject(okButton.gameObject);
            GetComponentInParent<PauseMenu>().SetCurrentBackButton(okButton);
            
        }
        else
        {
            gameObject.SetActive(false);
            settingsPanel.SetActive(true);
            CustomInputModule.Instance.PlaySelect();
            EventSystem.current.SetSelectedGameObject(keyBindingsButton);
            ApplyControls();
            GetComponentInParent<PauseMenu>().SetCurrentBackButton(backInSettings);
            
        }




        
    }

    public void HideSettingPanelAndEnableOne(GameObject toEnable)
    {
        keySettingPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(5000, 5000);
        toEnable.SetActive(true);
    }

    public void UnHideSettingPanelAndDisableOne(GameObject toDisable)
    {
        keySettingPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        toDisable.SetActive(false);
    }

    public void DisableEnableKeySettingPanel()
    {
        keySettingPanel.SetActive(false);
        keySettingPanel.SetActive(true);

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

    //=============================================

    private void setAnalogTurning(float turning)
    {
        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {
            GameController.Controller.PlayerOne.GetComponent<PlayerMovement>().analogTurning = turning;
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {
            GameController.Controller.PlayerTwo.GetComponent<PlayerMovement>().analogTurning = turning;
        }
    }

    private void setJumpSingleButton(bool state)
    {
        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {
            GameController.Controller.PlayerOne.GetComponent<PlayerMovement>().jumpSingleButton = state;
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {
            GameController.Controller.PlayerTwo.GetComponent<PlayerMovement>().jumpSingleButton = state;
        }
    }

    private void setPlayerDevice(int num)
    {
        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {
            CustomInputModule.Instance.PlayerOneDevice = num;
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {
            CustomInputModule.Instance.PlayerTwoDevice = num;
        }


    }




}
