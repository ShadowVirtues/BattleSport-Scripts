using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSelector : MenuSelector
{

    public List<GameObject> Options;        //For tank selector the options are actual tank prefabs (the tank collection specifically for UI tho, without thrusters and colliders)

    void Awake()                     //Stuff to do when the menu loads
    {
        index = 0;
        Options[index].SetActive(true);                      //So show the first tank    //TODO get the tank number from 'save' container to show previously selected option
        OptionValue.text = Options[index].name.ToUpper();

    }

    public string Option => Options[index].name;  //This gets called when confirming game settings, for transfering the selected tank to the loading scene (we compare the name of Tank UI prefabs with "playable" Tank prefabs

    protected override string NextOption    //To select the next tank
    {
        get
        {
            Options[index].SetActive(false);    //A slight hack, cuz during 'get'ting the tank name, we also enable and disable tank models rotating on the screen, so this line disables previosly selected tank, before changing the index
            if (index + 1 < Options.Count)      //Same stuff as for value selector, if we are still within actual array length
            {
                index++;                        //Increment the index
                Options[index].SetActive(true); //Enable the tank with new index
                return Options[index].name.ToUpper();   //And finally return the name of the tank to show (all options in the menu are written with uppercase letters)
            }
            else                                //If we went over array length, show the first tank with index 0
            {
                index = 0;
                Options[index].SetActive(true);
                return Options[index].name.ToUpper();
            }
        }
    }

    protected override string PreviousOption    //To select the previous tank
    {
        get
        {
            Options[index].SetActive(false);    //Same stuff, just in the opposite order
            if (index - 1 >= 0)
            {
                index--;
                Options[index].SetActive(true);
                return Options[index].name.ToUpper();
            }
            else
            {
                index = Options.Count - 1;
                Options[index].SetActive(true);
                return Options[index].name.ToUpper();
            }

        }
    }

   








}
