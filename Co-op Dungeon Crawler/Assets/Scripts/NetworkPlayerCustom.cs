using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayerCustom : NetworkBehaviour {
    /// <summary>
    /// Class to hold the players ID
    /// </summary>
    [SyncVar(hook = "OnPlayerIDChanged")]
    public string playerID;
    private Transform labelHolder;


    private void Awake()
    {
        //Find the label on the player. This is neccessary, so we know who is who.
        //The hat was for fun but this should be part of all players
        labelHolder = transform.Find("PlayerLabel");
    }

    private void LateUpdate()
    {
        //Keep the label fixed in one direction, since the camera doesn't move
        labelHolder.rotation = Quaternion.identity;
    }

    //Command to set a new ID
    [Command]
    void CmdSetPlayerID(string newID)
    {
        playerID = newID;
    }

    //When the ID changes, change the text and the current held ID value
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
        //When the local player starts, give them an ID and send it to the server.
        string localPlayerID = string.Format("Player " + netId.Value);
        CmdSetPlayerID(localPlayerID);
    }
}
