using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using System.Net;
using System.Net.Sockets;

public class LobbyUI : MonoBehaviour {

    public NetworkManager manager;

    private string networkAddress;
    private string currentCanvas;

    [SerializeField]
    private CanvasGroup[] canvasGroups;
    [SerializeField]
    private InputField inputAddress;

    [SerializeField]
    private Text networkAddressText;
    [SerializeField]
    private Text networkServerPort;
    [SerializeField]
    private Text clientServerPort;

    [SerializeField]
    private Text ipAddressText;

    private void FixedUpdate()
    {
        if (manager == null)
        {
            manager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkControl>();
        }
        //If the client isn't connected, there is no network server currently active nor any matchmaking
        if (!manager.IsClientConnected() && !NetworkServer.active) //&& manager.matchMaker == null) -- add back in when matchmaker is done
        {
            //If the client doesn't exist, theres no connection or the ID doesn't exist, then we can start a game safely.
            if (manager.client == null || manager.client.connection == null || manager.client.connection.connectionId == -1)
            {
                if (currentCanvas != "StartGame")
                {
                    SwitchCanvasGroup("StartGame");
                    currentCanvas = "StartGame";
                }
            }
            else
            {
                if (currentCanvas != "Connecting")
                {
                    SwitchCanvasGroup("Connecting");
                    currentCanvas = "Connecting";

                    networkAddressText.text = "Connecting to " + manager.networkAddress + ":" + manager.networkPort + "...";
                }
            }
        }
        else
        {
            if (currentCanvas != "Hosting")
            {
                SwitchCanvasGroup("Hosting");
                currentCanvas = "Hosting";

                networkServerPort.text = "Server: port=" + manager.networkPort;

                ipAddressText.text = GetLocalIPAddress();


                //Indicates if web sockets are being used.
                if (manager.useWebSockets)
                {
                    networkServerPort.text += " (Using WebSockets)";
                }

                if (manager.IsClientConnected())
                {
                    clientServerPort.text = "Client: Address= " + manager.networkAddress + " port= " + manager.networkPort;
                }
            }
        }
    }

    private string GetLocalIPAddress()
    {
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return null;
    }

    public void StartLobby(string startType)
    {
        if (startType == "Server")
        {
            //Server Only (not needed usually)
            manager.StartServer();
        }
        else if (startType == "Host")
        {
            //LAN Host
            manager.StartHost();
        }
        else if (startType == "Client")
        {
            //LAN Client
            manager.StartClient();
            //Do some checks here
            manager.networkAddress = inputAddress.text;
            //
        }
    }

    public void CancelConnect()
    {
        if (manager.client != null || manager.client.connection != null || manager.client.connection.connectionId != -1)
        {
            manager.StopClient();
        }
    }

    public void StopLobby(string endType)
    {
        if (manager.IsClientConnected())
        {
            if (endType == "Host")
            {
                manager.StopHost();
            }
            if (endType == "Server")
            {
                manager.StopServer();
            }
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }

    private void SwitchCanvasGroup(string canvasGroupName)
    {
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            if (canvasGroups[i].name == canvasGroupName)
            {
                canvasGroups[i].interactable = true;
                canvasGroups[i].alpha = 1;
                canvasGroups[i].blocksRaycasts = true;
            }
            else
            {
                canvasGroups[i].interactable = false;
                canvasGroups[i].alpha = 0;
                canvasGroups[i].blocksRaycasts = false;
            }
        }
    }
}
