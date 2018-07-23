namespace WSIO.Messages {

	internal interface IProtoMessage {
		ProtoMessage ProtoDefs { get; set; }
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
}