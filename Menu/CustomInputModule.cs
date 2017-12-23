#region [License]
//	The MIT License (MIT)
//	
//	Copyright (c) 2015, Unity Technologies
//	Copyright (c) 2015, Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//	copies of the Software, and to permit persons to whom the Software is
//	furnished to do so, subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in
//	all copies or substantial portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//	THE SOFTWARE.
//
//	https://bitbucket.org/Unity-Technologies/ui
#endregion
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
 
//This is custom edited by me StandaloneInputModule that we use in menus to accept all default hardcoded input for navigation. Like "Submit" buttons are Enter, Space, and a start button on all joysticks. 
//Directional keys are WASD, arrows on keyboard and left arrow pad and left stick on joysticks
//It is made to have default navigation in menus, regardless of what players set up in key bindings.

namespace TeamUtility.IO
{	
	public class CustomInputModule : PointerInputModule
	{
		public const string VERSION = "5.5";
               
	    public bool Menu = true;            //This flag defines if general input is active (in menu), or player-specific one (in game)
        [Header("Menu Only")]
	    public bool Enabled = true;                 //Flag to disable input (made it only for any button input that I was actually altering in the default input system, mouse inputs are blocked by a panel over everything)

        [Header("Gameplay Only")]
	    public bool PlayerOne;
	    public bool PlayerTwo;  //Flags that define which player can control the pause menu (two flags, cuz there can be a case of both of them controlling)

	    public int PlayerOneDevice;
	    public int PlayerTwoDevice;

        private float m_PrevActionTime;
		private Vector2 m_LastMoveVector;
		private int m_ConsecutiveMoveCount = 0;

		private Vector2 m_LastMousePosition;
		private Vector2 m_MousePosition;

		protected CustomInputModule()
		{
		}

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public enum InputMode
		{
			Mouse,
			Buttons
		}

		[Obsolete("Mode is no longer needed on input module as it handles both mouse and keyboard simultaneously.", false)]
		public InputMode inputMode
		{
			get { return InputMode.Mouse; }
		}

	    [SerializeField] private string m_HorizontalAxis = "Turning";

	    /// <summary>
	    /// Name of the vertical axis for movement (if axis events are used).
	    /// </summary>
	    [SerializeField] private string m_VerticalAxis = "Throttle";

	    /// <summary>
	    /// Name of the submit button.
	    /// </summary>
	    [SerializeField] private string m_SubmitButton = "Start";

	    /// <summary>
	    /// Name of the cancel button.
	    /// </summary>
	    [SerializeField] private string m_CancelButton = "Pause";

	    [Header("General")]
	    private AudioSource click;  //Click sound whenever user presses any vertical buttons (in allowed time-intervals), basically whenever vertical move actually executes. Used to PlayOneShot select sound through it as well

	    [SerializeField] private AudioClip select;  //There it is!

        [SerializeField] private float m_InputActionsPerSecond = 10;

		[SerializeField]
		private float m_RepeatDelay = 0.5f;

