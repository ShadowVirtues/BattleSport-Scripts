using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	[RequireComponent(typeof(Image))]
	public class RebindInput : MonoBehaviour, IPointerDownHandler, ISubmitHandler, IPointerEnterHandler, IDeselectHandler, ISelectHandler
    {
		public enum RebindType
		{
			Keyboard, GamepadButton, GamepadAxis
		}
        
		[SerializeField]
		[FormerlySerializedAs("m_keyDescription")]
		private Text _keyDescription;


        [SerializeField] private string device; //MINE

		[SerializeField]
		[FormerlySerializedAs("m_axisConfigName")]
		private string _axisConfigName;

		[SerializeField]
		[FormerlySerializedAs("m_cancelButton")]
		private string _cancelButton;

		[SerializeField]
		[FormerlySerializedAs("m_timeout")]
		private float _timeout;

		[SerializeField]
		[FormerlySerializedAs("m_changePositiveKey")]
		private bool _changePositiveKey;
        
		[SerializeField]
		[FormerlySerializedAs("m_allowAnalogButton")]
		private bool _allowAnalogButton;

		[SerializeField]
		[FormerlySerializedAs("m_joystick")]
		[Range(0, AxisConfiguration.MaxJoysticks)]
		private int _joystick = 0;

		[SerializeField]
		[FormerlySerializedAs("m_rebindType")]
		private RebindType _rebindType;
		
		private AxisConfiguration _axisConfig;
		
		private static string[] _axisNames = new string[] { "X", "Y", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };

        //===================MY SHIT====================

        [Header("Controls")]
        [SerializeField] private Text OptionName;     //Name of the option like "Arena", "PlayerOne"
        private Text OptionValue;    //The value we change, we use it in derived classes as well, to set default values on menu load for it

        public void OnPointerEnter(PointerEventData eventData)  //This is so when we hover over the selectable, it gets focused and selected
        {
            if (EventSystem.current != null)
                if (!EventSystem.current.alreadySelecting)
                    EventSystem.current.SetSelectedGameObject(gameObject);  //Set this selectable as current selected item (EventSystem automatically deselects previously selected selectable)
        }

        [Header("Colors")]
        [SerializeField] private Color optionSelected = new Color(146 / 256f, 16 / 256f, 16 / 256f);
        [SerializeField] private Color optionDeselected = Color.black;  //Menu selector colors. Colors for option and value text when they are and are not selected
        [SerializeField] private Color valueSelected = new Color(99 / 256f, 0, 0);
        [SerializeField] private Color valueDeselected = Color.black;
        [SerializeField] private Color binding = Color.red;

        public void OnSelect(BaseEventData eventData)           //When this selectable gets selected
        {
            OptionName.color = optionSelected;         //Set OptionName/Value colors to specified color when it's selected
            OptionValue.color = valueSelected;        
        }

        public void OnDeselect(BaseEventData eventData)     //When the selectable gets deselected
        {
            OptionName.color = optionDeselected;
            OptionValue.color = valueDeselected;     //Set back all the colors
        }

        public static PlayerID PausedPlayer;    //public static, so we don't have to set it for every single instance of this. Gets set in KeyBindingsMenu.cs

        void OnEnable()
        {
            InitializeAxisConfig();         //Reinitialize axis config, which on surface is read all buttons from set settings and put them into text fields for bound buttons
        }
        
        private EventSystem eventSystem;    //Reference to event system to disable-enable is when binding a button
        
	    public void OnSubmit(BaseEventData data)
	    {
	        StartCoroutine(StartInputScanDelayed());    //When pressed submit on key binding selector, start the scan
	    }
        
	    private void StartBinding()         //Function to call when we start binding to process all the stuff needed for me (like, additional stuff, that is not in the initial script)
	    {
	        eventSystem.enabled = false;    //Disable event system so with any input we only bind a button
	        OptionName.color = binding;     //Set colors to red so we see game is awaiting for the input
	        OptionValue.color = binding;
        }

	    private IEnumerator EndBinding()           //When ending binding. Coroutine, because this runs before Update processing Back menu key. So when we press "Cancel", we cancel binding, and not go into previous menu
	    {
	        yield return null;
	        eventSystem.enabled = true;     //Enable event system back
            eventSystem.SetSelectedGameObject(gameObject);  //Selected object gets nullified when disabling event system, so set it back, and while selecting, the buttons will get back to normal
	    }
        
        //===============================================

        private void Awake()
        {
            OptionValue = _keyDescription;      //Copying a reference, so I can use the names I am used to
			eventSystem = EventSystem.current;  //Get a reference to event system
            
			//	The axis config needs to be reinitialized because loading can invalidate
			//	the input configurations
			InputManager.Instance.Loaded += InitializeAxisConfig;
			//InputManager.Instance.ConfigurationDirty += HandleConfigurationDirty;
		}
		
		private void OnDestroy()
		{
			if(InputManager.Instance != null)
			{
				InputManager.Instance.Loaded -= InitializeAxisConfig;
				//InputManager.Instance.ConfigurationDirty -= HandleConfigurationDirty;
			}
		}
		
		private void InitializeAxisConfig()
		{
            _axisConfig = InputManager.GetAxisConfiguration(PausedPlayer, _axisConfigName);

		    AxisConfiguration deviceAxis = InputManager.GetAxisConfiguration(PausedPlayer, "DEVICE");
		    bool noBind = deviceAxis.description != device;

            if (_axisConfig != null)
			{
				if(_rebindType == RebindType.Keyboard || _rebindType == RebindType.GamepadButton)
				{
					if(_changePositiveKey)
					{						
						_keyDescription.text = _axisConfig.positive == KeyCode.None || noBind ? "?" : _axisConfig.positive.ToString();
					}
					else
					{						
						_keyDescription.text = _axisConfig.negative == KeyCode.None || noBind ? "?" : _axisConfig.negative.ToString();
					}
				}
				else
				{
					_keyDescription.text = _axisNames[_axisConfig.axis];
				}
			}
			else
			{
				_keyDescription.text = "";
				Debug.LogError($@"Input configuration for '{PausedPlayer}' does not exist or axis '{_axisConfigName}' does not exist");
			}
		    _keyDescription.color = valueDeselected;
		}

		//private void HandleConfigurationDirty(string configName)
		//{
		//	if(configName == _inputConfigName)
		//		InitializeAxisConfig();
		//}

		public void OnPointerDown(PointerEventData data)
		{
			StartCoroutine(StartInputScanDelayed());
		}

        private IEnumerator StartInputScanDelayed()
		{
			yield return null;

			if(!InputManager.IsScanning && _axisConfig != null)
			{
			    StartBinding();

				_keyDescription.text = "...";
				
				ScanSettings settings;
				settings.joystick = _joystick;
				settings.cancelScanButton = _cancelButton;
				settings.timeout = _timeout;
				settings.userData = null;
				if(_rebindType == RebindType.GamepadAxis)
				{
					settings.scanFlags = ScanFlags.JoystickAxis;
					InputManager.StartScan(settings, HandleJoystickAxisScan);
				}
				else if(_rebindType == RebindType.GamepadButton)
				{
					settings.scanFlags = ScanFlags.JoystickButton;
					if(_allowAnalogButton)
					{
						settings.scanFlags = settings.scanFlags | ScanFlags.JoystickAxis;
					}
					InputManager.StartScan(settings, HandleJoystickButtonScan);
				}
				else
				{
					settings.scanFlags = ScanFlags.Key;
					InputManager.StartScan(settings, HandleKeyScan);
				}

			}
		}
		
		private bool HandleKeyScan(ScanResult result)
		{
			//	When you return false you tell the InputManager that it should keep scaning for other keys
			if(!IsKeyValid(result.key))
				return false;
			
			//	The key is KeyCode.None when the timeout has been reached or the scan has been canceled
			if(result.key != KeyCode.None)
			{
				//	If the key is KeyCode.Backspace clear the current binding
				result.key = (result.key == KeyCode.Backspace || result.key == KeyCode.Escape) ? KeyCode.None : result.key;
				if(_changePositiveKey)
				{					
					_axisConfig.positive = result.key;
				}
				else
				{
					_axisConfig.negative = result.key;
				}
				_keyDescription.text = (result.key == KeyCode.None) ? "?" : result.key.ToString();
			}
			else
			{
				KeyCode currentKey = GetCurrentKeyCode();
				_keyDescription.text = (currentKey == KeyCode.None) ? "?" : currentKey.ToString();
			}

		    StartCoroutine(EndBinding());
            return true;
		}

		private bool IsKeyValid(KeyCode key)
		{
			bool isValid = true;

			if(_rebindType == RebindType.Keyboard)
			{
				if((int)key >= (int)KeyCode.JoystickButton0)
					isValid = false;
				else if(key == KeyCode.LeftApple || key == KeyCode.RightApple)
					isValid = false;
				else if(key == KeyCode.LeftWindows || key == KeyCode.RightWindows)
					isValid = false;
			}
			else
			{
				isValid = false;
			}

			return isValid;
		}

		private bool HandleJoystickButtonScan(ScanResult result)
		{
			if(result.scanFlags == ScanFlags.JoystickButton)
			{
				//	When you return false you tell the InputManager that it should keep scaning for other keys
				if(!IsJoytickButtonValid(result.key))
					return false;
				
				//	The key is KeyCode.None when the timeout has been reached or the scan has been canceled
				if(result.key != KeyCode.None)
				{
					//	If the key is KeyCode.Backspace clear the current binding
					result.key = (result.key == KeyCode.Backspace || result.key == KeyCode.Escape) ? KeyCode.None : result.key;
					_axisConfig.type = InputType.Button;
					if(_changePositiveKey)
					{
						_axisConfig.positive = result.key;
					}
					else
					{
						_axisConfig.negative = result.key;
					}
					_keyDescription.text = (result.key == KeyCode.None) ? "?" : result.key.ToString();
				}
				else
				{
					if(_axisConfig.type == InputType.Button)
					{
						KeyCode currentKey = GetCurrentKeyCode();
						_keyDescription.text = (currentKey == KeyCode.None) ? "?" : currentKey.ToString();
					}
					else
					{
						_keyDescription.text = (_axisConfig.invert ? "-" : "+") + _axisNames[_axisConfig.axis];
					}
				}
				
			}
			else
			{
				//	The axis is negative when the timeout has been reached or the scan has been canceled
				if(result.joystickAxis >= 0)
				{
					_axisConfig.type = InputType.AnalogButton;
					_axisConfig.invert = result.joystickAxisValue < 0.0f;
					_axisConfig.SetAnalogButton(_joystick, result.joystickAxis);
					_keyDescription.text = (_axisConfig.invert ? "-" : "+") + _axisNames[_axisConfig.axis];
				}
				else
				{
					if(_axisConfig.type == InputType.AnalogButton)
					{
						_keyDescription.text = (_axisConfig.invert ? "-" : "+") + _axisNames[_axisConfig.axis];
					}
					else
					{
						KeyCode currentKey = GetCurrentKeyCode();
						_keyDescription.text = (currentKey == KeyCode.None) ? "?" : currentKey.ToString();
					}
				}
				
			}
			
			return true;
		}

		private bool IsJoytickButtonValid(KeyCode key)
		{
			bool isValid = true;
			
			if(_rebindType == RebindType.GamepadButton)
			{
				//	Allow KeyCode.None to pass because it means that the scan has been canceled or the timeout has been reached
				//	Allow KeyCode.Backspace to pass so it can clear the current binding
				if((int)key < (int)KeyCode.JoystickButton0 && key != KeyCode.None && key != KeyCode.Backspace)
					isValid = false;
			}
			else
			{
				isValid = false;
			}
			
			return isValid;
		}

		private bool HandleJoystickAxisScan(ScanResult result)
		{
			//	The axis is negative when the timeout has been reached or the scan has been canceled
			if(result.joystickAxis >= 0)
				_axisConfig.SetAnalogAxis(_joystick, result.joystickAxis);

			
			_keyDescription.text = _axisNames[_axisConfig.axis];
			return true;
		}

		private KeyCode GetCurrentKeyCode()
		{
			if(_rebindType == RebindType.GamepadAxis)
				return KeyCode.None;

			if(_changePositiveKey)
			{
				return _axisConfig.positive;
			}
			else
			{
				return _axisConfig.negative;
			}
		}
	}
}