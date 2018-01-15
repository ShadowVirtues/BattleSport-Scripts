using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TankSelector : MenuSelector
{
    [Header("Options")]
    public List<Tank> Options;        //For tank selector the options are actual tank prefabs (the tank collection specifically for UI tho, without thrusters and colliders)

    [SerializeField] private Image[] statsSliders;  //Sliders for tank parameters to fill them to respective state when user changes a tank

    public string Option => Options[index].name;  //This gets called when confirming game settings, for transfering the selected tank to the loading scene (we compare the name of Tank UI prefabs with "playable" Tank prefabs

    protected override string NextOption    //To select the next tank
    {
        get
        {
            Options[index].gameObject.SetActive(false);    //A slight hack, cuz during 'get'ting the tank name, we also enable and disable tank models rotating on the screen, so this line disables previosly selected tank, before changing the index
            if (index + 1 < Options.Count)      //Same stuff as for value selector, if we are still within actual array length
            {
                index++;                        //Increment the index
                ChangeIndex();   //Enable the tank with new index and fill the parameters sliders
                return Options[index].name.ToUpper();   //And finally return the name of the tank to show (all options in the menu are written with uppercase letters)
            }
            else                                //If we went over array length, show the first tank with index 0
            {
                index = 0;
                ChangeIndex();
                return Options[index].name.ToUpper();
            }
        }
    }

    protected override string PreviousOption    //To select the previous tank
    {
        get
        {
            Options[index].gameObject.SetActive(false);    //Same stuff, just in the opposite order
            if (index - 1 >= 0)
            {
                index--;
                ChangeIndex();
                return Options[index].name.ToUpper();
            }
            else
            {
                index = Options.Count - 1;
                ChangeIndex();
                return Options[index].name.ToUpper();
            }

        }
    }

    private void ChangeIndex()  //Function that does the additional stuff to changing the selector value
    {
        DOTween.Kill(this);     //Kill the playing tweens filling tank parameters sliders, if they were playing
        foreach (Image im in statsSliders)  //Set fill amount for sliders to 0
        {
            im.fillAmount = 0;
        }       

        float dur = 0.75f;  //Duration of filling parameter with the value 100

        Tank tank = Options[index];     //Just shorter reference

        tank.gameObject.SetActive(true);    //Enable tank that plyaer selected

        statsSliders[0].DOFillAmount(tank.Acceleration / 100, dur * tank.Acceleration / 100).SetId(this);
        statsSliders[1].DOFillAmount(tank.TopSpeed / 100, dur * tank.TopSpeed / 100).SetId(this);
        statsSliders[2].DOFillAmount(tank.FirePower / 100, dur * tank.FirePower / 100).SetId(this);         //Fill the "sliders", where the time it takes to fill to the amount takes time proportional to this amount to fill
        statsSliders[3].DOFillAmount(tank.Armor / 100, dur * tank.Armor / 100).SetId(this);                 //Set ID to this TankSelector instance, because it's the same for all sliders of this selector and different for other player
        statsSliders[4].DOFillAmount(tank.BallHandling / 100, dur * tank.BallHandling / 100).SetId(this);
        
    }

    public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu
    {        
        index = ind;
        ChangeIndex();
        OptionValue.text = Options[index].name.ToUpper();
    }

    
}
