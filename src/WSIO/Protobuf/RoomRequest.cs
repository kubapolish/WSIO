using ProtoBuf;

using WSIO.Protocol;

namespace WSIO.Protobuf {

	//TODO: comment
	[ProtoContract]
	[ProtocolVersion(1, 3)]
	public class RoomRequest : IProtoMessage {

		//TODO: is ProtoDefs needed?
		[ProtoMember(1)]
		public ProtocolDefinition ProtoDefs { get; set; }

		[ProtoMember(ProtoMessage.StartOn)]
		public string RoomType { get; set; }

		[ProtoMember(ProtoMessage.StartOn + 1)]
		public string RoomId { get; set; }

		[ProtoMember(ProtoMessage.StartOn + 2)]
		public bool ConnectionStateRequest { get; set; }
	}
}