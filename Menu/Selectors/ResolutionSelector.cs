using System;
using System.Linq;
using UnityEngine;

public class ResolutionSelector : MenuSelector
{
    private Resolution[] Options;      //Array of available resolutions for user to select. Gets filled here in Awake
    
    protected override void Awake()        //Stuff to do when the menu loads
    {
        base.Awake();   //Call base class' Awake

        Resolution[] resolutions = Screen.resolutions;  //Get all available screen resolutions. This will output all resolutions with refresh rate as well
        //And we only leave the options with the highest refresh rate
        int highestRefreshRate = resolutions.Max(x => x.refreshRate);   //So find the highest index
        
        Options = resolutions.Where(x => x.refreshRate == highestRefreshRate).ToArray();    //Filter resolutions array to only that refresh rate and assign it to 'Options'

    }

    public Resolution Option => Options[index];  //This gets called when confirming game settings

    protected override string NextOption()    //To get next option and increment the index
    {
        if (index + 1 < Options.Length)     //If we are still not exceeding the length of the array
        {
            index++;                        //Increment the index and show the next option
            return FormatResolution(Options[index]);
        }
        else    //If we reach the end of available options, set index to 0 and show the first item
        {
            index = 0;
            return FormatResolution(Options[index]);
        }           
    }

    protected override string PreviousOption()    //To get previous option and decrement the index
    {
        if (index - 1 >= 0) //If we are still not going below 0 array index
        {
            index--;        //Decrement the index and show the previous option
            return FormatResolution(Options[index]);
        }
        else                //If we go below 0 in array index
        {
            index = Options.Length - 1; //Set index to the last option of the array and show it
            return FormatResolution(Options[index]);
        }         
    }

    private string FormatResolution(Resolution res) //Format the resolution from 'Resolution' struct to string
    {
        return $"{res.width}x{res.height}";     //Format is: 1280x720
        //@{res.refreshRate}
    }

    public void SetSelectorValue()  //Setting selector value when showing settings menu
    {
        Resolution current = new Resolution     //Create resolution struct with current window width and monitor refresh rate
        {
            width = Screen.width,
            height = Screen.height,
            refreshRate = Screen.currentResolution.refreshRate
        };
        int ind = Array.IndexOf(Options, current);  //Find the index in the Options array with that resolution
        if (ind != -1)  //If it didn't return "-1", means that such resolution wasn found
        {
            index = ind;    //Then set the index with such resolution and set the selector value
            OptionValue.text = FormatResolution(Options[index]);
        }
        else    //Or if such resolution wasn't found
        {
            index = Options.Length - 1; //Set the highest possible resolution for the selector
            OptionValue.text = FormatResolution(Options[index]);
        }

    }


    //public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu
    //{
    //    index = ind;        //Assign actual index of the selector to passed to the function index
    //    OptionValue.text = Options[index].ToString();
    //}


    //public void SetValue(Resolution value)   //Function to set the index and value from specifying the actual value, instead of index (used in Settings)
    //{
    //    index = Array.IndexOf(Options, value);  //Find the index of the value in the array
    //    OptionValue.text = FormatResolution(Options[index]);
    //}














}
