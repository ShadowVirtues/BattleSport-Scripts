using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickMatch : MonoBehaviour
{
    public Arena[] AllArenas;   //List of arenas to choose from for Quick Match

    public List<string[]> tankOptions = new List<string[]>  //Possibilities of different random tanks cases
    {
        new[] {"Sliver" , "Spitfire"},
        new[] {"Sliver" , "Hunter"},
        new[] {"Sliver" , "Runner"},
        new[] {"Spitfire" , "Hunter"},  //Out of two, randomly picks if which one to set for each player
        new[] {"Spitfire" , "Runner"},
        new[] {"Hunter" , "Runner"},
        new[] {"Invader" , "T-Shark"},
        new[] {"Invader" , "Repulse"},
        new[] { "T-Shark", "Repulse"},
        new[] { "T-Shark", "Scorpion"},
        new[] { "Scorpion", "Brawler"},
        new[] {"Invader" , "T-Shark"},  //Double chance for better tanks
        new[] {"Invader" , "Repulse"},
        new[] { "T-Shark", "Repulse"},
        new[] { "T-Shark", "Scorpion"},
        new[] { "Scorpion", "Brawler"},
    };




}
