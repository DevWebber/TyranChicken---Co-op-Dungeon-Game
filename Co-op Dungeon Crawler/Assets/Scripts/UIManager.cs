using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : NetworkBehaviour {

    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int clientPlayerHealth;
/*  [SyncVar(hook = "HandleOnUpdateHealth")]
    public int Player2Health;
    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int Player3Health;
    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int Player4Health;
*/

    private string[] playerIDs;
    private bool alreadySet = false;

    [SerializeField]
    private Slider healthText;


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
    }

    //Takes player health and displays it as a health bar
    private void HandleOnUpdateHealth(int health)
    {
        clientPlayerHealth = health;
        healthText.value = (float)clientPlayerHealth / 100;
    }

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

            Debug.Log(playerID);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.SpawnObjects();

        for (int i = 0; i < playerIDs.Length; i++)
        {
            playerIDs[i] = FindObjectOfType<NetworkManager>().client.connection.playerControllers[i].playerControllerId.ToString();
        }

    }
}
