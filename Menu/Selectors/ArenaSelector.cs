using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ArenaSelector : MenuSelector
{
    [Header("Options")]
    [SerializeField] private List<Arena> Options;     //The list of Arena ScriptableObjects to shoose from, gets filled in the inspector

    [Header("Fields")]                          //Fields of all the stuff we fill on screen when changing the arena
    [SerializeField] private Text ballText;     //Ball description
    [SerializeField] private Text goalText;     //Goal description
    [SerializeField] private Text sizeText;     //Text with the size of the arena

    [SerializeField] private VideoPlayer arenaVideoPlayer;      //Reference to VideoPlayer to change its clip when switching the arena
    [SerializeField] private RawImage arenaVideo;               //Reference to RawImage containing Render Texture that VideoPlayer renders video to

    [SerializeField] private Image ballImage;       //Ball icon image
    [SerializeField] private Image goalImage;       //Goal icon image
    [SerializeField] private Image[] powerupImage;  //4 powerups images

    [Header("Icons")]
    [SerializeField] private Sprite[] ballIcons;    //All ball and goal icons to show them dependion on when enum for the ball or goal is selected in the arena
    [SerializeField] private Sprite[] goalIcons;
    //Arena file references powerups by their actual prefabs, that contain powerup icons themselves and that's how we get them. We don't reference balls and goals that way, so we need icons specifically
    
    public Arena Option => Options[index]; //This gets called when confirming game settings, for transfering the selected value to the loading scene (we need the whole Arena object with all its fields)

    protected override string NextOption                //To get the next arena from the list
    {
        get                 //So, as we have not all arenas created, the arena list has full 70 item length, 
        {                   //and the arenas that are made are put into their actual number in the list (Arena 22 to Options[22]), that way we have to look for the next non-null arena in the list
            for (int i = index + 1; i < Options.Count; i++) //From the next item from current index, and until the end of the list
            {
                if (Options[i] != null) //If the arena with current index isn't in the list, we look further, but if current index is NOT null and has some arena
                {
                    ChangeIndex(i);          //Set the index to this arena index, show its name on screen and fill everything on screen about it
                    return Options[index].Name;
                }
            }
            for (int i = 0; i < index; i++) //If we didn't return anything from checking "in front" of current index, look in the list from the very start
            {
                if (Options[i] != null) //All the same
                {
                    ChangeIndex(i);
                    return Options[index].Name;
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
                    ChangeIndex(i);
                    return Options[index].Name;
                }
            }
            for (int i = Options.Count - 1; i > index; i--)
            {
                if (Options[i] != null) //We write the same stuff 4 times and turns out we can't actually make it into function, because we don't necessarily return at every iteration, returning only when the condition is met
                {
                    ChangeIndex(i);
                    return Options[index].Name;
                }
            }
            return Options[index].Name;

        }
    }
    
    private void ChangeIndex(int i)     //Function that fills all fields when changing arena selector
    {
        index = i;  //Set index of the selector

        Arena arena = Options[index];   //Just shorter reference to arena file

        ballImage.sprite = ballIcons[(int)arena.ballType];  //Set ball icon to one from array with all icons, depending on the enum value in Arena file
        string ballDesc = arena.ballDescription == String.Empty ? "NORMAL" : arena.ballDescription; //Generating the second line of ball description, which is "Normal" if the description is empty, if not, getting the actual description
        ballText.text = arena.ballType.ToString().ToUpper() + "\r\n" + ballDesc; //Put the ball description into the field

        if (arena.goalType == Goal.GoalType.OneSided)
        {
            goalText.text = "1 SIDED\r\n";   //Depending on goal type, set the first line of goal description and put the next line with \r\n
        }
        else if (arena.goalType == Goal.GoalType.TwoSided || arena.goalType == Goal.GoalType.TwoSidedPhantom)
        {
            goalText.text = "2 SIDED\r\n";
        }
        else if (arena.goalType == Goal.GoalType.FourSided)
        {
            goalText.text = "4 SIDED\r\n";
        }

        goalText.text += arena.goalDescription == String.Empty ? "NORMAL" : arena.goalDescription;   //Add the description if any specified in Arena file, if not, write "Normal"
        goalImage.sprite = goalIcons[(int)arena.goalType];  //Get goal icon the same way as for ball
        
        //When switching arenas, videos of their preview can't instantly switch, because it takes a bit to buffer them, so we fade them in as soon as they are ready to play
        StopCoroutine(nameof(FadeInVideo)); //Stop fading coroutine if it was running already
        if (DOTween.IsTweening(arenaVideo)) //Stop fading in the video if it was going already
        {
            DOTween.Kill(arenaVideo);
        }
        arenaVideo.color = new Color(1, 1, 1, 0);   //When switched arena, instantly make it transparent
        arenaVideoPlayer.clip = arena.arenaVideo;   //Put in the video of selected arena
        StartCoroutine(nameof(FadeInVideo));        //Start coroutine waiting for the video to get ready to be shown and then show it
        
        sizeText.text = arena.Size.ToString().ToUpper();    //Fill arena size from enum

        int powerupCount = arena.Powerups.Length > 4 ? 4 : arena.Powerups.Length;   //Decide how many powerups to show, if there are more than 4, show only first 4

        for (int j = 0; j < powerupCount; j++)  //For all powerups to show
        {
            powerupImage[j].enabled = true;         //Enable the image (we disable them, if not less than 4 powerups)
            powerupImage[j].sprite = arena.Powerups[j].icon;    //Set the icon
        }

        if (powerupCount < 4)   //If there are less than 4 powerups, disable redundant images
        {
            for (int j = powerupCount; j < 4; j++)
            {
                powerupImage[j].enabled = false;
            }
        }

    }

    IEnumerator FadeInVideo()
    {
        while (arenaVideoPlayer.isPrepared == false) yield return null;     //While the video is not ready to show, wait
        arenaVideo.DOFade(1, 0.25f);    //Then fade in the video

    }

    public void SetIndex(int ind)   //Set index for selector, when loading previous state of the menu
    {
        ChangeIndex(ind);
        OptionValue.text = Options[index].Name;
    }

}
