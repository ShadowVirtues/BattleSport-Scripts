using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringSelector : MenuSelector
{
    [SerializeField] private string[] Options;      //Array that fills in with and contains all the available numeric options for selection



    //protected override void Awake()        //Stuff to do when the menu loads
    //{
    //    base.Awake();   //Call base class' Awake
        
    //}

    public string Option => Options[index];  //This gets called when confirming game settings, for transfering the selected value to the loading scene

    protected override string NextOption    //To get next option and increment the index
    {
        get
        {
            if (index + 1 < Options.Length)     //If we are still not exceeding the length of the array
            {
                index++;                        //Increment the index and show the next option
                return Options[index];
            }
            else    //If we reach the end of available options, set index to 0 and show the first item
            {
                index = 0;
                return Options[index];
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
                return Options[index];
            }
            else                //If we go below 0 in array index
            {
                index = Options.Length - 1; //Set index to the last option of the array and show it
                return Options[index];
            }

        }
    }

    public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu, or switching between "Period (Minutes)" and "Play To Score"  (used in menus)
    {
        index = ind;        //Assign actual index of the selector to passed to the function index
        OptionValue.text = Options[index];
    }
    
    //public void SetValue(int value)   //Function to set the index and value from specifying the actual value, instead of index (used in Settings)
    //{
    //    index = Array.IndexOf(Options, value);  //Find the index of the value in the array
    //    OptionValue.text = Options[index].ToString();
    //}






















}
