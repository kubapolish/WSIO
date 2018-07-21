using Fleck;

using System;
using System.IO;

namespace WSIO.Messages {

	internal interface IMessageHandler {

		void Handler(IWebSocketConnection ws, Stream data);
	}

	internal abstract class MessageHandler {

		public abstract void Handle<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, Stream data)
			where TPlayer : Player, new();
	}

	internal class V1Handler : MessageHandler {

		public v1.SuccessState GetSuccess(bool state, string reason = null) {
			var instance = new v1.SuccessState().CreateInstance();

			instance.State = state;
			instance.Reason = reason;

			return instance;
		}

		private void DisconnectPlayerFrom<TPlayer>(RoomManager<TPlayer> rooms, Room<TPlayer> room, TPlayer p)
			where TPlayer : Player, new() {
			room.Disconnect(p);
			if (room._players.Items.Length < 1)
				room.Deletion();
			rooms.Delete(room);
		}

		public override async void Handle<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, Stream data) {
			if (!ProtoSerializer.Deserialize<ProtoMessage>(data, out var res)) {
				await ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your message.")));
				return;
			}

			if (!ProtoMessage.FindTypeFor(res, out var type)) {
				await ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to find the type for your message.")));
				return;
			}

			HandlePacket(type, ws, rooms, data);
		}

		internal void HandleReg<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, IRegistration reg)
			where TPlayer : Player, new() {
			if (reg.Username != null && reg.Password != null && reg.Email != null) {
				// ok we want to register & login

				if (ws.Username == null || ws.Password == null) {

					// the player isn't authenticated, so we'll register & log them in

				} else {

					// send an error message

				}

			} else {

				// send an error message

			}
		}

		internal void HandleAuth<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, IAuthentication auth)
			where TPlayer : Player, new() {
			if (auth.Username != null && auth.Password != null) {
				// ok we want to login

				if (ws.Username == null || ws.Password == null) {

					// the player isn't authenticated, so we'll log them in
					ws.SetupBy(new PlayerRequest(auth.Username, auth.Password, ws.Socket));

				} else {

					// send an error message

				}

			} else {
				//logout

				if (ws.Username != null && ws.Password != null) {

					// the player is authenticated, so we'll log them out

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
				room.Connect(ws);

			} else {

				// it's null

				if (ws.ConnectedTo != null) {

					// if they're connected somewhere, disconnect them

					var room = ws.ConnectedTo;
					DisconnectPlayerFrom(rooms, ((Room<TPlayer>)room), ws);

				} else {

					// send an error

				}

			}
		}

		internal void HandlePacket<TPlayer>(Type type, TPlayer ws, RoomManager<TPlayer> rooms, Stream data)
			where TPlayer : Player, new() {
			switch (Activator.CreateInstance(type)) {
				case IAuthentication auth: {
					if (ProtoSerializer.Deserialize<v1.Authentication>(data, out var __)) {
						auth = __;

						HandleAuth(ws, rooms, auth);
					}
				} break;

				case IRegistration reg: {
					if (ProtoSerializer.Deserialize<v1.Registration>(data, out var __)) {
						reg = __;

						HandleReg(ws, rooms, reg);
					}
				} break;

				case IRoomRequest req: {
					if (ProtoSerializer.Deserialize<v1.RoomRequest>(data, out var __)) {
						req = __;

						HandleRoom(ws, rooms, req);
					}
				} break;

				/*
				//case message:
				case ISuccessState suc: {

				} break;
				*/

				default: break;
			}
		}
	}
}