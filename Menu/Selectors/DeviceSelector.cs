using System;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using UnityEngine;

public class DeviceSelector : StringSelector
{

    public void UpdateDevices()
    {
        Options.RemoveRange(2,Options.Count - 2);
        
        Options.AddRange(InputManager.GetJoystickNames());
        
        for (int i = 2; i < Options.Count; i++)
        {
            if (String.IsNullOrEmpty(Options[i]))
            {
                Options[i] = "{Unplugged}";
            }

            if (Options[i].Contains(" (Controller)"))
            {
                Options[i] = Options[i].Remove(Options[i].Length - 13);
            }
        }
    }









}
