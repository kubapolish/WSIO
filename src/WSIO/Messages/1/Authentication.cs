using ProtoBuf;

using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 2)]
	internal class Authentication : IProtoMessage, IAuthentication {

		[ProtoMember(1)]
		public ProtoMessage ProtoDefs { get; set; }

		[ProtoMember(2)]
		public string Username { get; set; }

		[ProtoMember(3)]
		public string Password { get; set; }
	}
}