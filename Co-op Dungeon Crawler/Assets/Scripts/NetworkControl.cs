using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;

public class NetworkControl : NetworkLobbyManager
{
    /// <summary>
    /// Use this to override anything that exists within NetworkLobbyManager
    /// </summary>
    /// <param name="sceneName"></param>
    /// 

    public static NetworkControl instance = null;
    private List<Transform> playerList;
    private GameObject[] players;

    private void Awake()
    {
        serverBindAddress = GetLocalIPAddress();

        //Means there can only be one of this script at any time;
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

 /*     playerList = new List<Transform>();
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            playerList.Add(players[i].transform);
        }
 */
    }

    public override void ServerChangeScene(string sceneName)
    {
        base.ServerChangeScene(sceneName);

        //SceneManager.LoadScene(sceneName);

        Invoke("SpawnObjects", 0.5f);
    }

    public void SpawnObjects()
    {
        NetworkServer.SpawnObjects();
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
            NetworkLobbyPlayer networkLobbyPlayer = lobbySlots[i];
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

    private string GetLocalIPAddress()
    {
        string ipAddress = "";

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip.ToString();
            }
        }

        return ipAddress;
    }

    private void OnPlayerConnected(NetworkPlayer player)
    {

        playerList.Clear();
    }
    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        
    }
}
