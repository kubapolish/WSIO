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

		public override async void Handle<TPlayer>(TPlayer ws, RoomManager<TPlayer> rooms, Stream data) {
			if (!ProtoSerializer.Deserialize<ProtoMessage>(data, out var res)) {
				await ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your message.")));
				return;
			}

			if (!ProtoMessage.FindTypeFor(res, out var type)) {
				await ws.Socket.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to find the type for your message.")));
				return;
			}

			switch (Activator.CreateInstance(type)) {
				case IAuthentication auth: {
					if (auth.Username != null && auth.Password != null) {
						// ok we want to login

						if (ws.Username == null || ws.Password == null) {

							// the player isn't authenticated, so we'll log them in

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
				} break;

				case IRegistration reg: {
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
				} break;

				case IRoomRequest req: {
					if (req.RoomId != null && req.RoomType != null) {

						// room id & type isn't null, connect them to that room

						var room = rooms.FindOrCreateBy(new RoomRequest(req.RoomId, req.RoomType));
						room.Connect(ws);

					} else {

						// it's null

						if (ws.ConnectedTo != null) {

							// if they're connected somewhere, disconnect them

							var room = ws.ConnectedTo;
							((Room<TPlayer>)room).Disconnect(ws);

						} else {

							// send an error

						}

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