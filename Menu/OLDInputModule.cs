﻿#region [License]
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
using System;

namespace TeamUtility.IO
{	
	public class OLDInputModule : PointerInputModule
	{
		public const string VERSION = "5.5";

	    public bool PlayerOne;
	    public bool PlayerTwo;  //Changed
	    public AudioSource click;


		private float m_PrevActionTime;
		private Vector2 m_LastMoveVector;
		private int m_ConsecutiveMoveCount = 0;

		private Vector2 m_LastMousePosition;
		private Vector2 m_MousePosition;

		protected OLDInputModule()
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

		public override bool ShouldActivateModule()
		{
			if(!base.ShouldActivateModule())
				return false;

			var shouldActivate = m_ForceModuleActive;
		    if (PlayerOne)  //Changed
		    {
		        shouldActivate |= InputManager.GetButtonDown(m_SubmitButton, PlayerID.One);     //Changed
		        shouldActivate |= InputManager.GetButtonDown(m_CancelButton, PlayerID.One);     //Changed
		        //shouldActivate |= !Mathf.Approximately(InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.One), 0.0f);  //Changed
		        shouldActivate |= !Mathf.Approximately(InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.One), 0.0f);    //Changed
            }
		    if (PlayerTwo)  //Changed
            {
		        shouldActivate |= InputManager.GetButtonDown(m_SubmitButton, PlayerID.Two);     //Changed           
		        shouldActivate |= InputManager.GetButtonDown(m_CancelButton, PlayerID.Two);     //Changed            
		        //shouldActivate |= !Mathf.Approximately(InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.Two), 0.0f);  //Changed           
		        shouldActivate |= !Mathf.Approximately(InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.Two), 0.0f);    //Changed
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
			if(eventSystem.currentSelectedGameObject == null)
				return false;

			var data = GetBaseEventData();


			if(InputManager.GetButtonDown(m_SubmitButton, PlayerID.One) || InputManager.GetButtonDown(m_SubmitButton, PlayerID.Two))    //Changed
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.submitHandler);

			if(InputManager.GetButtonDown(m_CancelButton, PlayerID.One) || InputManager.GetButtonDown(m_CancelButton, PlayerID.Two))    //Changed
				ExecuteEvents.Execute(eventSystem.currentSelectedGameObject, data, ExecuteEvents.cancelHandler);


			return data.used;
		}

		private Vector2 GetRawMoveVector()  //VERY Changed
		{
		    Vector2 P1Vector = Vector2.zero;
		    Vector2 P2Vector = Vector2.zero;

            if (PlayerOne)
		    {
		        P1Vector = new Vector2(0, InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.One));   //InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.One)
            }
		    if (PlayerTwo)
		    {
		        P2Vector = new Vector2(0, InputManager.GetAxisRaw(m_VerticalAxis, PlayerID.Two));   //InputManager.GetAxisRaw(m_HorizontalAxis, PlayerID.Two)
            }
                        
            return (P1Vector + P2Vector).normalized;           
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




		    if (PlayerOne)      //Changed
		    {
		        //allow |= InputManager.GetButtonDown(m_HorizontalAxis, PlayerID.One);      //Changed
                allow |= InputManager.GetButtonDown(m_VerticalAxis, PlayerID.One);      //Changed
            }
		    if (PlayerTwo)      //Changed
            {
		        //allow |= InputManager.GetButtonDown(m_HorizontalAxis, PlayerID.Two);      //Changed
                allow |= InputManager.GetButtonDown(m_VerticalAxis, PlayerID.Two);      //Changed
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

		    if (allow) click.Play();
            
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