using System;
using UnityEngine.Events;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

namespace UnityEngine.Networking
{
	/// <summary>
	///   <para>This component works in conjunction with the NetworkLobbyManager to make up the multiplayer lobby system.</para>
	/// </summary>
	[AddComponentMenu("Network/NetworkLobbyPlayerCustom"), DisallowMultipleComponent]
	public class NetworkLobbyPlayerCustom : NetworkBehaviour
	{
		/// <summary>
		///   <para>This flag controls whether the default UI is shown for the lobby player.</para>
		/// </summary>
		[SerializeField]
		public bool ShowLobbyGUI = true;

		private byte m_Slot;

		private bool m_ReadyToBegin;

		/// <summary>
		///   <para>The slot within the lobby that this player inhabits.</para>
		/// </summary>
		public byte slot
		{
			get
			{
				return this.m_Slot;
			}
			set
			{
				this.m_Slot = value;
			}
		}

		/// <summary>
		///   <para>This is a flag that control whether this player is ready for the game to begin.</para>
		/// </summary>
		public bool readyToBegin
		{
			get
			{
				return this.m_ReadyToBegin;
			}
			set
			{
				this.m_ReadyToBegin = value;
			}
		}

		private void Start()
		{
            Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnEnable()
		{
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
		}

		private void OnDisable()
		{
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        }

		public override void OnStartClient()
		{
			NetworkLobbyManagerCustom networkLobbyManager = NetworkManager.singleton as NetworkLobbyManagerCustom;
			if (networkLobbyManager)
			{
                networkLobbyManager.lobbySlots[m_Slot] = this;
				this.m_ReadyToBegin = false;
				this.OnClientEnterLobby();
			}
			else
			{
				Debug.LogError("LobbyPlayer could not find a NetworkLobbyManager. The LobbyPlayer requires a NetworkLobbyManager object to function. Make sure that there is one in the scene.");
			}
		}

		/// <summary>
		///   <para>This is used on clients to tell the server that this player is ready for the game to begin.</para>
		/// </summary>
		public void SendReadyToBeginMessage()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkLobbyPlayerCustom SendReadyToBeginMessage");
			}
			NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;
			if (networkLobbyManager)
			{
                LobbyReadyToBeginMessageCustom lobbyReadyToBeginMessage = new LobbyReadyToBeginMessageCustom();
				lobbyReadyToBeginMessage.slotId = (byte)base.playerControllerId;
				lobbyReadyToBeginMessage.readyState = true;
				networkLobbyManager.client.Send(43, lobbyReadyToBeginMessage);
			}
		}

		/// <summary>
		///   <para>This is used on clients to tell the server that this player is not ready for the game to begin.</para>
		/// </summary>
		public void SendNotReadyToBeginMessage()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkLobbyPlayerCustom SendReadyToBeginMessage");
			}
			NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;
			if (networkLobbyManager)
			{
				LobbyReadyToBeginMessageCustom lobbyReadyToBeginMessage = new LobbyReadyToBeginMessageCustom();
				lobbyReadyToBeginMessage.slotId = (byte)base.playerControllerId;
				lobbyReadyToBeginMessage.readyState = false;
				networkLobbyManager.client.Send(43, lobbyReadyToBeginMessage);
			}
		}

		/// <summary>
		///   <para>This is used on clients to tell the server that the client has switched from the lobby to the GameScene and is ready to play.</para>
		/// </summary>
		public void SendSceneLoadedMessage()
		{
			if (LogFilter.logDebug)
			{
				Debug.Log("NetworkLobbyPlayerCustom SendSceneLoadedMessage");
			}
			NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;
			if (networkLobbyManager)
			{
				IntegerMessage msg = new IntegerMessage((int)base.playerControllerId);
				networkLobbyManager.client.Send(44, msg);
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;
			if (networkLobbyManager)
			{
                string name = scene.name;
				if (name == networkLobbyManager.lobbyScene)
				{
					return;
				}
			}
			if (base.isLocalPlayer)
			{
				this.SendSceneLoadedMessage();
			}
		}

		/// <summary>
		///   <para>This removes this player from the lobby.</para>
		/// </summary>
		public void RemovePlayer()
		{
			if (base.isLocalPlayer && !this.m_ReadyToBegin)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("NetworkLobbyPlayerCustom RemovePlayer");
				}
				ClientScene.RemovePlayer(base.GetComponent<NetworkIdentity>().playerControllerId);
			}
		}

		/// <summary>
		///   <para>This is a hook that is invoked on all player objects when entering the lobby.</para>
		/// </summary>
		public virtual void OnClientEnterLobby()
		{
		}

		/// <summary>
		///   <para>This is a hook that is invoked on all player objects when exiting the lobby.</para>
		/// </summary>
		public virtual void OnClientExitLobby()
		{
		}

		/// <summary>
		///   <para>This is a hook that is invoked on clients when a LobbyPlayer switches between ready or not ready.</para>
		/// </summary>
		/// <param name="readyState">Whether the player is ready or not.</param>
		public virtual void OnClientReady(bool readyState)
		{
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			writer.WritePackedUInt32(1u);
			writer.Write(this.m_Slot);
			writer.Write(this.m_ReadyToBegin);
			return true;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (reader.ReadPackedUInt32() != 0u)
			{
				this.m_Slot = reader.ReadByte();
				this.m_ReadyToBegin = reader.ReadBoolean();
			}
		}

		private void OnGUI()
		{
			if (this.ShowLobbyGUI)
			{
				NetworkLobbyManager networkLobbyManager = NetworkManager.singleton as NetworkLobbyManager;
				if (networkLobbyManager)
				{
					if (!networkLobbyManager.showLobbyGUI)
					{
						return;
					}
                    string name = SceneManager.GetSceneAt(0).name;
					if (name != networkLobbyManager.lobbyScene)
					{
						return;
					}
				}
				Rect rect = new Rect((float)(100 + this.m_Slot * 100), 200f, 90f, 20f);
				if (base.isLocalPlayer)
				{
					string text;
					if (this.m_ReadyToBegin)
					{
						text = "(Ready)";
					}
					else
					{
						text = "(Not Ready)";
					}
					GUI.Label(rect, text);
					if (this.m_ReadyToBegin)
					{
                        rect.y += 25f;
						if (GUI.Button(rect, "STOP"))
						{
							this.SendNotReadyToBeginMessage();
						}
					}
					else
					{
                        rect.y += 25f;
						if (GUI.Button(rect, "START"))
						{
							this.SendReadyToBeginMessage();
						}
                        rect.y += 25f;
						if (GUI.Button(rect, "Remove"))
						{
							ClientScene.RemovePlayer(base.GetComponent<NetworkIdentity>().playerControllerId);
						}
					}
				}
				else
				{
					GUI.Label(rect, "Player [" + base.netId + "]");
                    rect.y += 25f;
					GUI.Label(rect, "Ready [" + this.m_ReadyToBegin + "]");
				}
			}
		}

        public static implicit operator NetworkLobbyPlayer(NetworkLobbyPlayerCustom v)
        {
            throw new NotImplementedException();
        }
    }
}
