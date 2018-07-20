using ProtoBuf;

using WSIO.Protocol;

namespace WSIO.Protobuf {

	//TODO: comment
	[ProtoContract]
	[ProtocolVersion(1, 0)]
	public class ProtocolDefinition {

		[ProtoMember(1, IsRequired = true)]
		public uint ProtocolVersion { get; set; }

		[ProtoMember(2, IsRequired = true)]
		public uint MessageType { get; set; }

		//TODO: use an enum
	}
}