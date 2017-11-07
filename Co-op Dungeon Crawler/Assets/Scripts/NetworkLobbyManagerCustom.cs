using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace UnityEngine.Networking
{
    /// <summary>
    ///   <para>This is a specialized NetworkManager that includes a networked lobby.</para>
    /// </summary>
    [AddComponentMenu("Network/NetworkLobbyManagerCustom")]
    public class NetworkLobbyManagerCustom : NetworkManager
    {
        private struct PendingPlayer
        {
            public NetworkConnection conn;

            public GameObject lobbyPlayer;
        }

        [SerializeField]
        private bool m_ShowLobbyGUI = true;

        [SerializeField]
        private int m_MaxPlayers = 4;

        [SerializeField]
        private int m_MaxPlayersPerConnection = 1;

        [SerializeField]
        private int m_MinPlayers;

        [SerializeField]
        private NetworkLobbyPlayer m_LobbyPlayerPrefab;

        [SerializeField]
        private GameObject m_GamePlayerPrefab;

        [SerializeField]
        private string m_LobbyScene = "";

        [SerializeField]
        private string m_PlayScene = "";

        private List<PendingPlayer> m_PendingPlayers = new List<PendingPlayer>();

        /// <summary>
        ///   <para>These slots track players that enter the lobby.</para>
        /// </summary>
        public NetworkLobbyPlayer[] lobbySlots;

        private static LobbyReadyToBeginMessageCustom s_ReadyToBeginMessage = new LobbyReadyToBeginMessageCustom();

        private static IntegerMessage s_SceneLoadedMessage = new IntegerMessage();

        private static LobbyReadyToBeginMessageCustom s_LobbyReadyToBeginMessage = new LobbyReadyToBeginMessageCustom();

        /// <summary>
        ///   <para>This flag enables display of the default lobby UI.</para>
        /// </summary>
        public bool showLobbyGUI
        {
            get
            {
                return this.m_ShowLobbyGUI;
            }
            set
            {
                this.m_ShowLobbyGUI = value;
            }
        }

        /// <summary>
        ///   <para>The maximum number of players allowed in the game.</para>
        /// </summary>
        public int maxPlayers
        {
            get
            {
                return this.m_MaxPlayers;
            }
            set
            {
                this.m_MaxPlayers = value;
            }
        }

        /// <summary>
        ///   <para>The maximum number of players per connection.</para>
        /// </summary>
        public int maxPlayersPerConnection
        {
            get
            {
                return this.m_MaxPlayersPerConnection;
            }
            set
            {
                this.m_MaxPlayersPerConnection = value;
            }
        }

        /// <summary>
        ///   <para>The minimum number of players required to be ready for the game to start.</para>
        /// </summary>
        public int minPlayers
        {
            get
            {
                return this.m_MinPlayers;
            }
            set
            {
                this.m_MinPlayers = value;
            }
        }

        /// <summary>
        ///   <para>This is the prefab of the player to be created in the LobbyScene.</para>
        /// </summary>
        public NetworkLobbyPlayer lobbyPlayerPrefab
        {
            get
            {
                return this.m_LobbyPlayerPrefab;
            }
            set
            {
                this.m_LobbyPlayerPrefab = value;
            }
        }

        /// <summary>
        ///   <para>This is the prefab of the player to be created in the PlayScene.</para>
        /// </summary>
        public GameObject gamePlayerPrefab
        {
            get
            {
                return this.m_GamePlayerPrefab;
            }
            set
            {
                this.m_GamePlayerPrefab = value;
            }
        }

        /// <summary>
        ///   <para>The scene to use for the lobby. This is similar to the offlineScene of the NetworkManager.</para>
        /// </summary>
        public string lobbyScene
        {
            get
            {
                return this.m_LobbyScene;
            }
            set
            {
                this.m_LobbyScene = value;
                base.offlineScene = value;
            }
        }

        /// <summary>
        ///   <para>The scene to use for the playing the game from the lobby. This is similar to the onlineScene of the NetworkManager.</para>
        /// </summary>
        public string playScene
        {
            get
            {
                return this.m_PlayScene;
            }
            set
            {
                this.m_PlayScene = value;
            }
        }

        private void OnValidate()
        {
            if (this.m_MaxPlayers <= 0)
            {
                this.m_MaxPlayers = 1;
            }
            if (this.m_MaxPlayersPerConnection <= 0)
            {
                this.m_MaxPlayersPerConnection = 1;
            }
            if (this.m_MaxPlayersPerConnection > this.maxPlayers)
            {
                this.m_MaxPlayersPerConnection = this.maxPlayers;
            }
            if (this.m_MinPlayers < 0)
            {
                this.m_MinPlayers = 0;
            }
            if (this.m_MinPlayers > this.m_MaxPlayers)
            {
                this.m_MinPlayers = this.m_MaxPlayers;
            }
            if (this.m_LobbyPlayerPrefab != null)
            {
                NetworkIdentity component = this.m_LobbyPlayerPrefab.GetComponent<NetworkIdentity>();
                if (component == null)
                {
                    this.m_LobbyPlayerPrefab = null;
                    Debug.LogWarning("LobbyPlayer prefab must have a NetworkIdentity component.");
                }
            }
    /*        if (this.m_GamePlayerPrefab != null)
            {
                NetworkIdentity component2 = this.m_GamePlayerPrefab.GetComponent<NetworkIdentity>();
                if (component2 == null)
                {
                    this.m_GamePlayerPrefab = null;
                    Debug.LogWarning("GamePlayer prefab must have a NetworkIdentity component.");
                }
            }
     */
        }

        private byte FindSlot()
        {
            byte b = 0;
            byte result;
            while ((int)b < this.maxPlayers)
            {
                if (this.lobbySlots[(int)b] == null)
                {
                    result = b;
                    return result;
                }
                b += 1;
            }
            result = 255;
            return result;
        }

        private void SceneLoadedForPlayer(NetworkConnection conn, GameObject lobbyPlayerGameObject)
        {
            NetworkLobbyPlayer component = lobbyPlayerGameObject.GetComponent<NetworkLobbyPlayer>();
            if (!(component == null))
            {
                string name = SceneManager.GetSceneAt(0).name;
                if (LogFilter.logDebug)
                {
                    Debug.Log(string.Concat(new object[]
                    {
                        "NetworkLobby SceneLoadedForPlayer scene:",
                        name,
                        " ",
                        conn
                    }));
                }
                if (name == this.m_LobbyScene)
                {
                    PendingPlayer item;
                    item.conn = conn;
                    item.lobbyPlayer = lobbyPlayerGameObject;
                    this.m_PendingPlayers.Add(item);
                }
                else
                {
                    short playerControllerId = lobbyPlayerGameObject.GetComponent<NetworkIdentity>().playerControllerId;
                    GameObject gameObject = this.OnLobbyServerCreateGamePlayer(conn, playerControllerId);
                    if (gameObject == null)
                    {
                        Transform startPosition = base.GetStartPosition();
                        if (startPosition != null)
                        {
                            gameObject = Object.Instantiate<GameObject>(this.gamePlayerPrefab, startPosition.position, startPosition.rotation);
                        }
                        else
                        {
                            gameObject = Object.Instantiate<GameObject>(this.gamePlayerPrefab, Vector3.zero, Quaternion.identity);
                        }
                    }
                    if (this.OnLobbyServerSceneLoadedForPlayer(lobbyPlayerGameObject, gameObject))
                    {
                        NetworkServer.ReplacePlayerForConnection(conn, gameObject, playerControllerId);
                    }
                }
            }
        }

        private static int CheckConnectionIsReadyToBegin(NetworkConnection conn)
        {
            int num = 0;
            for (int i = 0; i < conn.playerControllers.Count; i++)
            {
                PlayerController playerController = conn.playerControllers[i];
                if (playerController.IsValid)
                {
                    NetworkLobbyPlayer component = playerController.gameObject.GetComponent<NetworkLobbyPlayer>();
                    if (component.readyToBegin)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        /// <summary>
        ///   <para>CheckReadyToBegin checks all of the players in the lobby to see if their readyToBegin flag is set.</para>
        /// </summary>
        public void CheckReadyToBegin()
        {
            string name = SceneManager.GetSceneAt(0).name;
            if (!(name != this.m_LobbyScene))
            {
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < NetworkServer.connections.Count; i++)
                {
                    NetworkConnection networkConnection = NetworkServer.connections[i];
                    if (networkConnection != null)
                    {
                        num2++;
                        num += CheckConnectionIsReadyToBegin(networkConnection);
                    }
                }
                if (this.m_MinPlayers <= 0 || num >= this.m_MinPlayers)
                {
                    if (num >= num2)
                    {
                        this.m_PendingPlayers.Clear();
                        this.OnLobbyServerPlayersReady();
                    }
                }
            }
        }

        /// <summary>
        ///   <para>Calling this causes the server to switch back to the lobby scene.</para>
        /// </summary>
        public void ServerReturnToLobby()
        {
            if (!NetworkServer.active)
            {
                Debug.Log("ServerReturnToLobby called on client");
            }
            else
            {
                this.ServerChangeScene(this.m_LobbyScene);
            }
        }

        private void CallOnClientEnterLobby()
        {
            this.OnLobbyClientEnter();
            for (int i = 0; i < this.lobbySlots.Length; i++)
            {
                NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[i];
                if (!(networkLobbyPlayer == null))
                {
                    networkLobbyPlayer.readyToBegin = false;
                    networkLobbyPlayer.OnClientEnterLobby();
                }
            }
        }

        private void CallOnClientExitLobby()
        {
            this.OnLobbyClientExit();
            for (int i = 0; i < this.lobbySlots.Length; i++)
            {
                NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[i];
                if (!(networkLobbyPlayer == null))
                {
                    networkLobbyPlayer.OnClientExitLobby();
                }
            }
        }

        /// <summary>
        ///   <para>Sends a message to the server to make the game return to the lobby scene.</para>
        /// </summary>
        /// <returns>
        ///   <para>True if message was sent.</para>
        /// </returns>
        public bool SendReturnToLobby()
        {
            bool result;
            if (this.client == null || !this.client.isConnected)
            {
                result = false;
            }
            else
            {
                EmptyMessage msg = new EmptyMessage();
                this.client.Send(46, msg);
                result = true;
            }
            return result;
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (base.numPlayers > this.maxPlayers)
            {
                if (LogFilter.logWarn)
                {
                    Debug.LogWarning("NetworkLobbyManagerCustom can't accept new connection [" + conn + "], too many players connected.");
                }
                conn.Disconnect();
            }
            else
            {
                string name = SceneManager.GetSceneAt(0).name;
                if (name != this.m_LobbyScene)
                {
                    if (LogFilter.logWarn)
                    {
                        Debug.LogWarning("NetworkLobbyManagerCustom can't accept new connection [" + conn + "], not in lobby and game already in progress.");
                    }
                    conn.Disconnect();
                }
                else
                {
                    base.OnServerConnect(conn);
                    for (int i = 0; i < this.lobbySlots.Length; i++)
                    {
                        if (this.lobbySlots[i])
                        {
                            this.lobbySlots[i].SetDirtyBit(1u);
                        }
                    }
                    this.OnLobbyServerConnect(conn);
                }
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            for (int i = 0; i < this.lobbySlots.Length; i++)
            {
                NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[i];
                if (!(networkLobbyPlayer == null))
                {
                    if (networkLobbyPlayer.connectionToClient == conn)
                    {
                        this.lobbySlots[i] = null;
                        NetworkServer.Destroy(networkLobbyPlayer.gameObject);
                    }
                }
            }
            this.OnLobbyServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            string name = SceneManager.GetSceneAt(0).name;
            if (!(name != this.m_LobbyScene))
            {
                int num = 0;
                for (int i = 0; i < conn.playerControllers.Count; i++)
                {
                    if (conn.playerControllers[i].IsValid)
                    {
                        num++;
                    }
                }
                if (num >= this.maxPlayersPerConnection)
                {
                    if (LogFilter.logWarn)
                    {
                        Debug.LogWarning("NetworkLobbyManagerCustom no more players for this connection.");
                    }
                    EmptyMessage msg = new EmptyMessage();
                    conn.Send(45, msg);
                }
                else
                {
                    byte b = this.FindSlot();
                    if (b == 255)
                    {
                        if (LogFilter.logWarn)
                        {
                            Debug.LogWarning("NetworkLobbyManagerCustom no space for more players");
                        }
                        EmptyMessage msg2 = new EmptyMessage();
                        conn.Send(45, msg2);
                    }
                    else
                    {
                        GameObject gameObject = this.OnLobbyServerCreateLobbyPlayer(conn, playerControllerId);
                        if (gameObject == null)
                        {
                            gameObject = Object.Instantiate<GameObject>(this.lobbyPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
                        }
                        NetworkLobbyPlayer component = gameObject.GetComponent<NetworkLobbyPlayer>();
                        component.slot = b;
                        this.lobbySlots[(int)b] = component;
                        NetworkServer.AddPlayerForConnection(conn, gameObject, playerControllerId);
                    }
                }
            }
        }

        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            short playerControllerId = player.playerControllerId;
            byte slot = player.gameObject.GetComponent<NetworkLobbyPlayer>().slot;
            this.lobbySlots[(int)slot] = null;
            base.OnServerRemovePlayer(conn, player);
            for (int i = 0; i < this.lobbySlots.Length; i++)
            {
                NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[i];
                if (networkLobbyPlayer != null)
                {
                    networkLobbyPlayer.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;
                    s_LobbyReadyToBeginMessage.slotId = networkLobbyPlayer.slot;
                    s_LobbyReadyToBeginMessage.readyState = false;
                    NetworkServer.SendToReady(null, 43, s_LobbyReadyToBeginMessage);
                }
            }
            this.OnLobbyServerPlayerRemoved(conn, playerControllerId);
        }

        public override void ServerChangeScene(string sceneName)
        {
            if (sceneName == this.m_LobbyScene)
            {
                for (int i = 0; i < this.lobbySlots.Length; i++)
                {
                    NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[i];
                    if (!(networkLobbyPlayer == null))
                    {
                        NetworkIdentity component = networkLobbyPlayer.GetComponent<NetworkIdentity>();
                        PlayerController playerController;

                        if (component.connectionToClient.playerControllers.ElementAt(component.playerControllerId) != null)
                        {
                            playerController = component.connectionToClient.playerControllers.ElementAt(component.playerControllerId);
                            NetworkServer.Destroy(playerController.gameObject);
                        }
                        if (NetworkServer.active)
                        {
                            networkLobbyPlayer.GetComponent<NetworkLobbyPlayer>().readyToBegin = false;
                            NetworkServer.ReplacePlayerForConnection(component.connectionToClient, networkLobbyPlayer.gameObject, component.playerControllerId);
                        }
                    }
                }
            }
            base.ServerChangeScene(sceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName != this.m_LobbyScene)
            {
                for (int i = 0; i < this.m_PendingPlayers.Count; i++)
                {
                    PendingPlayer pendingPlayer = this.m_PendingPlayers[i];
                    this.SceneLoadedForPlayer(pendingPlayer.conn, pendingPlayer.lobbyPlayer);
                }
                this.m_PendingPlayers.Clear();
            }
            this.OnLobbyServerSceneChanged(sceneName);
        }

        private void OnServerReadyToBeginMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug)
            {
                Debug.Log("NetworkLobbyManagerCustom OnServerReadyToBeginMessage");
            }
            netMsg.ReadMessage<LobbyReadyToBeginMessageCustom>(s_ReadyToBeginMessage);
            PlayerController playerController;
            if (netMsg.conn.playerControllers.ElementAt((short)s_ReadyToBeginMessage.slotId) == null)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom OnServerReadyToBeginMessage invalid playerControllerId " + s_ReadyToBeginMessage.slotId);
                }
            }
            else
            {
                playerController = netMsg.conn.playerControllers.ElementAt((short)s_ReadyToBeginMessage.slotId);
                NetworkLobbyPlayer component = playerController.gameObject.GetComponent<NetworkLobbyPlayer>();
                component.readyToBegin = s_ReadyToBeginMessage.readyState;
                NetworkServer.SendToReady(null, 43, new LobbyReadyToBeginMessageCustom
                {
                    slotId = component.slot,
                    readyState = s_ReadyToBeginMessage.readyState
                });
                this.CheckReadyToBegin();
            }
        }

        private void OnServerSceneLoadedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug)
            {
                Debug.Log("NetworkLobbyManagerCustom OnSceneLoadedMessage");
            }
            netMsg.ReadMessage<IntegerMessage>(s_SceneLoadedMessage);
            PlayerController playerController;
            if (netMsg.conn.playerControllers.ElementAt((short)s_SceneLoadedMessage.value) == null)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom OnServerSceneLoadedMessage invalid playerControllerId " + s_SceneLoadedMessage.value);
                }
            }
            else
            {
                playerController = netMsg.conn.playerControllers.ElementAt((short)s_SceneLoadedMessage.value);
                this.SceneLoadedForPlayer(netMsg.conn, playerController.gameObject);
            }
        }

        private void OnServerReturnToLobbyMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug)
            {
                Debug.Log("NetworkLobbyManagerCustom OnServerReturnToLobbyMessage");
            }
            this.ServerReturnToLobby();
        }

        public override void OnStartServer()
        {
            if (string.IsNullOrEmpty(this.m_LobbyScene))
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom LobbyScene is empty. Set the LobbyScene in the inspector for the NetworkLobbyMangaer");
                }
            }
            else if (string.IsNullOrEmpty(this.m_PlayScene))
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom PlayScene is empty. Set the PlayScene in the inspector for the NetworkLobbyMangaer");
                }
            }
            else
            {
                if (this.lobbySlots.Length == 0)
                {
                    this.lobbySlots = new NetworkLobbyPlayer[this.maxPlayers];
                }
                NetworkServer.RegisterHandler(43, new NetworkMessageDelegate(this.OnServerReadyToBeginMessage));
                NetworkServer.RegisterHandler(44, new NetworkMessageDelegate(this.OnServerSceneLoadedMessage));
                NetworkServer.RegisterHandler(46, new NetworkMessageDelegate(this.OnServerReturnToLobbyMessage));
                this.OnLobbyStartServer();
            }
        }

        public override void OnStartHost()
        {
            this.OnLobbyStartHost();
        }

        public override void OnStopHost()
        {
            this.OnLobbyStopHost();
        }

        public override void OnStartClient(NetworkClient lobbyClient)
        {
            if (this.lobbySlots.Length == 0)
            {
                this.lobbySlots = new NetworkLobbyPlayer[this.maxPlayers];
            }
            if (this.m_LobbyPlayerPrefab == null || this.m_LobbyPlayerPrefab.gameObject == null)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom no LobbyPlayer prefab is registered. Please add a LobbyPlayer prefab.");
                }
            }
            else
            {
                ClientScene.RegisterPrefab(this.m_LobbyPlayerPrefab.gameObject);
            }
            if (this.m_GamePlayerPrefab == null)
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom no GamePlayer prefab is registered. Please add a GamePlayer prefab.");
                }
            }
            else
            {
                ClientScene.RegisterPrefab(this.m_GamePlayerPrefab);
            }
            lobbyClient.RegisterHandler(43, new NetworkMessageDelegate(this.OnClientReadyToBegin));
            lobbyClient.RegisterHandler(45, new NetworkMessageDelegate(this.OnClientAddPlayerFailedMessage));
            this.OnLobbyStartClient(lobbyClient);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            this.OnLobbyClientConnect(conn);
            this.CallOnClientEnterLobby();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            this.OnLobbyClientDisconnect(conn);
            base.OnClientDisconnect(conn);
        }

        public override void OnStopClient()
        {
            this.OnLobbyStopClient();
            this.CallOnClientExitLobby();
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            string name = SceneManager.GetSceneAt(0).name;
            if (name == this.m_LobbyScene)
            {
                if (this.client.isConnected)
                {
                    this.CallOnClientEnterLobby();
                }
            }
            else
            {
                this.CallOnClientExitLobby();
            }
            base.OnClientSceneChanged(conn);
            this.OnLobbyClientSceneChanged(conn);
        }

        private void OnClientReadyToBegin(NetworkMessage netMsg)
        {
            netMsg.ReadMessage<LobbyReadyToBeginMessageCustom>(s_LobbyReadyToBeginMessage);
            if ((int)s_LobbyReadyToBeginMessage.slotId >= this.lobbySlots.Count<NetworkLobbyPlayer>())
            {
                if (LogFilter.logError)
                {
                    Debug.LogError("NetworkLobbyManagerCustom OnClientReadyToBegin invalid lobby slot " + s_LobbyReadyToBeginMessage.slotId);
                }
            }
            else
            {
                NetworkLobbyPlayer networkLobbyPlayer = this.lobbySlots[(int)s_LobbyReadyToBeginMessage.slotId];
                if (networkLobbyPlayer == null || networkLobbyPlayer.gameObject == null)
                {
                    if (LogFilter.logError)
                    {
                        Debug.LogError("NetworkLobbyManagerCustom OnClientReadyToBegin no player at lobby slot " + s_LobbyReadyToBeginMessage.slotId);
                    }
                }
                else
                {
                    networkLobbyPlayer.readyToBegin = s_LobbyReadyToBeginMessage.readyState;
                    networkLobbyPlayer.OnClientReady(s_LobbyReadyToBeginMessage.readyState);
                }
            }
        }

        private void OnClientAddPlayerFailedMessage(NetworkMessage netMsg)
        {
            if (LogFilter.logDebug)
            {
                Debug.Log("NetworkLobbyManagerCustom Add Player failed.");
            }
            this.OnLobbyClientAddPlayerFailed();
        }

        /// <summary>
        ///   <para>This is called on the host when a host is started.</para>
        /// </summary>
        public virtual void OnLobbyStartHost()
        {
        }

        /// <summary>
        ///   <para>This is called on the host when the host is stopped.</para>
        /// </summary>
        public virtual void OnLobbyStopHost()
        {
        }

        /// <summary>
        ///   <para>This is called on the server when the server is started - including when a host is started.</para>
        /// </summary>
        public virtual void OnLobbyStartServer()
        {
        }

        /// <summary>
        ///   <para>This is called on the server when a new client connects to the server.</para>
        /// </summary>
        /// <param name="conn">The new connection.</param>
        public virtual void OnLobbyServerConnect(NetworkConnection conn)
        {
        }

        /// <summary>
        ///   <para>This is called on the server when a client disconnects.</para>
        /// </summary>
        /// <param name="conn">The connection that disconnected.</param>
        public virtual void OnLobbyServerDisconnect(NetworkConnection conn)
        {
        }

        /// <summary>
        ///   <para>This is called on the server when a networked scene finishes loading.</para>
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public virtual void OnLobbyServerSceneChanged(string sceneName)
        {
        }

        /// <summary>
        ///   <para>This allows customization of the creation of the lobby-player object on the server.</para>
        /// </summary>
        /// <param name="conn">The connection the player object is for.</param>
        /// <param name="playerControllerId">The controllerId of the player.</param>
        /// <returns>
        ///   <para>The new lobby-player object.</para>
        /// </returns>
        public virtual GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        /// <summary>
        ///   <para>This allows customization of the creation of the GamePlayer object on the server.</para>
        /// </summary>
        /// <param name="conn">The connection the player object is for.</param>
        /// <param name="playerControllerId">The controllerId of the player on the connnection.</param>
        /// <returns>
        ///   <para>A new GamePlayer object.</para>
        /// </returns>
        public virtual GameObject OnLobbyServerCreateGamePlayer(NetworkConnection conn, short playerControllerId)
        {
            return null;
        }

        /// <summary>
        ///   <para>This is called on the server when a player is removed.</para>
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="playerControllerId"></param>
        public virtual void OnLobbyServerPlayerRemoved(NetworkConnection conn, short playerControllerId)
        {
        }

        /// <summary>
        ///   <para>This is called on the server when it is told that a client has finished switching from the lobby scene to a game player scene.</para>
        /// </summary>
        /// <param name="lobbyPlayer">The lobby player object.</param>
        /// <param name="gamePlayer">The game player object.</param>
        /// <returns>
        ///   <para>False to not allow this player to replace the lobby player.</para>
        /// </returns>
        public virtual bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            return true;
        }

        /// <summary>
        ///   <para>This is called on the server when all the players in the lobby are ready.</para>
        /// </summary>
        public virtual void OnLobbyServerPlayersReady()
        {
            this.ServerChangeScene(this.m_PlayScene);
        }

        /// <summary>
        ///   <para>This is a hook to allow custom behaviour when the game client enters the lobby.</para>
        /// </summary>
        public virtual void OnLobbyClientEnter()
        {
        }

        /// <summary>
        ///   <para>This is a hook to allow custom behaviour when the game client exits the lobby.</para>
        /// </summary>
        public virtual void OnLobbyClientExit()
        {
        }

        /// <summary>
        ///   <para>This is called on the client when it connects to server.</para>
        /// </summary>
        /// <param name="conn">The connection that connected.</param>
        public virtual void OnLobbyClientConnect(NetworkConnection conn)
        {
        }

        /// <summary>
        ///   <para>This is called on the client when disconnected from a server.</para>
        /// </summary>
        /// <param name="conn">The connection that disconnected.</param>
        public virtual void OnLobbyClientDisconnect(NetworkConnection conn)
        {
        }

        /// <summary>
        ///   <para>This is called on the client when a client is started.</para>
        /// </summary>
        /// <param name="lobbyClient"></param>
        public virtual void OnLobbyStartClient(NetworkClient lobbyClient)
        {
        }

        /// <summary>
        ///   <para>This is called on the client when the client stops.</para>
        /// </summary>
        public virtual void OnLobbyStopClient()
        {
        }

        /// <summary>
        ///   <para>This is called on the client when the client is finished loading a new networked scene.</para>
        /// </summary>
        /// <param name="conn"></param>
        public virtual void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
        }

        /// <summary>
        ///   <para>Called on the client when adding a player to the lobby fails.</para>
        /// </summary>
        public virtual void OnLobbyClientAddPlayerFailed()
        {
        }

        private void OnGUI()
        {
            if (this.showLobbyGUI)
            {
                string name = SceneManager.GetSceneAt(0).name;
                if (!(name != this.m_LobbyScene))
                {
                    Rect rect = new Rect(90f, 180f, 500f, 150f);
                    GUI.Box(rect, "Players:");
                    if (NetworkClient.active)
                    {
                        Rect rect2 = new Rect(100f, 300f, 120f, 20f);
                        if (GUI.Button(rect2, "Add Player"))
                        {
                            this.TryToAddPlayer();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   <para>This is used on clients to attempt to add a player to the game.</para>
        /// </summary>
        public void TryToAddPlayer()
        {
            if (NetworkClient.active)
            {
                short num = -1;
                List<PlayerController> playerControllers = NetworkClient.allClients[0].connection.playerControllers;
                if (playerControllers.Count < this.maxPlayers)
                {
                    num = (short)playerControllers.Count;
                }
                else
                {
                    short num2 = 0;
                    while ((int)num2 < this.maxPlayers)
                    {
                        if (!playerControllers[(int)num2].IsValid)
                        {
                            num = num2;
                            break;
                        }
                        num2 += 1;
                    }
                }
                if (LogFilter.logDebug)
                {
                    Debug.Log(string.Concat(new object[]
                    {
                        "NetworkLobbyManagerCustom TryToAddPlayer controllerId ",
                        num,
                        " ready:",
                        ClientScene.ready
                    }));
                }
                if (num == -1)
                {
                    if (LogFilter.logDebug)
                    {
                        Debug.Log("NetworkLobbyManagerCustom No Space!");
                    }
                }
                else if (ClientScene.ready)
                {
                    ClientScene.AddPlayer(num);
                }
                else
                {
                    ClientScene.AddPlayer(NetworkClient.allClients[0].connection, num);
                }
            }
            else if (LogFilter.logDebug)
            {
                Debug.Log("NetworkLobbyManagerCustom NetworkClient not active!");
            }
        }
    }
}
