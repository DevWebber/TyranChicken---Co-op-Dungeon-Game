using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerCustom : NetworkBehaviour {

    public delegate void SendPlayerID(string id);
    public static event SendPlayerID OnSendPlayerID;

    [SyncVar(hook = "OnPlayerIDChanged")]
    public string playerID;
    private Transform labelHolder;


    private void Awake()
    {
        labelHolder = transform.Find("PlayerLabel");
    }

    private void LateUpdate()
    {
        labelHolder.rotation = Quaternion.identity;
    }

    [Command]
    void CmdSetPlayerID(string newID)
    {
        playerID = newID;
    }

    void OnPlayerIDChanged(string newValue)
    {
        playerID = newValue;
        TextMesh tempText = labelHolder.GetComponent<TextMesh>();
        tempText.text = newValue;
    }

    public override void OnStartClient()
    {
        OnPlayerIDChanged(playerID);
    }

    public override void OnStartLocalPlayer()
    {
        string localPlayerID = string.Format("Player " + netId.Value);
        CmdSetPlayerID(localPlayerID);
        OnSendPlayerID(localPlayerID);
    }
}
