using System;

namespace WSIO.Attributes {

	public sealed class MessageVersionAttribute : Attribute {

		public MessageVersionAttribute(uint protocolVersion, uint messageType) {
			this.ProtocolVersion = protocolVersion;
			this.MessageType = messageType;
		}

		public uint ProtocolVersion { get; }
		public uint MessageType { get; }
	}
}