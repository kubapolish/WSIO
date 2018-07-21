using ProtoBuf;

using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 3)]
	internal class Registration : ProtoMessage, IRegistration {

		[ProtoMember(3)]
		public string Username { get; set; }

		[ProtoMember(4)]
		public string Password { get; set; }

		[ProtoMember(5)]
		public string Email { get; set; }
	}
}