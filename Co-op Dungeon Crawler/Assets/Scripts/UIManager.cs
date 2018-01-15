using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : NetworkBehaviour {

    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int clientPlayerHealth;
    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int Player2Health;
    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int Player3Health;
    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int Player4Health;

    [SerializeField]
    private CanvasGroup objectiveCanvasGroup;
    [SerializeField]
    private CanvasGroup pauseMenuCanvasGroup;
    private bool isObjectivesOpen = true;
    private bool isPauseOpen = false;

    private string[] playerIDs;
    private bool alreadySet = false;

    [SerializeField]
    private Slider healthText;
    [SerializeField]
    private Text objectiveText;


    void OnEnable()
    {
        PlayerBehaviour.OnSendHealthInfo += HandleOnUpdateHealth;
        NetworkPlayerCustom.OnSendPlayerID += HandleOnSendPlayerID;

        //playerIDs = new string[FindObjectOfType<NetworkManager>().matchSize];
    }
    void OnDisable()
    {
        PlayerBehaviour.OnSendHealthInfo -= HandleOnUpdateHealth;
        NetworkPlayerCustom.OnSendPlayerID -= HandleOnSendPlayerID;
    }

    void Awake()
    {
        playerIDs = new string[4];
        objectiveText.text = "-Find the Manor and stop whatever is attacking the Town";
    }

    //Opens the objectives and the pause menu by keypress;
    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (!isObjectivesOpen)
            {
                objectiveCanvasGroup.interactable = true;
                objectiveCanvasGroup.blocksRaycasts = true;
                objectiveCanvasGroup.alpha = 1;

                isObjectivesOpen = true;
            }
            else
            {
                objectiveCanvasGroup.interactable = false;
                objectiveCanvasGroup.blocksRaycasts = false;
                objectiveCanvasGroup.alpha = 0;

                isObjectivesOpen = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!isPauseOpen)
            {
                pauseMenuCanvasGroup.interactable = true;
                pauseMenuCanvasGroup.blocksRaycasts = true;
                pauseMenuCanvasGroup.alpha = 1;

                isPauseOpen = true;
            }
            else
            {
                pauseMenuCanvasGroup.interactable = false;
                pauseMenuCanvasGroup.blocksRaycasts = false;
                pauseMenuCanvasGroup.alpha = 0;

                isPauseOpen = false;
            }
        }
    }

    //Takes player health and displays it as a health bar
    private void HandleOnUpdateHealth(int health)
    {
        clientPlayerHealth = health;
        healthText.value = (float)clientPlayerHealth / 100;
    }

    //This is meant to get a player ID, and add it to the array.
    private void HandleOnSendPlayerID(string playerID)
    {
        alreadySet = false;

        for (int i = 0; i < playerIDs.Length; i++)
        {
            if ((playerIDs[i] == "0" || playerIDs[i] == null) && !alreadySet)
            {
                playerIDs[i] = playerID;
                alreadySet = true;
            }
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.SpawnObjects();

        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerIDs[i] = FindObjectOfType<NetworkControl>().client.connection.playerControllers[i].playerControllerId.ToString();
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
