using ProtoBuf;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using WSIO.Protobuf;

namespace WSIO.Protocol {

	[ProtocolHandler(1)]
	public class V1 : WSIOProtocolHandler {
		//TODO: cleanup

		public override ProtoSerializedMessage Handle<TPlayer>(Server<TPlayer> server, TPlayer p, out TPlayer changed, ProtocolDefinition def, Stream data) {
			var protoMessage = WSIOProtocolHandler.GetProtoMessage(def.ProtocolVersion, def.MessageType);
			changed = p;

			//TODO: comment it
			//TODO: handle if protoMessage is null

			switch (protoMessage) {
				case Authentication auth: {
					//TODO: make this not awful i guess... maybe with every message or something, don't know how i'd concise it though
					if (!CanDeserialize<Authentication>(data, out auth)) return Fail(p, def, "There was an error deserializing your message.");

					if (auth.Username == null || auth.Password == null) return Fail(p, def, "The username or password was null.");

					// SHA256 the password before we even do anything
					//TODO: implement some kind of golden standard when dealing with authentication to do this
					using (var sha = SHA256.Create())
						auth.Password = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(auth.Password)));

					if (p.Auth.Authenticated) return Fail(p, def, "You are already authenticated!");
					else {
						var playerLookup = server.Lookup(auth);
						if (playerLookup == null || playerLookup == default(TPlayer)) return Fail(p, def, "No player object was found matching the credentials specified.");

						// ok we found a player matching our credentials
						// but what if they're online?
						// kick them if they're online

						//TODO: removing players from server,
						//		removing rooms from server,
						//		suddenly terminating connections,
						//		it's all messy and carefully coddled.
						//		we need to be more robust.

						if (server.IsOnline(playerLookup.Auth, out var terminate)) return Fail(p, def, "You've attempted to log in from another location.");

						//TODO: put this in a function of it's own
						changed = playerLookup;

						changed.Socket = p.Socket;
						changed.Connect = p.Connect;

						changed.Auth.Upgrade(auth.Username, auth.Password);

						//TODO: sending a success is awful
						//TODO: rename `Fail` to `SetSuccess`, and hardcode the protocol version ( since it's 1 ) and make it more widely usable.
						changed.Send(new Success {
							ProtoDefs = new ProtocolDefinition {
								ProtocolVersion = def.ProtocolVersion,
								MessageType = Helper.GetMessageType<Success>(),
							},
							SuccessState = true,
							FailureReason = null
						});
					}
				}
				break;

				case Register reg: {
					if (!CanDeserialize<Register>(data, out reg)) return Fail(p, def, "There was an error deserializing your message.");

					if (reg.Username == null || reg.Password == null) return Fail(p, def, "The username or password was null.");

					//TOFO: don't repeat code this is bad
					// SHA256 the password before we even do anything
					using (var sha = System.Security.Cryptography.SHA256.Create())
						reg.Password = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(reg.Password)));

					if (p.Auth.Authenticated) return Fail(p, def, "You are already authenticated!");
					else {
						//TODO: implement authentication logic
						var playerLookup = server.Lookup(reg);
						if (playerLookup != default(TPlayer) && playerLookup != null) return Fail(p, def, "The server was able to find a player with your username. Choose a different one.");

						p.Auth.Upgrade(reg.Username, reg.Password);

						p.Send(new Success {
							ProtoDefs = new ProtocolDefinition {
								ProtocolVersion = def.ProtocolVersion,
								MessageType = Helper.GetMessageType<Success>(),
							},
							SuccessState = true,
							FailureReason = null
						});
					}
				}
				break;

				case RoomRequest roomRequest: {
					if (!CanDeserialize<RoomRequest>(data, out roomRequest)) return Fail(p, def, "There was an error deserializing your message.");

					var friendlyConnectionStatus = p.Connect.ConnectedToRoom ? "connected" : "disconnected";
					var friendlyConnectionRequestStatus = roomRequest.ConnectionStateRequest ? "connect" : "disconnect";

					if (!p.Auth.Authenticated) return Fail(p, def, "You have not authenticated yet!");
					else if (!p.Connect.ConnectedToRoom != roomRequest.ConnectionStateRequest) return Fail(p, def, $"You cannot {friendlyConnectionRequestStatus} while being {friendlyConnectionStatus}!");
					else {
						// we already checked above if theyre trying to join a room and they're currently connected, or if they're trying to disconnect a room and they're currently disconnected

						if (roomRequest.ConnectionStateRequest) { // if we want to join a room
																  // find instances of room
							var room = server.FindOrCreateRoom(roomRequest.RoomId, roomRequest.RoomType);

							if (room == null) return Fail(p, def, "Unable to find any rooms with the RoomType specified.");

							// ask room if they can join
							var joinable = room.CanJoin(p);

							// done
							if (joinable.CanJoin)
								p.Connect.Upgrade(roomRequest.RoomId, roomRequest.RoomType);

							p.Send(new Success {
								ProtoDefs = new ProtocolDefinition {
									ProtocolVersion = def.ProtocolVersion,
									MessageType = Helper.GetMessageType<Success>(),
								},
								SuccessState = joinable.CanJoin,
								FailureReason = joinable.Reason,
							});

							if (joinable.CanJoin) {
								room.Join(p);
							} else if (room._players.Count < 1)
								server.CloseRoom(room);
						} else {
							var room = server.FindOrCreateRoom(roomRequest.RoomId, roomRequest.RoomType);

							p.Connect.Upgrade(null, null);

							room.Leave(p);

							if (room._players.Count < 1)
								server.CloseRoom(room);
						}
					}
				}
				break;

				case ProtoSerializedMessage message: {
					if (!CanDeserialize<ProtoSerializedMessage>(data, out message)) return Fail(p, def, "There was an error deserializing your message.");

					return !p.Auth.Authenticated ?
						Fail(p, def, "You have not authenticated yet!")
						: !p.Connected ?
							Fail(p, def, "You must first connect to a room!")
							: message;
				}

				default: return null;
			}

			return null;
		}

		private static bool CanDeserialize<T>(Stream source, out T result)
					where T : IProtoMessage {
			try {
				result = Serializer.Deserialize<T>(source);
				return true;
			} catch (ProtoException) {
				result = default(T);
				return false;
			}
		}

		//TODO: complete a todo to make this more useful
		private static ProtoSerializedMessage Fail<TPlayer>(TPlayer p, ProtocolDefinition def, string reason)
			where TPlayer : Player, new() {
			p.Send(new Success {
				ProtoDefs = new ProtocolDefinition {
					ProtocolVersion = def.ProtocolVersion,
					MessageType = Helper.GetMessageType<Success>(),
				},
				SuccessState = false,
				FailureReason = reason,
			});

			return null;
		}
	}
}