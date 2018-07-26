using ProtoBuf;

using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 4)]
	internal class RoomRequest : IProtoMessage, IRoomRequest {

		[ProtoMember(1)]
		public ProtocolDefinition ProtoDefs { get; set; }

		[ProtoMember(2)]
		public string RoomType { get; set; }

		[ProtoMember(3)]
		public string RoomId { get; set; }
	}
}