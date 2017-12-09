using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels/Menus")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject soundSetPanel;
    [SerializeField] private GameObject gameSetPanel;

    [Header("Other")]
    [SerializeField] private GameObject resume; //TODO Maybe not GameObject
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject gameSettings;

    [SerializeField] private AudioSource UISource;
    [SerializeField] private AudioClip select;

    void OnEnable()
    {
        UISource.PlayOneShot(select);

        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
        soundSetPanel.SetActive(false);
        gameSetPanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resume);

    }

    public void ResumeGame()
    {
        gameObject.SetActive(false);
        GameController.Controller.gameUI.gameObject.SetActive(false);
        GameController.Controller.Pause();

    }

    public void SettingsPanel()
    {
        UISource.PlayOneShot(select);

        settingsPanel.SetActive(true);
        mainPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(gameSettings);


    }

    public void BackFromSettings()
    {
        UISource.PlayOneShot(select);

        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(settings);

    }



    void Update()
    {
        //print(EventSystem.current.currentSelectedGameObject);
        
        //print(EventSystem.current.IsPointerOverGameObject());

        //EventSystem.RaycastAll()
    }










}
