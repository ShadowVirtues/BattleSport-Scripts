using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TeamUtility.IO.Examples;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyBindingsMenu : MonoBehaviour
{
    [SerializeField] private Text playerLabel;
    [SerializeField] private DeviceSelector device;

    [Header("Panels")]
    [SerializeField] private GameObject keyboardPanel;
    [SerializeField] private GameObject gamepadPanel;
    

    private CustomInputModule inputModule;
    

    void Awake()
    {
        inputModule = EventSystem.current.GetComponent<CustomInputModule>();
        
    }

    void OnEnable()
    {
        RebindInput[] rebinds = GetComponentsInChildren<RebindInput>(true);

        if (GameController.Controller.PausedPlayer == PlayerID.One)
        {
            foreach (RebindInput rebind in rebinds)
            {
                rebind._inputConfigName = "PlayerOneConfiguration";   //TODO
            }
            playerLabel.text = "PLAYER ONE CONTROLS";
        }
        else if (GameController.Controller.PausedPlayer == PlayerID.Two)
        {
            foreach (RebindInput rebind in rebinds)
            {
                rebind._inputConfigName = "PlayerTwoConfiguration";   //TODO
            }
            playerLabel.text = "PLAYER TWO CONTROLS";
        }

        inputModule.Menu = true;

    }

    void OnDisable()
    {
        inputModule.Menu = false;
    }
    


















}
