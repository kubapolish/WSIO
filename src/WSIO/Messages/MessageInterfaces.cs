using System.Collections.Generic;

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
		List<IMessageItem> Items { get; set; }
	}

	internal interface IMessageItem {
		uint Type { get; set; }
		object Value();
		List<double> DoubleValues { get; set; }
		List<float> FloatValues { get; set; }
		List<int> IntValues { get; set; }
		List<long> LongValues { get; set; }
		List<uint> UintValues { get; set; }
		List<ulong> UlongValues { get; set; }
		List<bool> BoolValues { get; set; }
		List<string> StringValues { get; set; }
		List<byte[]> ByteArrayValues { get; set; }
		List<IMessageItem> InnerMessageItems { get; set; }
	}
}