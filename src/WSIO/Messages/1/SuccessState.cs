using ProtoBuf;

using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 1)]
	internal class SuccessState : ProtoMessage, ISuccessState {

		[ProtoMember(3)]
		public bool State { get; set; }

		[ProtoMember(4)]
		public string Reason { get; set; }
	}
}