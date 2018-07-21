using ProtoBuf;
using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 2)]
	internal class Authentication : ProtoMessage {
		[ProtoMember(3)]
		public string Username { get; set; }

		[ProtoMember(4)]
		public string Password { get; set; }
	}
}