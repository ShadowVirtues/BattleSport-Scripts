using System.Collections.Generic;
using UnityEngine;

public class StringSelector : MenuSelector
{
    [SerializeField] protected List<string> Options; //List that gets filled from Inspector
    
    public string Option => Options[index];  //This gets called when confirming settings

    protected override string NextOption()    //To get next option and increment the index
    {
        if (index + 1 < Options.Count)     //If we are still not exceeding the length of the list
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

    protected override string PreviousOption()    //To get previous option and decrement the index
    {
        if (index - 1 >= 0) //If we are still not going below 0 array index
        {
            index--;        //Decrement the index and show the previous option
            return Options[index];
        }
        else                //If we go below 0 in array index
        {
            index = Options.Count - 1; //Set index to the last option of the array and show it
            return Options[index];
        }           
    }

    public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu
    {
        if (ind > Options.Count - 1 || ind < 0) //Check if the setting value can even be set
        {
            ind = Options.Count - 1;    //If not, set the max value
            Debug.LogError("WTF Are you trying to set?");
        }
        index = ind;        //Assign actual index of the selector passed to the function parameter
        OptionValue.text = Options[index];
    }
    



}
