using ProtoBuf;

using WSIO.Protocol;

namespace WSIO.Protobuf {
	//TODO: is the todo below to-done?
	//TODO: [ProtoInclude(x, typeof())] https://github.com/mgravell/protobuf-net#inheritance
	//TODO: is IProtoMessage still needed?

	[ProtoContract]
	[ProtocolVersion(1, 0)]
	public interface IProtoMessage {

		[ProtoMember(1, IsRequired = true)]
		ProtocolDefinition ProtoDefs { get; set; }
	}

	[ProtoContract]
	[ProtocolVersion(1, 0)]
	public class ProtoMessage : IProtoMessage {
		public const int StartOn = 2;

		[ProtoMember(1, IsRequired = true)]
		public ProtocolDefinition ProtoDefs { get; set; }
	}
}