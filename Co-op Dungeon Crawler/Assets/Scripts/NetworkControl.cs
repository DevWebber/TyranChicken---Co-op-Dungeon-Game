using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkControl : NetworkLobbyManager
{
    /// <summary>
    /// Use this to override anything that exists within NetworkLobbyManager
    /// </summary>
    /// <param name="sceneName"></param>
    /// 

    public override void ServerChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public override void OnLobbyClientDisconnect(NetworkConnection conn)
    {
        Destroy(conn.playerControllers[conn.hostId].gameObject);
        Debug.Log("destroyed");
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        for (int i = 0; i < lobbySlots.Length; i++)
        {
            UnityEngine.Networking.NetworkLobbyPlayer networkLobbyPlayer = lobbySlots[i];
            if (!(networkLobbyPlayer == null))
            {
                if (networkLobbyPlayer.connectionToClient == conn)
                {
                    lobbySlots[i] = null;
                    NetworkServer.Destroy(networkLobbyPlayer.gameObject);
                }
            }
        }
        OnLobbyServerDisconnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);

    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
