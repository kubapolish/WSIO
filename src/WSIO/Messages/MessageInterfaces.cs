using ProtoBuf;
using System.Collections.Generic;
using WSIO.Authentication;
using WSIO.Messages.v1;

namespace WSIO.Messages {

	internal interface IProtoMessage {
		ProtocolDefinition ProtoDefs { get; set; }
	}

	internal interface IAuthentication : IProtoMessage {
		string Username { get; set; }
		string Password { get; set; }
	}

	internal interface IRegistration : IProtoMessage {
		string Username { get; set; }
		string Password { get; set; }
		string Email { get; set; }
	}

	internal interface IRoomRequest : IProtoMessage {
		string RoomType { get; set; }
		string RoomId { get; set; }
	}

	internal interface ISuccessState : IProtoMessage {
		bool State { get; set; }
		string Reason { get; set; }
	}

	internal interface IMessage : IProtoMessage {
		string Type { get; set; }
		List<MessageItem> Items { get; set; }
	}

	[ProtoContract]
	internal interface IMessageItem {
		[ProtoMember(1)]
		uint Type { get; set; }
		object Value();
		[ProtoMember(2)]
		List<double> DoubleValues { get; set; }
		[ProtoMember(3)]
		List<float> FloatValues { get; set; }
		[ProtoMember(4)]
		List<int> IntValues { get; set; }
		[ProtoMember(5)]
		List<long> LongValues { get; set; }
		[ProtoMember(6)]
		List<uint> UintValues { get; set; }
		[ProtoMember(7)]
		List<ulong> UlongValues { get; set; }
		[ProtoMember(8)]
		List<bool> BoolValues { get; set; }
		[ProtoMember(9)]
		List<string> StringValues { get; set; }
		[ProtoMember(10)]
		List<byte[]> ByteArrayValues { get; set; }
		[ProtoMember(11)]
		List<MessageItem> InnerMessageItems { get; set; }
	}
}