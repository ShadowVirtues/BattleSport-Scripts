using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {


        Destroy(GameObject.Find(nameof(StartupController)));    //Destroy StartupController of this scene (cuz it gets DontDestroyOnLoad in its Awake)

    }






    public void InstantActionSetup()
    {
        SceneManager.LoadScene("InstantActionSetup");       
    }





}