		[SerializeField] [FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
		private bool m_ForceModuleActive;

		[Obsolete("allowActivationOnMobileDevice has been deprecated. Use forceModuleActive instead (UnityUpgradable) -> forceModuleActive")]
		public bool allowActivationOnMobileDevice
		{
			get { return m_ForceModuleActive; }
			set { m_ForceModuleActive = value; }
		}

		public bool forceModuleActive
		{
			get { return m_ForceModuleActive; }
			set { m_ForceModuleActive = value; }
		}

		public float inputActionsPerSecond
		{
			get { return m_InputActionsPerSecond; }
			set { m_InputActionsPerSecond = value; }
		}

		public float repeatDelay
		{
			get { return m_RepeatDelay; }
			set { m_RepeatDelay = value; }
		}

	    /// <summary>
	    /// Name of the horizontal axis for movement (if axis events are used).
	    /// </summary>
	    public string horizontalAxis
	    {
	        get { return m_HorizontalAxis; }
	        set { m_HorizontalAxis = value; }
	    }

	    /// <summary>
	    /// Name of the vertical axis for movement (if axis events are used).
	    /// </summary>
	    public string verticalAxis
	    {
	        get { return m_VerticalAxis; }
	        set { m_VerticalAxis = value; }
	    }

	    public string submitButton
	    {
	        get { return m_SubmitButton; }
	        set { m_SubmitButton = value; }
	    }

	    public string cancelButton
	    {
	        get { return m_CancelButton; }
	        set { m_CancelButton = value; }
	    }

        public override void UpdateModule()
		{
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = InputManager.mousePosition;
		}

		public override bool IsModuleSupported()
		{
			return m_ForceModuleActive || InputManager.mousePresent || InputManager.touchSupported;
		}

	    public static int joyNum;   //Static variable to hold the number of conneceted joysticks (we also need it in MenuSelectors)

	    public static readonly string[,] joyAxisNamesHor = new string[8, 2] //Array of allowed horizontal joystick axis names (for joysticks 1-4, because no way someone will have more than that connected)
	    {                                                                   //Static because we also need it in MenuSelectors
	        {"joy_0_axis_0", "joy_0_axis_5"},
	        {"joy_1_axis_0", "joy_1_axis_5"},
	        {"joy_2_axis_0", "joy_2_axis_5"},
	        {"joy_3_axis_0", "joy_3_axis_5"},
	        {"joy_4_axis_0", "joy_4_axis_5"},
	        {"joy_5_axis_0", "joy_5_axis_5"},
	        {"joy_6_axis_0", "joy_6_axis_5"},
	        {"joy_7_axis_0", "joy_7_axis_5"}
        };

        private readonly string[,] joyAxisNames = new string[8, 2]  //Vertical ones, only need them here
	    {
	        {"joy_0_axis_1", "joy_0_axis_6"}, 
            {"joy_1_axis_1", "joy_1_axis_6"}, 
            {"joy_2_axis_1", "joy_2_axis_6"},
	        {"joy_3_axis_1", "joy_3_axis_6"},
	        {"joy_4_axis_1", "joy_4_axis_6"},
	        {"joy_5_axis_1", "joy_5_axis_6"},
	        {"joy_6_axis_1", "joy_6_axis_6"},
	        {"joy_7_axis_1", "joy_7_axis_6"}
        };

	    public static CustomInputModule Instance;   //Static singleton and stuff

	    protected override void OnEnable()  //It was giving errors in Awake, but OnEnable worked, so that's how it gets done, I guess
	    {
	        base.OnEnable();    //Idk, if base class has whatever stuff in Awake (it has)
	        if (Instance != null)
	        {
	            Destroy(gameObject);    //Regular singleton stuff. Making sure if we go back to main menu, there won't be two EventSystems
            }
	        else
	        {
	            DontDestroyOnLoad(gameObject);  
	            Instance = this;
	        }
        }

        protected override void Awake()
        {
            click = GetComponent<AudioSource>();        //Get audioSource attached to the same object as EventSystem and InputModule

	        joyNum = Input.GetJoystickNames().Length;   //Get the number of joysticks when the event system awakes at the scene load (that means after connecting a joystick, for it to work in the menu, you have to move to some other menu)
	        if (joyNum > 8) joyNum = 8;                 //So shit doesn't break if there IS ACTUALLY SOME INSANE PERSON HAVING 9+ JOYSTICKS CONNECTED
	    }

	    protected override void Start()             //Setting device for each player to be able to process Joystick's both left stick and D-Pad in the menu for player-specific controls
	    {
	        AxisConfiguration playerOneDevice = InputManager.GetAxisConfiguration(PlayerID.One, "DEVICE");          //Getting axisConfig of devices
	        AxisConfiguration playerTwoDevice = InputManager.GetAxisConfiguration(PlayerID.Two, "DEVICE"); 

	        if (playerOneDevice.description == "Keyboard")  //Setting the number
	        {
	            PlayerOneDevice = 0;
	        }
            else if (playerOneDevice.description == "Keyboard+Mouse")
	        {
	            PlayerOneDevice = 1;
	        }
	        else
	        {
	            try     //If the user wrote some bs in the config file, don't get the exception, but instead set default device (other stuff regarding this is handled in other scripts)
	            {
	                PlayerOneDevice = Int32.Parse(playerOneDevice.description.Substring(playerOneDevice.description.Length - 1, 1)) + 2;    //Getting last character with joystick number and +2, because 0 and 1 are keyboard-mouse
                }
	            catch (Exception)
	            {
	                PlayerOneDevice = 0;
                }

            }

	        if (playerTwoDevice.description == "Keyboard")  //Same for player two
	        {
                PlayerTwoDevice = 0;
	        }
	        else if (playerTwoDevice.description == "Keyboard+Mouse")
	        {
                PlayerTwoDevice = 1;
	        }
	        else
	        {
	            try     
	            {
	                PlayerTwoDevice = Int32.Parse(playerTwoDevice.description.Substring(playerTwoDevice.description.Length - 1, 1)) + 2;
                }
	            catch (Exception)
	            {
	                PlayerTwoDevice = 0;
                }
                
	        }

        }

	    public void PlayClick()     //Used in MenuSelectors on every Left-Right button
	    {
	        click.Play();
	    }

	    public void PlaySelect()    //Used everywhere when pressing some button
	    {
	        click.PlayOneShot(select);
	    }

	    public const float dead = 0.6f;         //Dead zone of joystick sticks when to accept menu input

        public override bool ShouldActivateModule()
        {           
			if(!base.ShouldActivateModule())
				return false;

			var shouldActivate = m_ForceModuleActive;

            if (Menu)
            {
                if (Enabled == false) return false; //This is put everywhere where I brought my hands to in this script

                //SUBMIT
                shouldActivate |= InputManager.GetKeyDown(KeyCode.Return);     
		        shouldActivate |= InputManager.GetKeyDown(KeyCode.Space);               //I am not exactly sure what "shouldActivate" is for, but we put here all default hardcoded buttons that you can control menu with
                shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick1Button7);    //Also not sure why default Unity script didn't have it written like "if (someButtonWasPressed) return true;" for all buttons. Much more efficient, IMO. We don't bottleneck from this implementation, so 'who cares'
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick2Button7);  
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick3Button7);
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick4Button7);

