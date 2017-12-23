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
    [SerializeField] private ValueSelector[] stickSelectables;      //Along with disabling text fields, disabling ValueSelector script components so they can't be selected with mouse

    [Header("Bottom")]              //References to bottom side of the menu buttons
    [SerializeField] private Selectable defaultWASD;
    [SerializeField] private Selectable defaultArrows;
    [SerializeField] private Selectable back;
    
    private Selectable deviceSelectable;    //Selectable component on device selector (to set its navigation)

    private PlayerID PausedPlayer;      //General variable of which player to set up controls for. Gets set when paused in game, or in main menu when selected for which player to set controls

    void Awake()
    {
        deviceSelectable = device.GetComponent<Selectable>();       //Getting it from device
        
        keySettingPanel.SetActive(true);       
        keyboardMousePanel.SetActive(true);     //When the pause menu gets 'awaken' in the start of the scene with enable-disabling it in pause menu, do the same here (to initialize all selectors)
        gamepadPanel.SetActive(true);
        
        gamepadPanel.SetActive(false);
        keyboardMousePanel.SetActive(false);
        keySettingPanel.SetActive(false);

        PausedPlayer = PlayerID.One;        //Set the default paused player to avoid exceptions in case some Awake or OnEnable raises
    }

    void OnEnable()     //When user enters key bindings menu
    {
        foreach (GameObject panel in keyBindingsPanels) //Disable all panels in case the menu prefab was saved with one active
        {
            panel.SetActive(false);
        }
        keySettingPanel.SetActive(true);    //Enable main key setting panel

        if (GameController.Controller != null)  //Only if we are in a game, set paused player to one from GameController
        {
            PausedPlayer = GameController.Controller.PausedPlayer;
            RebindInput.PausedPlayer = GameController.Controller.PausedPlayer;      //Also set the static variable for RebindInput
        }
        //If we are in the menu, it will get set from menu button onClick event

        if (PausedPlayer == PlayerID.One)
        {           
            playerLabel.text = "PLAYER ONE CONTROLS";   //Depending on paused player, set the label on top of the panel
        }
        else if (PausedPlayer == PlayerID.Two)
        {           
            playerLabel.text = "PLAYER TWO CONTROLS";
        }

        LoadKeyBindingsValues();        //Run a function to load all values to the menu from settings

        CustomInputModule.Instance.Menu = true; //Make universal controls in the menu, so when players rebind the keys they don't get restricted to their previosly set buttons, or newly set
    }

    void OnDisable()    //When exiting key bindings menu, disable universal controls
    {
        if (GameController.Controller != null)          //When we are in the menu, we always have menu controls, so don't disable them
            CustomInputModule.Instance.Menu = false;
    }

    public void SetPausedPlayer(int toSet) //Public function that is tied to buttons in main menu to set "PausedPlayer"
    {
        PausedPlayer = (PlayerID)(toSet - 1);       //Parameters tied to buttons are 1 - PlayerOne, 2 - PlayerTwo. In enum they are 0 - PlayerOne, 1 - PlayerTwo, so converting them
        RebindInput.PausedPlayer = (PlayerID)(toSet - 1);
    }

    private void LoadKeyBindingsValues()    //Function to load all values to the menu from the settings
    {
        device.UpdateDevices();     //Update connected joysticks

        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(PausedPlayer, "DEVICE");   //Getting those AxisConfigurations throughout the script, not gonna comment them
        //First parameter is the player for which to get the config, second is the "axis", which may be just a field like "DEVICE", or single button ((((Unity input system is absolute shit))))
        AxisConfiguration turningAxis = InputManager.GetAxisConfiguration(PausedPlayer, "Turning");

        if (playerDevice.description == "Keyboard")     //The line says it clear
        {
            defaultDevice();        //Set keyboard, which is the default device, which gets fallen back to if some issue happens
        }
        else if (playerDevice.description == "Keyboard+Mouse")
        {
            device.SetIndex(1);                 //Setting device selector index
            turningThrottling.SetIndex(0);      //Setting default index for selector if used decides to switch to it 
            sensitivityStick.SetValue(25);      //We basically need to set all selectors, so they get operated appropriately, setting their index and the value they show
            deadZone.SetValue(0);
            sensitivityMouse.SetValue(Mathf.RoundToInt((turningAxis.sensitivity - 0.1f) * 100));    //For mouse sensitivity get it from value set in controls config (the 0-100 sensitivity is 0.1-1.1 sensitivity in config, so 0 sensitivity would move the mouse at all)
        }
        else
        {
            int selectedJoystick = 0;    //Declaring variable
            try
            {
                selectedJoystick = Int32.Parse(playerDevice.description.Substring(playerDevice.description.Length - 1, 1));  //Try to get the last character of the device string
            }
            catch (Exception)   //If some bullshit is written there (most likely by manually editing config)
            {
                defaultDevice();        //Set default device, which is keyboard
                ChangeDevice();         //Since we return from the function, run ChangeDevice to swith to keyboard tab in the menu (this method is getting called in the end of this function)
                return;                 //Returning, cuz if error occured, we don't want to use the unassigned "selectedJoystick"
            }
            
            if (Input.GetJoystickNames().Length - 1 < selectedJoystick)  //If specified joystick index in the config is largher than the amount of joysticks 
            {                                                           //(this gets invoked only when entering key bindings, so when entered game, player can freely connect the joystick and play without reconfiguring anything)
                defaultDevice();        //Setting to default device, which is keyboard     
            }
            else    //If we are still in range of connected joysticks
            {
                device.SetIndex(2 + selectedJoystick);  //Set index to a correcponding joysticks (joysticks are numbered from 0, but their device index from 2)
                
                if (turningAxis.axis == 5)  //D-Pad
                {
                    turningThrottling.SetIndex(1);  //If D-Pad is set in config, set its index in selector
                    sensitivityStick.SetValue(25);  //And set default values for all other selectors
                    deadZone.SetValue(0);
                    sensitivityMouse.SetValue(25);
                }                  
                else if (turningAxis.axis == 0) //Stick
                {
                    turningThrottling.SetIndex(0);  //Set it on selector
                    sensitivityStick.SetValue(Mathf.RoundToInt((turningAxis.sensitivity - 0.5f) * 100));    //Get sensitivity from config (0-100 is 0.5-1.5 in config)
                    deadZone.SetValue(Mathf.RoundToInt(turningAxis.deadZone * 100));                        //Get deadZone from config (0-100 is 0-1 in config)
                    sensitivityMouse.SetValue(25);
                }
                  
                TurningThrottlingChange();      //Run a function that depending on a selector state, changes some menu fields/selectors
            }
        }
        
        ChangeDevice(); //Run a function that shows specific panel with specific for Keyboard/Keyboard+Mouse/Joystick fields
        
    }

    private void defaultDevice()        //Used a couple of times here, so get this all in a function
    {
        device.SetIndex(0);             //Set device selector to Keyboard
        turningThrottling.SetIndex(0);  //So when some player with joystick sets their turning, it doesnt transfer to keyboard player when he switches to gamepad (so for keyboard player default option is "Stick", and not whatever 'joystick player' set last)
        sensitivityStick.SetValue(25);  //Set default values for joystick panel, so when the player switched to them, they are not the last selected
        deadZone.SetValue(0);           
        sensitivityMouse.SetValue(25);  
    }

    public void ChangeDevice()
    {
        if (device.GetIndex == 0)   //Keyboard
        {
            keyboardPanel.SetActive(true);      //Enable keyboard panel and disable the rest
            keyboardMousePanel.SetActive(false);
            gamepadPanel.SetActive(false);

            defaultWASD.gameObject.SetActive(true);     //Enable bottom buttons (only for keyboard/mouse)
            defaultArrows.gameObject.SetActive(true);

            setSelectableDown(deviceSelectable, turnLeft);   //Set selectables so key navigation is proper
            setSelectableUp(back, defaultArrows);            //In those functions, first parameter is what to set, second is TO what to set
            setSelectableUp(defaultWASD, shootBallKeyboard);
            
            if (PausedPlayer == PlayerID.One)
            {
                cancelButtonKeyboard.text = "Escape";   //For specific player set their default hardcoded changeable-only-manually-editing-config-file
                submitButtonKeyboard.text = "Space";    //This is because its super clumsy to bind those buttons (them being buttons that start and cancel binding process)
            }
            else if (PausedPlayer == PlayerID.Two)
            {
                cancelButtonKeyboard.text = "Backspace";
                submitButtonKeyboard.text = "Return";
            }
            
        }
        else if (device.GetIndex == 1)  //Keyboard+Mouse
        {
            keyboardMousePanel.SetActive(true); //Pretty much all the same as for keyboard
            keyboardPanel.SetActive(false);           
            gamepadPanel.SetActive(false);

            defaultWASD.gameObject.SetActive(true);
            defaultArrows.gameObject.SetActive(true);

            setSelectableDown(deviceSelectable, sensitivityMouse);
            setSelectableUp(back, defaultArrows);
            setSelectableUp(defaultWASD, shootBallMouse);

            if (PausedPlayer == PlayerID.One)
            {
                cancelButtonMouse.text = "Escape";
                submitButtonMouse.text = "Space";
            }
            else if (PausedPlayer == PlayerID.Two)
            {
                cancelButtonMouse.text = "Backspace";
                submitButtonMouse.text = "Return";
            }
        }
        else                            //All joystick
        {
            keyboardPanel.SetActive(false);
            keyboardMousePanel.SetActive(false);
            gamepadPanel.SetActive(true);

            defaultWASD.gameObject.SetActive(false);        //Disable botton buttons for binding default keyboard keys
            defaultArrows.gameObject.SetActive(false);

            setSelectableDown(deviceSelectable, turningThrottling);
            setSelectableUp(back, turningThrottling);
            
            TurningThrottlingChange();      //Change joystick panel depending on if D-Pad or stick is selected
        }

    }

    public void TurningThrottlingChange()       //Yeah, this function
    {
        if (turningThrottling.GetIndex == 0)    //Stick
        {
            //We don't want to disable whole selectors when switching between D-Pad and Stick, because that would move Vertical Layout group layout, so we are leaving objects in there, but disabling it visually and interactibly
            foreach (GameObject text in stickSelectors) //Enable text fields of sensitivity and deadZone selectors
            {
                text.SetActive(true);
            }
            foreach (ValueSelector selectable in stickSelectables)  //Enable script components which handle their selection with mouse
            {
                selectable.enabled = true;
            }

            setSelectableDown(turningThrottling.GetComponent<Selectable>(), sensitivityStick);  //Set navigation
            setSelectableUp(back, deadZone);

            throttlingValue.text = "LEFT STICK Y Axis"; //Write that throttling is controlled by Stick
        }
        else if (turningThrottling.GetIndex == 1)   //D-Pad
        {
            foreach (GameObject text in stickSelectors) //Disable text fields of sensitivity and deadZone selectors
            {
                text.SetActive(false);
            }
            foreach (ValueSelector selectable in stickSelectables)  //Disable script components which handle their selection with mouse
            {
                selectable.enabled = false;
            }

            setSelectableDown(turningThrottling.GetComponent<Selectable>(), back);  //Set navigation
            setSelectableUp(back, turningThrottling);

            throttlingValue.text = "D-PAD UP/DOWN";     //Write that throttling is controlled by D-Pad
        }
        
    }
    
    public void ApplyControls()     //Function that runs when successfully gone back from keybindings menu
    {
        //Keyboard controls when getting bound are instantly getting set to the InputManager config (that's how they made it work), so we just set all the rest but button binds here
        //If you bound something for keyboard, but then switched back to joystick, everything will get cleared properly

        if (device.GetIndex == 0 || device.GetIndex == 1)   //Applying keyboard and keyboard+mouse has a lot of similarities, so doing a lot of stuff for them both, but if keyboard+mouse is selected, overriding some stuff with its specific settings
        {
            AxisConfiguration throttling = InputManager.GetAxisConfiguration(PausedPlayer, "Throttle"); //Getting all axis configurations from InputManager config, setting axis types and clearing each axis accordingly
            throttling.type = InputType.DigitalAxis;
            throttling.ClearDigitalAxis();      //Means everything but positive-negative buttons and axis type is cleared
            AxisConfiguration turning = InputManager.GetAxisConfiguration(PausedPlayer, "Turning");
            turning.type = InputType.DigitalAxis;
            turning.ClearDigitalAxis();
            AxisConfiguration strafing = InputManager.GetAxisConfiguration(PausedPlayer, "Strafing");
            strafing.type = InputType.DigitalAxis;
            strafing.ClearDigitalAxis();

            AxisConfiguration jump = InputManager.GetAxisConfiguration(PausedPlayer, "Jump");
            jump.ClearButton();     //Means everything but positive button is cleared
            AxisConfiguration LB = InputManager.GetAxisConfiguration(PausedPlayer, "LB");
            LB.ClearCompletely();
            AxisConfiguration RB = InputManager.GetAxisConfiguration(PausedPlayer, "RB");
            RB.ClearCompletely();

            AxisConfiguration rocket = InputManager.GetAxisConfiguration(PausedPlayer, "Rocket");
            rocket.ClearButton();
            AxisConfiguration laser = InputManager.GetAxisConfiguration(PausedPlayer, "Laser");
            laser.ClearButton();
            AxisConfiguration shootBall = InputManager.GetAxisConfiguration(PausedPlayer, "ShootBall");
            shootBall.ClearButton();

            AxisConfiguration cancelButton = InputManager.GetAxisConfiguration(PausedPlayer, "Pause");
            cancelButton.ClearButton();
            AxisConfiguration submitButton = InputManager.GetAxisConfiguration(PausedPlayer, "Start");
            submitButton.ClearButton();
            if (PausedPlayer == PlayerID.One)   //Set default cancel-submit buttons to specific player (hardcoded)
            {
                cancelButton.positive = KeyCode.Escape;
                submitButton.positive = KeyCode.Space;                
            }
            else if (PausedPlayer == PlayerID.Two)
            {
                cancelButton.positive = KeyCode.Backspace;
                submitButton.positive = KeyCode.Return;               
            }

            AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(PausedPlayer, "DEVICE");
            playerDevice.description = "Keyboard";
            setPlayerDevice(0);             //Setting player device in CustomInputModule
            setJumpSingleButton(true);      //Setting if jumping is handled with one button in PlayerMovement 
            setAnalogTurning(1);            //Setting so sensitivity saved in config gets lead to appropriate values depending on the device 

            if (device.GetIndex == 1)       //All written before was the same for both keyboard and keyboard+mouse, so if the device is mouse, override some settings
            {
                turning.ClearCompletely();  //Means clear the whole axis
                turning.type = InputType.MouseAxis; //Set type to mouse
                turning.axis = 0;       //Set X axis (left-right)
                setAnalogTurning(0.2f); //Set factor for turning, so mouse is appropriately sensitive
                turning.sensitivity = 0.1f + sensitivityMouse.Option / 100f;        //Convert from 0-100 to 0.1-1.1 that gets saved in config
                
                playerDevice.description = "Keyboard+Mouse";    //Set device
                setPlayerDevice(1);    //Setting player device in CustomInputModule
            }
        }       
        else    //Joystick
        {            
            int joystickIndex = device.GetIndex - 2;    //Converting from 2-based selector jostick indexes to 0-based joystick number (x-based means numbering starts from 'x")

            AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(PausedPlayer, "DEVICE");

            AxisConfiguration throttling = InputManager.GetAxisConfiguration(PausedPlayer, "Throttle");
            AxisConfiguration turning = InputManager.GetAxisConfiguration(PausedPlayer, "Turning");     //Getting all axis
            AxisConfiguration strafing = InputManager.GetAxisConfiguration(PausedPlayer, "Strafing");

            AxisConfiguration jump = InputManager.GetAxisConfiguration(PausedPlayer, "Jump");
            AxisConfiguration LB = InputManager.GetAxisConfiguration(PausedPlayer, "LB");
            AxisConfiguration RB = InputManager.GetAxisConfiguration(PausedPlayer, "RB");

            AxisConfiguration rocket = InputManager.GetAxisConfiguration(PausedPlayer, "Rocket");
            AxisConfiguration laser = InputManager.GetAxisConfiguration(PausedPlayer, "Laser");
            AxisConfiguration shootBall = InputManager.GetAxisConfiguration(PausedPlayer, "ShootBall");
           
            AxisConfiguration cancelButton = InputManager.GetAxisConfiguration(PausedPlayer, "Pause");
            AxisConfiguration submitButton = InputManager.GetAxisConfiguration(PausedPlayer, "Start");

            throttling.ClearCompletely();
            turning.ClearCompletely();
            strafing.ClearCompletely();
            jump.ClearCompletely();
            LB.ClearCompletely();       //Clearing literally everything. Ok, actually leaving DEVICE not cleared
            RB.ClearCompletely();
            rocket.ClearCompletely();   //COMM further 
            laser.ClearCompletely();
            shootBall.ClearCompletely();
            cancelButton.ClearCompletely();
            submitButton.ClearCompletely();
            
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
        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(PausedPlayer, "DEVICE");
        
        AxisConfiguration throttling = InputManager.GetAxisConfiguration(PausedPlayer, "Throttle");
        AxisConfiguration turning = InputManager.GetAxisConfiguration(PausedPlayer, "Turning");
        AxisConfiguration strafing = InputManager.GetAxisConfiguration(PausedPlayer, "Strafing");

        AxisConfiguration jump = InputManager.GetAxisConfiguration(PausedPlayer, "Jump");
        AxisConfiguration LB = InputManager.GetAxisConfiguration(PausedPlayer, "LB");
        AxisConfiguration RB = InputManager.GetAxisConfiguration(PausedPlayer, "RB");

        AxisConfiguration rocket = InputManager.GetAxisConfiguration(PausedPlayer, "Rocket");
        AxisConfiguration laser = InputManager.GetAxisConfiguration(PausedPlayer, "Laser");
        AxisConfiguration shootBall = InputManager.GetAxisConfiguration(PausedPlayer, "ShootBall");

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
        AxisConfiguration playerDevice = InputManager.GetAxisConfiguration(PausedPlayer, "DEVICE");

        AxisConfiguration throttling = InputManager.GetAxisConfiguration(PausedPlayer, "Throttle");
        AxisConfiguration turning = InputManager.GetAxisConfiguration(PausedPlayer, "Turning");
        AxisConfiguration strafing = InputManager.GetAxisConfiguration(PausedPlayer, "Strafing");

        AxisConfiguration jump = InputManager.GetAxisConfiguration(PausedPlayer, "Jump");
        AxisConfiguration LB = InputManager.GetAxisConfiguration(PausedPlayer, "LB");
        AxisConfiguration RB = InputManager.GetAxisConfiguration(PausedPlayer, "RB");

        AxisConfiguration rocket = InputManager.GetAxisConfiguration(PausedPlayer, "Rocket");
        AxisConfiguration laser = InputManager.GetAxisConfiguration(PausedPlayer, "Laser");
        AxisConfiguration shootBall = InputManager.GetAxisConfiguration(PausedPlayer, "ShootBall");

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
            laser.positive = KeyCode.KeypadEnter;    
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

    [SerializeField] private GameObject errorPanel;
    [SerializeField] private Text errorMessage;
    [SerializeField] private Button okButton;

    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject previousPanel;   
    [SerializeField] private Button backInSettings;

    private GameObject previousButton;

    public void SetPrevoiusButton(GameObject button)
    {
        previousButton = button;    //TODO IN game
    }

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
            settingsMenu.SetActive(true);
            previousPanel.SetActive(true);
            CustomInputModule.Instance.PlaySelect();
            EventSystem.current.SetSelectedGameObject(previousButton);
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
        if (GameController.Controller != null)
        {
            if (PausedPlayer == PlayerID.One)
            {
                GameController.Controller.PlayerOne.GetComponent<PlayerMovement>().analogTurning = turning;
            }
            else if (PausedPlayer == PlayerID.Two)
            {
                GameController.Controller.PlayerTwo.GetComponent<PlayerMovement>().analogTurning = turning;
            }
        }
    }

    private void setJumpSingleButton(bool state)
    {
        if (GameController.Controller != null)
        {
            if (PausedPlayer == PlayerID.One)
            {
                GameController.Controller.PlayerOne.GetComponent<PlayerMovement>().jumpSingleButton = state;
            }
            else if (PausedPlayer == PlayerID.Two)
            {
                GameController.Controller.PlayerTwo.GetComponent<PlayerMovement>().jumpSingleButton = state;
            }
        }       
    }

    private void setPlayerDevice(int num)
    {
        if (PausedPlayer == PlayerID.One)
        {
            CustomInputModule.Instance.PlayerOneDevice = num;
        }
        else if (PausedPlayer == PlayerID.Two)
        {
            CustomInputModule.Instance.PlayerTwoDevice = num;
        }


    }




}
