using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

public class DeviceSelector : StringSelector
{

    public void UpdateDevices()     //Function that gets called when entering key bindings menu to update currently connected joysticks
    {
        Options.RemoveRange(2,Options.Count - 2);   //First two items in Options List are always "Keyboard" and "Keyboard+Mouse", so remove everything beyond them
        
        Options.AddRange(InputManager.GetJoystickNames());  //Add all the entries of joysticks
        
        for (int i = 2; i < Options.Count; i++) //For all the joystick names in the array
        {
            if (String.IsNullOrEmpty(Options[i]))   //If the name is empty, means the joystick got disconnected
            {
                Options[i] = "{Unplugged}"; //State that on the selector if it gets selected (you won't be able to save the device for the player tho)
            }

            if (Options[i].Contains(" (Controller)"))  //For my controller, there was this suffix, which just takes too much space, the selector Option just overflows //TEST with other controllers
            {
                Options[i] = Options[i].Remove(Options[i].Length - 13); //Remove the amount of characters from the string
            }

            Options[i] = Options[i].TrimEnd(' ');
        }
    }

    public void UpdateSelector()    //COMM
    {
        UpdateDevices();
        OptionValue.text = Options[index];
    }







}
