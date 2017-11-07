using System;

namespace UnityEngine.Networking.NetworkSystem
{
	public class LobbyReadyToBeginMessageCustom : MessageBase
	{
		public byte slotId;

		public bool readyState;

		public override void Deserialize(NetworkReader reader)
		{
			this.slotId = reader.ReadByte();
			this.readyState = reader.ReadBoolean();
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(this.slotId);
			writer.Write(this.readyState);
		}
	}
}
