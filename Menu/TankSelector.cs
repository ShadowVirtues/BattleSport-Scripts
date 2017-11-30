using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankSelector : MenuSelector
{

    public List<GameObject> Options;

    protected override string Option => Options[index].name.ToUpper();

    protected override string NextOption
    {
        get
        {
            Options[index].SetActive(false);
            if (index + 1 < Options.Count)
            {
                index++;
                Options[index].SetActive(true);
                return Options[index].name.ToUpper();
            }
            else
            {
                index = 0;
                Options[index].SetActive(true);
                return Options[index].name.ToUpper();
            }
        }
    }

    protected override string PreviousOption
    {
        get
        {
            Options[index].SetActive(false);
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

    void Awake()
    {        
        index = 0;
        Options[index].SetActive(true);
        OptionValue.text = Options[index].name.ToUpper();

    }









}
