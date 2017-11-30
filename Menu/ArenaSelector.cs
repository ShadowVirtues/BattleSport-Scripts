using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaSelector : MenuSelector
{

    public List<Arena> Options;

    protected override string Option => Options[index].Name;

    protected override string NextOption
    {
        get
        {
            for (int i = index + 1; i < Options.Count; i++)
            {
                if (Options[i] != null)
                {
                    index = i;
                    return Options[i].Name;
                }
            }
            for (int i = 0; i < index; i++)
            {
                if (Options[i] != null)
                {
                    index = i;
                    return Options[i].Name;
                }
            }
            return Options[index].Name;

            //if (index + 1 < Options.Count)
            //{
            //    return Options[index + 1].Name;
            //}
            //else
            //{
            //    return Options[0].Name;
            //}
        }
    }

    protected override string PreviousOption
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
                if (Options[i] != null)
                {
                    index = i;
                    return Options[i].Name;
                }
            }
            return Options[index].Name;

        }
    }

    void Awake()
    {
        for (int i = 0; i < Options.Count; i++)
        {
            if (Options[i] != null)
            {
                index = i;
                OptionValue.text = Options[index].Name;
                break;
            }
        }
       
    }

}
