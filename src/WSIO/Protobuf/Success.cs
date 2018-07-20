using ProtoBuf;

using WSIO.Protocol;

namespace WSIO.Protobuf {

	//TODO: comment
	[ProtoContract]
	[ProtocolVersion(1, 2)]
	internal class Success : IProtoMessage {

		//TODO: is ProtoDefs needed?
		[ProtoMember(1)]
		public ProtocolDefinition ProtoDefs { get; set; }

		[ProtoMember(ProtoMessage.StartOn)]
		public bool SuccessState { get; set; }

		[ProtoMember(ProtoMessage.StartOn + 1, IsRequired = false)]
		public string FailureReason { get; set; }
	}
}