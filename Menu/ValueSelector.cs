using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueSelector : MenuSelector   //This selector selects only numeric values like "Period Time", "Period Number"
{    
    private int[] Options;      //Array that fills in with and contains all the available numeric options for selection

    [Header("Options")]
    [SerializeField] private int min;
    [SerializeField] private int max;   //Variables to define the numeric range and step for selection
    [SerializeField] private int step;

    protected override void Awake()        //Stuff to do when the menu loads
    {
        base.Awake();   //Call base class' Awake

        int length = (max - min) / step + 1;    //Calculate the number of items for selection to
        Options = new int[length];              //initialize the array with
        int value = min;                        //This value will get increased as the array fills
        for (int i = 0; i < length; i++)        //For the array length
        {
            Options[i] = value;                 //Set current index with current value
            value += step;                      //Increase the value for the next step to fill it in
        }
        
    }
    
    public int Option => Options[index];  //This gets called when confirming game settings, for transfering the selected value to the loading scene

    protected override string NextOption    //To get next option and increment the index
    {
        get
        {
            if (index + 1 < Options.Length)     //If we are still not exceeding the length of the array
            {
                index++;                        //Increment the index and show the next option
                return Options[index].ToString();
            }
            else    //If we reach the end of available options, set index to 0 and show the first item
            {
                index = 0;
                return Options[index].ToString();
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
                return Options[index].ToString();
            }
            else                //If we go below 0 in array index
            {
                index = Options.Length - 1; //Set index to the last option of the array and show it
                return Options[index].ToString();                
            }
            
        }
    }
    
    public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu, or switching between "Period (Minutes)" and "Play To Score"
    {
        index = ind;
        OptionValue.text = Options[index].ToString();
    }

}
