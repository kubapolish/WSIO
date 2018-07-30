using Fleck;

using System;
using System.IO;
using WSIO.Authentication;

namespace WSIO.Messages {

	internal interface IMessageHandler {

		void Handler(IWebSocketConnection ws, Stream data);
	}

	internal abstract class MessageHandler {

		public static void DisconnectPlayerFrom<TPlayer>(RoomManager<TPlayer> rooms, Room<TPlayer> room, TPlayer p)
			where TPlayer : Player, new() {
			room.Disconnect(p);
			if (room._players.Items.Length < 1) {
				room.Deletion();
				rooms.Delete(room);
			}
		}

		public abstract void Handle<TPlayer>(TPlayer ws, Authentication.IAuthModule auther, RoomManager<TPlayer> rooms, Stream data)
			where TPlayer : Player, new();
	}

	internal class V1Handler : MessageHandler {

		public v1.SuccessState GetSuccess(bool state, string reason = null) {
			var instance = new v1.SuccessState().CreateInstance();

			instance.State = state;
			instance.Reason = reason;

			return instance;
		}

		public override async void Handle<TPlayer>(TPlayer ws, Authentication.IAuthModule auther, RoomManager<TPlayer> rooms, Stream data) {
			using(var t = new MemoryStream()) {
				data.CopyTo(t);
				data.Seek(0, SeekOrigin.Begin);
				Console.WriteLine(Convert.ToBase64String( t.ToArray() ));
			}
			if (!ProtoSerializer.Deserialize<SimpleIProtoMessageInheriter>(data, out var res)) {
				await ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your message.")));
				return;
			}

			if (!ProtocolDefinition.FindTypeFor(res, out var type)) {
				await ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to find the type for your message.")));
				return;
			}

			HandlePacket(type, ws, auther, rooms, data);
		}

		internal void HandleReg<TPlayer>(TPlayer ws, Authentication.IAuthModule auther, RoomManager<TPlayer> rooms, IRegistration reg)
			where TPlayer : Player, new() {
			if (reg.Username != null && reg.Password != null && reg.Email != null) {
				// ok we want to register & login

				if (ws.Username == null || ws.Password == null) {

					var res = auther.Register(new Credentials(reg.Username, ws.Socket, new PasswordStruct(reg.Password), reg.Email), out var token);

					if (res.Successful) {
						// they've been registered, let's log them in now

						Login(ws, new Messages.v1.Authentication {
							Username = reg.Username,
							Password = reg.Password,
						});
					} else {

						// send an error message

					}

				} else {

					// send an error message

				}

			} else {

				// send an error message

			}
		}

		private void Login<TPlayer>(TPlayer ws, IAuthentication auth)
			where TPlayer : Player, new() {
			// the player isn't authenticated, so we'll log them in
			ws.SetupBy(new PlayerRequest(auth.Username, auth.Password, ws.Socket));

			ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(true)));
		}

		internal void HandleAuth<TPlayer>(TPlayer ws, Authentication.IAuthModule auther, RoomManager<TPlayer> rooms, IAuthentication auth)
			where TPlayer : Player, new() {
			if (auth.Username != null && auth.Password != null) {
				// ok we want to login

				if (ws.Username == null || ws.Password == null) {

					var res = auther.Login(new Credentials(auth.Username, ws.Socket, new PasswordStruct(auth.Password)), out var token);

					if (res.Successful) {

						Login(ws, auth);

					} else {

						// send an error message

					}

				} else {

					// send an error message

				}

			} else {
				//logout

				if (ws.Username != null && ws.Password != null) {

					// the player is authenticated, so we'll log them out

					ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(true)));

				} else {

					// send an error message

				}
			}
		}

		internal void HandleRoom<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, IRoomRequest req)
			where TPlayer : Player, new() {
			if (req.RoomId != null && req.RoomType != null) {

				// room id & type isn't null, connect them to that room

				var room = rooms.FindOrCreateBy(new RoomRequest(req.RoomId, req.RoomType));

				if (room != null) {

					room.Connect(ws);

					ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(true)));

				} else {

					ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "A room of that room type couldn't be found.")));

				}

			} else {

				// it's null

				if (ws.ConnectedTo != null) {

					// if they're connected somewhere, disconnect them

					var room = ws.ConnectedTo;
					DisconnectPlayerFrom(rooms, ((Room<TPlayer>)room), ws);

					ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(true)));

				} else {

					// send an error

				}

			}
		}

		internal void HandleMessage<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, IMessage msg)
			where TPlayer : Player, new() {
			if(ws.ConnectedTo != null) {

				var room = (Room<TPlayer>)ws.ConnectedTo;
				room.Message(ws, msg);

			} else ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "You're not connected to a room!")));
		}

		internal void HandlePacket<TPlayer>(Type type, TPlayer ws, Authentication.IAuthModule auther, RoomManager<TPlayer> rooms, Stream data)
			where TPlayer : Player, new() {
			switch (Activator.CreateInstance(type)) {
				case IAuthentication auth: {
					if (ProtoSerializer.Deserialize<v1.Authentication>(data, out var __)) {
						auth = __;

						HandleAuth(ws, auther, rooms, auth);
					} else ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your authentication packet.")));
				} break;

				case IRegistration reg: {
					if (ProtoSerializer.Deserialize<v1.Registration>(data, out var __)) {
						reg = __;

						HandleReg(ws, auther, rooms, reg);
					} else ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your registration packet.")));
				} break;

				case IRoomRequest req: {
					if (ProtoSerializer.Deserialize<v1.RoomRequest>(data, out var __)) {
						req = __;

						HandleRoom(ws, rooms, req);
					} else ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your roomrequest packet.")));
				} break;

				case IMessage msg: {
					if (ProtoSerializer.Deserialize<v1.Message>(data, out var __)) {
						msg = __;

						HandleMessage(ws, rooms, msg);
					} else ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your message packet.")));
				} break;

				default: break;
			}
		}
	}
}