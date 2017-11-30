using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueSelector : MenuSelector
{
    private int[] Options;

    public int min;
    public int max;
    public int step;

    void Awake()
    {

        int length = (max - min) / step + 1;
        Options = new int[length];
        int value = min;
        for (int i = 0; i < length; i++)
        {
            Options[i] = value;
            value += step;
        }

        index = 0;
        OptionValue.text = Options[index].ToString();

    }
    
    protected override string Option => Options[index].ToString();

    protected override string NextOption
    {
        get
        {
            if (index + 1 < Options.Length)
            {
                index++;
                return Options[index].ToString();
            }
            else
            {
                index = 0;
                return Options[index].ToString();
            }
        }
    }

    protected override string PreviousOption
    {
        get
        {
            if (index - 1 >= 0)
            {
                index--;
                return Options[index].ToString();
            }
            else
            {
                index = Options.Length - 1;
                return Options[index].ToString();                
            }
            
        }
    }
}
