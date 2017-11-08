using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class UIManager : NetworkBehaviour {

    [SyncVar(hook = "HandleOnUpdateHealth")]
    public int player1Health;

    [SerializeField]
    private Slider healthText;


    void OnEnable()
    {
        PlayerBehaviour.OnSendHealthInfo += HandleOnUpdateHealth;
    }
	void OnDisable()
    {
        PlayerBehaviour.OnSendHealthInfo -= HandleOnUpdateHealth;
    }

    //Takes player health and displays it as a health bar
    private void HandleOnUpdateHealth(int health)
    {
        player1Health = health;
        healthText.value = (float) player1Health / 100;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.SpawnObjects();
    }
}
