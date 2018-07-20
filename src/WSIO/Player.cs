using Fleck;

using MongoDB.Bson.Serialization.Attributes;

using ProtoBuf;

using System.IO;

using WSIO.Protobuf;

namespace WSIO {

	/// <summary>
	/// A player. Inherit it and add your own methods and properties
	/// </summary>
	public class Player {

		public Player() {
		}

		[BsonId]
		public PlayerAuth Auth { get; private set; }

		public bool Connected => this.Connect.ConnectedToRoom;

		[BsonElement]
		public string Ip => $"{this.Socket.ConnectionInfo.ClientIpAddress}:{this.Socket.ConnectionInfo.ClientPort}";

		[BsonElement]
		public string LastOrCurrentRoomId => this.Connect.RoomId;

		[BsonElement]
		public string LastOrCurrentRoomType => this.Connect.RoomType;

		public string Username => this.Auth.Username;

		internal ConnectionStatus Connect { get; set; }

		internal IWebSocketConnection Socket { get; set; }

		//	internal Server<TPlayer> _server { get; set; }

		//internal Server<Player> ServerRef { get; set; }

		/// <summary>
		/// Kicks the player ( given a reason )
		/// </summary>
		/// <param name="reason">The reason that they were kicked</param>
		public void Kick(string reason) {
			//TODO: send a protobuf message that they were kicked

			/*this.Send<KickMessage>(new KickMessage {
				ProtoDefs = new ProtocolDefinition {
					ProtocolVersion = 1,
					MessageType = Protocol.Helper.GetMessageType<KickMessage>(),
				},
				KickReason = reason
			});*/
			this.Terminate();
		}

		/// <summary>
		/// Send the player a message.
		/// Equivalent to Send(Message.Create(type, args));
		/// </summary>
		/// <param name="type">The type of message</param>
		/// <param name="args">The arguments of the message</param>
		public void Send(string type, params object[] args)
			=> this.Send<ProtoSerializedMessage>(ProtoSerializedMessage.Create(type, args));

		/// <summary>
		/// Send the player a message.
		/// </summary>
		/// <param name="msg">The message.</param>
		public void Send(Message msg)
			// specify the generic
			// function we're calling is below
			// it does actual protobuf work
			=> this.Send<ProtoSerializedMessage>(msg._message);

		/// <summary>
		/// Closes the underlying socket for the player.
		/// Terminates their connection
		/// Much less friendly then a kick
		/// </summary>
		public void Terminate() {
			//TODO: get a handle on the server that made us
			this.Socket.Close();
		}

		/// <summary>
		/// Serializes and sends a protobuf message
		/// </summary>
		internal void Send<T>(T msg)
			where T : IProtoMessage {
			using (var ms = new MemoryStream()) {
				Serializer.Serialize<T>(ms, msg);

				//TODO: figure out if this is necessary ( probably not )
				ms.Seek(0, SeekOrigin.Begin);
				this.Socket.Send(ms.ToArray());
			}
		}

		/// <summary>
		/// When creating players, we call this since `new()` is the only available generic constraint for constructors.
		/// </summary>
		/// <param name="socket">The socket</param>
		internal void Setup(IWebSocketConnection socket) {
			this.Socket = socket;

			//TODO: figure out if we should set auth and connect by default, or wait for a null reference exception from it.
			this.Auth = new PlayerAuth();
			this.Connect = new ConnectionStatus();
		}
	}
}