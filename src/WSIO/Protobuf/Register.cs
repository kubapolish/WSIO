using ProtoBuf;

using WSIO.Protocol;

namespace WSIO.Protobuf {

	//TODO: comment
	[ProtoContract]
	[ProtocolVersion(1, 5)]
	internal class Register : IProtoMessage, IUserAuth {

		//TODO: is ProtoDefs needed?
		[ProtoMember(1)]
		public ProtocolDefinition ProtoDefs { get; set; }

		[ProtoMember(ProtoMessage.StartOn)]
		public string Username { get; set; }

		[ProtoMember(ProtoMessage.StartOn + 1)]
		public string Password { get; set; }
	}
}