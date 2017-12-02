using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSelector : MenuSelector
{
    [SerializeField] private List<Arena> Options;     //The list of Arena ScriptableObjects to shoose from, gets filled in the inspector

    void Awake()            //Stuff to do when the menu loads
    {
        for (int i = 0; i < Options.Count; i++)     //We need to find the first arena that is available in the list, so look through the whole list
        {
            if (Options[i] != null)
            {
                index = i;
                OptionValue.text = Options[index].Name; //Set the option value to found arena
                break;                                  //Don't look further if we found it
            }
        }

    }

    public Arena Option => Options[index]; //This gets called when confirming game settings, for transfering the selected value to the loading scene (we need the whole Arena object with all its fields)

    protected override string NextOption                //To get the next arena from the list
    {
        get                 //So, as we have not all arenas created, the arena list has full 70 item length, 
        {                   //and the arenas that are made are put into their actual number in the list (Arena 22 to Options[22]), that way we have to look for the next non-null arena in the list
            for (int i = index + 1; i < Options.Count; i++) //From the next item from current index, and until the end of the list
            {
                if (Options[i] != null) //If the arena with current index isn't in the list, we look further, but if current index is NOT null and has some arena
                {
                    index = i;          //Set the index to this arena index and show its name on screen
                    return Options[i].Name;
                }
            }
            for (int i = 0; i < index; i++) //If we didn't return anything from checking "in front" of current index, look in the list from the very start
            {
                if (Options[i] != null) //All the same
                {
                    index = i;
                    return Options[i].Name;
                }
            }
            return Options[index].Name; //There may be the case when there is just one single arena in the list, so if we didn't find any arena, just return the current one (IntelliSense asks for default value to return in case the loops don't return anything)
        }
    }

    protected override string PreviousOption    //All the same stuff, but in the opposite direction
    {
        get
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (Options[i] != null)
                {
                    index = i;
                    return Options[i].Name;
                }
            }
            for (int i = Options.Count - 1; i > index; i--)
            {
                if (Options[i] != null) //TODO Maybe replace this thing with actual function, cuz we literally write the same 5 lines 4 times in the code
                {
                    index = i;
                    return Options[i].Name;
                }
            }
            return Options[index].Name;

        }
    }

    

}
