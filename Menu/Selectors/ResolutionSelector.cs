using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResolutionSelector : MenuSelector
{
    private Resolution[] Options;      //Array that fills in with and contains all the available numeric options for selection

    

    protected override void Awake()        //Stuff to do when the menu loads
    {
        base.Awake();   //Call base class' Awake

        Resolution[] resolutions = Screen.resolutions;

        int highestRefreshRate = resolutions.Max(x => x.refreshRate);
        
        

        Options = resolutions.Where(x => x.refreshRate == highestRefreshRate).ToArray();

    }

    public Resolution Option => Options[index];  //This gets called when confirming game settings, for transfering the selected value to the loading scene

    protected override string NextOption    //To get next option and increment the index
    {
        get
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
    }

    protected override string PreviousOption    //To get previous option and decrement the index
    {
        get
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
    }

    public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu, or switching between "Period (Minutes)" and "Play To Score"  (used in menus)
    {
        index = ind;        //Assign actual index of the selector to passed to the function index
        OptionValue.text = Options[index].ToString();
    }

    private string FormatResolution(Resolution res)
    {
        return $"{res.width}x{res.height}"; //@{res.refreshRate}
    }

    public void SetValue(Resolution value)   //Function to set the index and value from specifying the actual value, instead of index (used in Settings)
    {
        index = Array.IndexOf(Options, value);  //Find the index of the value in the array
        OptionValue.text = FormatResolution(Options[index]);
    }

    public void SetSelectorValue()
    {
        Resolution current = new Resolution
        {
            width = Screen.width,
            height = Screen.height,
            refreshRate = Screen.currentResolution.refreshRate
        };
        int ind = Array.IndexOf(Options, current);
        if (ind != -1)
        {
            index = ind;
            OptionValue.text = FormatResolution(Options[index]);
        }
        else
        {
            index = Options.Length - 1;
            OptionValue.text = FormatResolution(Options[index]);
        }

    }















}