		        ////CANCEL
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Return);
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Space);
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick1Button7);
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick2Button7);
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick3Button7);
		        //shouldActivate |= InputManager.GetKeyDown(KeyCode.Joystick4Button7);

                //VERTICAL AXIS		   
		        shouldActivate |= Mathf.Abs(Input.GetAxisRaw(joyAxisNames[0, 0])) > dead; //Joystick1 Y Axis
		        shouldActivate |= Mathf.Abs(Input.GetAxisRaw(joyAxisNames[0, 1])) > dead; //Joystick1 7 Axis
		        shouldActivate |= InputManager.GetKeyDown(KeyCode.W);                //This all initially had only "InputManager" class here, but it has all "GetAxis" tied to player number, so we are using default Input class when we need Axis
		        shouldActivate |= InputManager.GetKeyDown(KeyCode.S);
		        shouldActivate |= InputManager.GetKeyDown(KeyCode.UpArrow);
		        shouldActivate |= InputManager.GetKeyDown(KeyCode.DownArrow);

                if (joyNum > 1) //If there is more than 1 joystick connected, evaluate buttons for all additional ones
		        {
		            for (int i = 1; i < joyNum; i++)
		            {
		                shouldActivate |= Mathf.Abs(Input.GetAxisRaw(joyAxisNames[i, 0])) > dead; //Joystick[i] Y Axis
		                shouldActivate |= Mathf.Abs(Input.GetAxisRaw(joyAxisNames[i, 1])) > dead; //Joystick[i] 7 Axis
		                shouldActivate |= InputManager.GetKeyDown((KeyCode)(357 + i * 20)); //Submit button numbers in the KeyCode enum
                    }
                }
            
                //MAYBE HORIZONTAL IF NEEDED
            

            }
            else
            {
                if (PlayerOne)  //If player one controls, activate whatever with this player's buttons
                {
                    shouldActivate |= InputManager.GetButtonDown(m_SubmitButton, PlayerID.One);     
                    shouldActivate |= InputManager.GetButtonDown(m_CancelButton, PlayerID.One);       
                    //shouldActivate |= !Mathf.Approximately(InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.One), 0.0f);
                    shouldActivate |= Mathf.Abs(InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.One)) > dead;       
                }
                if (PlayerTwo)  //Changed
                {
                    shouldActivate |= InputManager.GetButtonDown(m_SubmitButton, PlayerID.Two);             
                    shouldActivate |= InputManager.GetButtonDown(m_CancelButton, PlayerID.Two);              
                    //shouldActivate |= !Mathf.Approximately(InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.Two), 0.0f);           
                    shouldActivate |= Mathf.Abs(InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.Two)) > dead;
                }

                shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
                shouldActivate |= InputManager.GetMouseButtonDown(0);
            }

            
            shouldActivate |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0.0f;
			shouldActivate |= InputManager.GetMouseButtonDown(0);

			if(InputManager.touchCount > 0)
				shouldActivate = true;

			return shouldActivate;
		}

		public override void ActivateModule()
		{
			base.ActivateModule();
			m_MousePosition = InputManager.mousePosition;
			m_LastMousePosition = InputManager.mousePosition;

			var toSelect = eventSystem.currentSelectedGameObject;
			if(toSelect == null)
				toSelect = eventSystem.firstSelectedGameObject;

			eventSystem.SetSelectedGameObject(toSelect, GetBaseEventData());
		}

		public override void DeactivateModule()
		{
			base.DeactivateModule();
			ClearSelection();
		}

		public override void Process()
		{
			bool usedEvent = SendUpdateEventToSelectedObject();

			if(eventSystem.sendNavigationEvents)
			{
				if(!usedEvent)
					usedEvent |= SendMoveEventToSelectedObject();

				if(!usedEvent)
					SendSubmitEventToSelectedObject();
			}

			// touch needs to take precedence because of the mouse emulation layer
			if(!ProcessTouchEvents() && InputManager.mousePresent)
				ProcessMouseEvent();
		}

		private bool ProcessTouchEvents()
		{
			for(int i = 0; i < InputManager.touchCount; ++i)
			{
				Touch touch = InputManager.GetTouch(i);

				if(touch.type == TouchType.Indirect)
					continue;

				bool released;
				bool pressed;
				var pointer = GetTouchPointerEventData(touch, out pressed, out released);

				ProcessTouchPress(pointer, pressed, released);

				if(!released)
				{
					ProcessMove(pointer);
					ProcessDrag(pointer);
				}
				else
					RemovePointerData(pointer);
			}
			return InputManager.touchCount > 0;
		}

		protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
		{
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

			// PointerDown notification
			if(pressed)
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

				//DeselectIfSelectionChanged(currentOverGo, pointerEvent);

				if(pointerEvent.pointerEnter != currentOverGo)
				{
					// send a pointer enter to the touched element if it isn't the one to select...
					HandlePointerExitAndEnter(pointerEvent, currentOverGo);
					pointerEvent.pointerEnter = currentOverGo;
				}

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

				// didnt find a press handler... search for a click handler
				if(newPressed == null)
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// Debug.Log("Pressed: " + newPressed);

				float time = Time.unscaledTime;

				if(newPressed == pointerEvent.lastPress)
				{
					var diffTime = time - pointerEvent.clickTime;
					if(diffTime < 0.3f)
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;

					pointerEvent.clickTime = time;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}

				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;

				pointerEvent.clickTime = time;

				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

				if(pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}

			// PointerUp notification
			if(released)
			{
				// Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

				// Debug.Log("KeyCode: " + pointer.eventData.keyCode);

				// see if we mouse up on the same element that we clicked on...
				var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// PointerClick and Drop events
				if(pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if(pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
				}

				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;

				if(pointerEvent.pointerDrag != null && pointerEvent.dragging)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;

				if(pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

				pointerEvent.pointerDrag = null;

				// send exit events as we need to simulate this on touch up on touch device
				ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
				pointerEvent.pointerEnter = null;
			}
		}


		/// <summary>
		/// Process submit keys.
		/// </summary>
		protected bool SendSubmitEventToSelectedObject()
		{		    
            if (eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData();

		    if (Menu)
		    {
		        if (Enabled == false) return false;

                bool pressed = false;

		        pressed |= InputManager.GetKeyDown(KeyCode.Return); //Evaluating all Submit buttons
		        pressed |= InputManager.GetKeyDown(KeyCode.Space);
		        pressed |= InputManager.GetKeyDown(KeyCode.Joystick1Button7);
		        if (joyNum > 1)
		        {
		            for (int i = 1; i < joyNum; i++)
		            {
		                pressed |= InputManager.GetKeyDown((KeyCode)(357 + i * 20));    //And additional joysticks if there are any
		            }
		        }

		        if (pressed)    //Changed it a bit from original implementation
		            ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
            }
		    else
		    {
		        if (PlayerOne)
		        {
		            if (InputManager.GetButtonDown(m_SubmitButton, PlayerID.One)) //Doing similar stuff for player-specific controlling
		            {
		                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
                    }

		            if (InputManager.GetButtonDown(m_CancelButton, PlayerID.One)) 
		            {
		                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
                    }
		                
                }
		        if (PlayerTwo)
		        {
		            if (InputManager.GetButtonDown(m_SubmitButton, PlayerID.Two)) 
		            {
		                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);
                    }

		            if (InputManager.GetButtonDown(m_CancelButton, PlayerID.Two)) 
		            {
		                ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);
                    }
		                
                }
		       
            }
		             
			return data.used;
		}

	    private const string Joystick = "Joystick";
        
        private Vector2 GetRawMoveVector()  //If for whatever reason players press directional buttons together, we add all of them and return "-1,0,1" value out of all their input. Otherwise, just whatever button got pressed will output "-1,1"
		{
		    if (Menu)
		    {
		        if (Enabled == false) return Vector2.zero;

		        Vector2 vector = Vector2.zero;

		        if (InputManager.GetKey(KeyCode.W)) vector.y += 1;
		        if (InputManager.GetKey(KeyCode.S)) vector.y += -1;
		        if (InputManager.GetKey(KeyCode.UpArrow)) vector.y += 1;
		        if (InputManager.GetKey(KeyCode.DownArrow)) vector.y += -1;

		        if (Input.GetAxisRaw(joyAxisNames[0, 0]) > dead) vector.y += -1;
		        if (Input.GetAxisRaw(joyAxisNames[0, 0]) < -dead) vector.y += 1;
		        if (Input.GetAxisRaw(joyAxisNames[0, 1]) > dead) vector.y += 1;     //Considering Sticks controlling with Dead Zone
		        if (Input.GetAxisRaw(joyAxisNames[0, 1]) < -dead) vector.y += -1;

		        if (joyNum > 1)
		        {
		            for (int i = 1; i < joyNum; i++)
		            {
		                if (Input.GetAxisRaw(joyAxisNames[i, 0]) > dead) vector.y += -1;
		                if (Input.GetAxisRaw(joyAxisNames[i, 0]) < -dead) vector.y += 1;
		                if (Input.GetAxisRaw(joyAxisNames[i, 1]) > dead) vector.y += 1;
		                if (Input.GetAxisRaw(joyAxisNames[i, 1]) < -dead) vector.y += -1;
		            }
		        }

		        return vector.normalized;      //If players pressed 10000 "Up" buttons, we convert it to a value of "1"     
            }
		    else
		    {
		        Vector2 P1Vector = Vector2.zero;
		        Vector2 P2Vector = Vector2.zero;
                
                if (PlayerOne)
		        {
		            

                    if (PlayerOneDevice == 0 || PlayerOneDevice == 1)
		            {                        
		                P1Vector.y = InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.One);   //InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.One)
                    }
		            else
		            {		                
                        if (InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.One) > dead) P1Vector.y += 1;
		                if (InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.One) < -dead) P1Vector.y += -1;

                        if (Input.GetAxisRaw(joyAxisNames[PlayerOneDevice - 2, 0]) > dead) P1Vector.y += -1;
		                if (Input.GetAxisRaw(joyAxisNames[PlayerOneDevice - 2, 0]) < -dead) P1Vector.y += 1;
		                if (Input.GetAxisRaw(joyAxisNames[PlayerOneDevice - 2, 1]) > dead) P1Vector.y += 1;
		                if (Input.GetAxisRaw(joyAxisNames[PlayerOneDevice - 2, 1]) < -dead) P1Vector.y += -1;
                    }
                    
		        }
		        if (PlayerTwo)
		        {
		            if (PlayerTwoDevice == 0 || PlayerTwoDevice == 1)
		            {
		                P2Vector.y = InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.Two);   //InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.One)
		            }
		            else
		            {
		                if (InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.Two) > dead) P2Vector.y += 1;
		                if (InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.Two) < -dead) P2Vector.y += -1;

		                if (Input.GetAxisRaw(joyAxisNames[PlayerTwoDevice - 2, 0]) > dead) P2Vector.y += -1;
		                if (Input.GetAxisRaw(joyAxisNames[PlayerTwoDevice - 2, 0]) < -dead) P2Vector.y += 1;
		                if (Input.GetAxisRaw(joyAxisNames[PlayerTwoDevice - 2, 1]) > dead) P2Vector.y += 1;
		                if (Input.GetAxisRaw(joyAxisNames[PlayerTwoDevice - 2, 1]) < -dead) P2Vector.y += -1;
		            }
                }

		        return (P1Vector + P2Vector).normalized;    //We don't really need X axis here, I just left as it was in original code
            }
		    
        }
        
	    
        /// <summary>
        /// Process keyboard events.
        /// </summary>
        protected bool SendMoveEventToSelectedObject()
		{		    
            float time = Time.unscaledTime;

			Vector2 movement = GetRawMoveVector();
            
			if(Mathf.Approximately(movement.x, 0f) && Mathf.Approximately(movement.y, 0f))
			{
				m_ConsecutiveMoveCount = 0;
				return false;
			}

            // If user pressed key again, always allow event
            bool allow = false;

		    if (Menu)
		    {
		        if (Enabled == false) return false;

		        allow |= Input.GetButtonDown(joyAxisNames[0, 0]); //Joystick1 Y Axis
		        allow |= Input.GetButtonDown(joyAxisNames[0, 1]); //Joystick1 7 Axis
		        allow |= InputManager.GetKeyDown(KeyCode.W);
		        allow |= InputManager.GetKeyDown(KeyCode.S);                            //Evaluating all vertical buttons
		        allow |= InputManager.GetKeyDown(KeyCode.UpArrow);
		        allow |= InputManager.GetKeyDown(KeyCode.DownArrow);

		        if (joyNum > 1)
		        {
		            for (int i = 1; i < joyNum; i++)
		            {
		                allow |= Input.GetButtonDown(joyAxisNames[i, 0]); //Joystick[i] Y Axis
		                allow |= Input.GetButtonDown(joyAxisNames[i, 1]); //Joystick[i] 7 Axis		            
		            }
		        }
            }
		    else
		    {
		        if (PlayerOne)      //Changed
		        {
		            //allow |= InputManager.GetButtonDown(m_HorizontalAxis, PlayerID.One);      //Changed
		            allow |= InputManager.GetButtonDown(m_VerticalAxis, PlayerID.One);      //Changed

		            if (PlayerOneDevice > 1)
		            {
		                allow |= Input.GetButtonDown(joyAxisNames[PlayerOneDevice - 2, 0]);		                
		                allow |= Input.GetButtonDown(joyAxisNames[PlayerOneDevice - 2, 1]);	               
                    }
                    
                }
		        if (PlayerTwo)      //Changed
		        {
		            //allow |= InputManager.GetButtonDown(m_HorizontalAxis, PlayerID.Two);      //Changed
		            allow |= InputManager.GetButtonDown(m_VerticalAxis, PlayerID.Two);      //Changed

		            if (PlayerTwoDevice > 1)
		            {
		                allow |= Input.GetButtonDown(joyAxisNames[PlayerTwoDevice - 2, 0]);
		                allow |= Input.GetButtonDown(joyAxisNames[PlayerTwoDevice - 2, 1]);
		            }
                }
            }
		   




            bool similarDir = (Vector2.Dot(movement, m_LastMoveVector) > 0);
			if(!allow)
			{
				// Otherwise, user held down key or axis.
				// If direction didn't change at least 90 degrees, wait for delay before allowing consequtive event.
				if(similarDir && m_ConsecutiveMoveCount == 1)
					allow = (time > m_PrevActionTime + m_RepeatDelay);
				// If direction changed at least 90 degree, or we already had the delay, repeat at repeat rate.
				else
					allow = (time > m_PrevActionTime + 1f / m_InputActionsPerSecond);
			}

		    if (allow) click.Play();    //Play click sound when pressed some Up/Down button
            
            if (!allow)
				return false;

			// Debug.Log(m_ProcessingEvent.rawType + " axis:" + m_AllowAxisEvents + " value:" + "(" + x + "," + y + ")");
			var axisEventData = GetAxisEventData(movement.x, movement.y, 0.6f);

			if(axisEventData.moveDir != MoveDirection.None)
			{
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, axisEventData, ExecuteEvents.moveHandler);			    
                if (!similarDir)
					m_ConsecutiveMoveCount = 0;
				m_ConsecutiveMoveCount++;
				m_PrevActionTime = time;
				m_LastMoveVector = movement;
			}
			else
			{
				m_ConsecutiveMoveCount = 0;
			}

			return axisEventData.used;
		}

		protected void ProcessMouseEvent()
		{
			ProcessMouseEvent(0);
		}

		protected virtual bool ForceAutoSelect()
		{
			return false;
		}

		/// <summary>
		/// Process all mouse events.
		/// </summary>
		protected void ProcessMouseEvent(int id)
		{
			var mouseData = GetMousePointerEventData(id);
			var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

			if(ForceAutoSelect())
				eventSystem.SetSelectedGameObject(leftButtonData.buttonData.pointerCurrentRaycast.gameObject, leftButtonData.buttonData);

			// Process the first mouse button fully
			ProcessMousePress(leftButtonData);
			ProcessMove(leftButtonData.buttonData);
			ProcessDrag(leftButtonData.buttonData);

			// Now process right / middle clicks
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

			if(!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
			{
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		protected bool SendUpdateEventToSelectedObject()
		{
			if(eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData();
			ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.updateSelectedHandler);
			return data.used;
		}

		/// <summary>
		/// Process the current mouse press.
		/// </summary>
		protected void ProcessMousePress(MouseButtonEventData data)
		{
			var pointerEvent = data.buttonData;
			var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

			// PointerDown notification
			if(data.PressedThisFrame())
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

				//DeselectIfSelectionChanged(currentOverGo, pointerEvent);

				// search for the control that will receive the press
				// if we can't find a press handler set the press
				// handler to be what would receive a click.
				var newPressed = ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

				// didnt find a press handler... search for a click handler
				if(newPressed == null)
					newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// Debug.Log("Pressed: " + newPressed);

				float time = Time.unscaledTime;

				if(newPressed == pointerEvent.lastPress)
				{
					var diffTime = time - pointerEvent.clickTime;
					if(diffTime < 0.3f)
						++pointerEvent.clickCount;
					else
						pointerEvent.clickCount = 1;

					pointerEvent.clickTime = time;
				}
				else
				{
					pointerEvent.clickCount = 1;
				}

				pointerEvent.pointerPress = newPressed;
				pointerEvent.rawPointerPress = currentOverGo;

				pointerEvent.clickTime = time;

				// Save the drag handler as well
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

				if(pointerEvent.pointerDrag != null)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
			}

			// PointerUp notification
			if(data.ReleasedThisFrame())
			{
				// Debug.Log("Executing pressup on: " + pointer.pointerPress);
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

				// Debug.Log("KeyCode: " + pointer.eventData.keyCode);

				// see if we mouse up on the same element that we clicked on...
				var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

				// PointerClick and Drop events
				if(pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				else if(pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
				}

				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;

				if(pointerEvent.pointerDrag != null && pointerEvent.dragging)
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;

				// redo pointer enter / exit to refresh state
				// so that if we moused over somethign that ignored it before
				// due to having pressed on something else
				// it now gets it.
				if(currentOverGo != pointerEvent.pointerEnter)
				{
					HandlePointerExitAndEnter(pointerEvent, null);
					HandlePointerExitAndEnter(pointerEvent, currentOverGo);
				}
			}
		}


	}
}
