using Fleck;

using ProtoBuf;

using System;
using System.Collections.Generic;

using TimedQuery;

using WSIO.Protobuf;
using WSIO.Protocol;

namespace WSIO {
	// exclamation marks makes everything better.
	// just notice the difference:
	//
	// Hosts your game.
	//
	// to
	//
	// Hosts your game!
	//
	// very stark & important difference
	// this is an important modification and deserves 10 lines of comments. :)

	/// <summary>
	/// Hosts your game!
	/// </summary>
	/// <typeparam name="TPlayer">The player to use</typeparam>
	public class Server<TPlayer> : IDisposable
		where TPlayer : Player, new() {
		private readonly Database<TPlayer> _db;
		private readonly List<Room<TPlayer>> _openRooms;
		private readonly List<TPlayer> _players = new List<TPlayer>();

		private readonly Type[] _roomTypes;

		private readonly WebSocketServer _server;

		private readonly QueryExecutioner<SynchronousTask> _taskScheduler;

		//TODO: use reflection instead of asking the user to pass in their room types

		/// <summary>Create a new server! It hosts your game!</summary>
		/// <param name="location">A WebSockets location to run the server on.</param>
		/// <param name="db">The Database to use for storing rooms and players</param>
		/// <param name="roomTypes">The type of each room.</param>
		public Server(string location, Database<TPlayer> db, Type[] roomTypes) {
			this._db = db;
			this._db.Setup();

			foreach (var i in roomTypes) // prevent non-Room<TPlayer>-inheriting types from coming through
				if (!i.IsSubclassOf(typeof(Room<TPlayer>)))
					throw new Exception($"One of the room types passed in doies not inherit from Room<TPlayer>");

			this._roomTypes = roomTypes;
			this._openRooms = new List<Room<TPlayer>>();

			// take multi-threadedness and put it on a QueryExecutioner stack
			this._taskScheduler = new QueryExecutioner<SynchronousTask>();
			this._taskScheduler.ProcessQueryItem += this.ProccesSynchronousTask;

			(this._server = new WebSocketServer(location) {
				RestartAfterListenError = true
			}).Start((socket) => {
				// takes any socket event and throws it onto the task scheduler

				socket.OnOpen = () => this._taskScheduler.AddQueryItem(socket.CreateTask(TaskType.OnOpen));
				socket.OnClose = () => this._taskScheduler.AddQueryItem(socket.CreateTask(TaskType.OnClose));
				socket.OnError = (err) => { socket.Close(); this._taskScheduler.AddQueryItem(socket.CreateErrorTask(err)); };
				socket.OnBinary = (binary) => this._taskScheduler.AddQueryItem(socket.CreateBinaryTask(binary));
				socket.OnMessage = (msg) => this._taskScheduler.AddQueryItem(socket.CreateMessageTask(msg));
			});
		}

		public void Dispose() {
			this._server.Dispose();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Closes a room by kicking every online player, and saving the room to a database.
		/// </summary>
		/// <param name="room">The room instance to close.</param>
		internal void CloseRoom(Room<TPlayer> room) {
			foreach (var i in room._players) {
				i.Kick("Room closing.");
				room._players.Remove(i);
			}

			room.Close();
			this._db.Store(room);
			this._openRooms.Remove(room);
		}

		/// <summary>
		/// Create a room with the room type and room id specified.
		/// Uses reflection & generics, so please don't try to DIY.
		/// </summary>
		/// <param name="roomId">The room id to call it</param>
		/// <param name="roomType">The room type - the rooms that this can make were passed into the constructor.</param>
		/// <returns>An instance of the requested room.</returns>
		internal Room<TPlayer> FindOrCreateRoom(string roomId, string roomType) {
			// THE PLAN:
			// 1) find if the room is open
			//		if found, return it ( do some garbage collection though )
			// 2) search the database for the room
			//		if found, return it
			// 3) create a new instance of the room
			//		return it

			// step 1.

			var rm = FindOpenRoom(roomId, roomType);

			if (rm != null) {
				if (rm._players.Count > 0) return rm;

				// so we found an open room - but it has no players... and is still open for some odd reason?
				// seems like a good time to go through every open room that has no players and kill it.

				for (var i = 0; i < this._openRooms.Count; i++)
					if (this._openRooms[i]._players.Count < 1) {
						// remember the i--
						this._openRooms[i--].Close();
						this._openRooms.Remove(this._openRooms[i]);
					}

				// back to square one

				rm = FindOpenRoom(roomId, roomType);

				if (rm != null) return rm;
			}

			// pre-cursor to step 2

			// get the type of room we need

			var typeFound = default(Type);

			foreach (var i in this._roomTypes) {
				var tmp = Activator.CreateInstance(i) as Room<TPlayer>;
				if (tmp.GetRoomType<TPlayer>() == roomType) {
					typeFound = i; break;
				}
			}

			// only happens when an invalid room type was requested

			if (typeFound == default(Type)) { // no room of that type exists lol
											  // tell the player that the room they're looking for doesn't exist
				return null;
			}

			// step 2 - i'll try to document this since i don't even know what it does

			var dbType = this._db.GetType(); // get's the databases type so we can get methods
			var method = dbType.GetMethod("GetRoom"); // get the GetRoom method
			var generic = method.MakeGenericMethod(typeFound); // create an instance of the generic method, specifying the room type we're looking for
			var objs = new object[] { new RoomRequest { RoomId = roomId, RoomType = roomType }, null }; // pass in parameters - null represents the "out"
			var res = (bool)generic.Invoke(this._db, objs); // invoke it - this method returns a boolean
			if (res) { // if we were able to find the room in the database
				rm = (Room<TPlayer>)objs[1]; // cast the result from an object to a Room<TPlayer> ( we're boxing it from a typeFound though, so it retains properties )

				CreateRoom(rm, roomId);

				return rm;
			}

			// step 3

			rm = Activator.CreateInstance(typeFound) as Room<TPlayer>;

			if (rm.GetRoomType<TPlayer>() == roomType) {
				CreateRoom(rm, roomId);

				return rm;
			}

			// this should never happen

			//TODO: add numbers to the this should never happen
			Console.WriteLine("this should never happen");

			return null;
		}

		internal void CreateRoom(Room<TPlayer> rm, string roomId) {
			rm._serverRef = this;
			this._openRooms.Add(rm); // open the room
			rm.Upgrade(roomId);
			rm.Creation();
		}

		/// <summary>
		/// Checks if a player is online based on their authentication details and returns them
		/// </summary>
		internal bool IsOnline(IUserAuth auth, out TPlayer player) {
			player = default(TPlayer);

			var exists = this._players.Exists(x =>
				x.Auth.Username == auth.Username &&
				x.Auth.Password == auth.Password);

			if (exists) player = this._players.Find(x =>
				 x.Auth.Username == auth.Username &&
				 x.Auth.Password == auth.Password);

			return exists;
		}

		internal void Pop(TPlayer p) {
			this._players.Remove(p);
		}

		internal void Pop(Room<TPlayer> r) {
			this._openRooms.Remove(r);
		}

		/// <summary>
		/// Finds whatever room you're looking for if it's open,
		/// and asks nicely if the player can join.
		/// </summary>
		/// <param name="p">The player to present the room with</param>
		/// <param name="roomId">The room id</param>
		/// <param name="roomType">The room type</param>
		/// <returns>An answer - and null if the room couldn't be found ( should never happen )</returns>
		internal RoomRequestAnswer JoinRoom(TPlayer p, string roomId, string roomType) {
			var room = FindOrCreateRoom(roomId, roomType);

			// this should never happen
			if (room == null) {
				Console.WriteLine("this should never happen");
				return null;
			}

			return room.CanJoin(p);
		}

		/// <summary>
		/// Finds a stored player based on their auth
		/// </summary>
		/// <param name="auth">The authentication of the player</param>
		/// <returns>default(TPlayer) if unable to find, the player if could be found</returns>
		internal TPlayer Lookup(IUserAuth auth) {
			this._db.GetPlayer(auth.Username, out var player);

			if (player == default(TPlayer)) return default(TPlayer);

			return player.Auth.Password == auth.Password ? player : default(TPlayer);
		}

		/// <summary>
		/// Looks in the open players list to find whatever player matches with that socket
		/// </summary>
		/// <param name="socket">The socket to compare</param>
		/// <returns>The index for the player. See FindIndex on a List for more details.</returns>
		private int FindIndexForSocket(IWebSocketConnection socket)
			=> this._players.FindIndex(x => x.Socket.ConnectionInfo.Id == socket.ConnectionInfo.Id);

		/// <summary>
		/// Finds an open room based on it's roomid and room type
		/// </summary>
		private Room<TPlayer> FindOpenRoom(string roomId, string roomType)
			=> this._openRooms.Find(x => x.GetRoomType<TPlayer>() == roomType && x.RoomId == roomId);

		/// <summary>
		/// Processes each synchronous task.
		/// Called by a QueryExecutioner.
		/// </summary>
		/// <param name="task">The task to execute.</param>
		private void ProccesSynchronousTask(SynchronousTask task) {
			try {
				var socket = task.Socket;

				switch (task.Type) {
					case TaskType.OnOpen: {
						var p = new TPlayer();

						p.Setup(socket);

						this._players.Add(p);
					}
					return;

					case TaskType.OnBinary: {
						var pIndex = this.FindIndexForSocket(socket);

						if (pIndex < 0) Console.WriteLine($"Couldn't find a player for socket {socket.ConnectionInfo.Id}"); // this should never happen
						else {
							var p = this._players[pIndex];

							var binary = (task as OnBinaryTask).BinaryData;

							ProcessOnBinary(socket, p, pIndex, binary);
						}
					}
					return;

					case TaskType.OnClose: {
						var pIndex = this.FindIndexForSocket(socket);
						if (pIndex >= 0) {
							var p = this._players[pIndex];

							if (p.Auth.Authenticated) { // if they're authenticated, store their stuff
								this._db.Store(p);
							}

							if (p.Connect.ConnectedToRoom) { // if they're connected to a room
															 // disconnect them from it

								var room = this.FindOrCreateRoom(p.Connect.RoomId, p.Connect.RoomType);

								room.Leave(p);

								if (room._players.Count < 1)
									this.CloseRoom(room);
							}

							this._players.RemoveAt(pIndex);
						} else Console.WriteLine("this should never happen");
					}
					return;

					case TaskType.OnMessage: { // tell developers they need to set the websocket to an array buffer
						var message = (task as OnMessageTask).MessageData;

						if (message == "ping") socket.Send("pong");
						else socket.Send("WSIO only accepts byte arrays - try setting your websocket type to an 'arraybuffer' ( in javascript )"); //TODO: provide link to docs //TODO: only send this once to prevent bandwidth from dieing
					}
					return;

					case TaskType.OnError: {
						// i don't want to deal with anybody who errors me D:<
						socket?.Close();

						var err = (task as OnErrorTask).Error;
						Console.WriteLine(err.Message);
					}
					return;

					// this should never happen
					default: throw new Exception("Unhandled task type");
				}
			} catch (Exception error) {
				// the god try catch
				// we don't want the server to die if an exception happens

				// we'll log the exception along with all the information
				// then we'll shut down the room and disconnect every player related
				// so that way, all the other rooms are still up and running

				Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine($"Error - {error.GetType()}");
				Console.WriteLine($"{error.StackTrace}");
				Console.WriteLine();

				/*
				// try to find the player

				try {
					var p = this._players?.Find(x => x?.Socket == task?.Socket);

					if (p == null)
						Console.WriteLine($"Unable to find the player for the socket.");
					else {
						var name = p.Auth?.Username;

						Console.WriteLine($"Happened for player '{name ?? ""}'");
						Console.WriteLine();

						var rmId = p.Connect?.RoomId;
						var rmType = p.Connect?.RoomType;

						if (rmId != null && rmType != null) {
							var rm = FindOpenRoom(rmId, rmType);

							if (rm != null) {
								Console.WriteLine($"Room: {rmId}, {rmType}");

								CloseRoom(rm);
							}
						}
					}
				}catch(Exception error_closing) {
					// F to pay respects

					Console.WriteLine($"Error closing room: {error_closing.GetType()}");
					Console.WriteLine($"{error_closing.StackTrace}");
				}
				*/

				Console.ResetColor();
			}
		}

		/// <summary>
		/// Processes any OnBinary message
		/// essentially handles the entire operation
		/// </summary>
		private void ProcessOnBinary(IWebSocketConnection socket, TPlayer p, int pIndex, byte[] binary) {
			using (var ms = new System.IO.MemoryStream(binary)) {
				ProtoMessage msg;

				try {
					msg = Serializer.Deserialize<ProtoMessage>(ms);
				} catch (ProtoException) {
					// we won't even bother if the client sends us invalid data
					socket.Close();
					return;
				}

				var handler = WSIOProtocolHandler.GetWSIOProtocolHandler(msg.ProtoDefs.ProtocolVersion);

				// yeah this needs to be here
				ms.Seek(0, System.IO.SeekOrigin.Begin);

				var changed = default(TPlayer);
				var onMsg = handler?.Handle(this, p, out changed, msg.ProtoDefs, ms);
				p = changed;

				// could've just popped the player

				//TODO: `out bool` for if we should do this - probably a micro-optimization but /shrug
				this._players[pIndex] = changed;

				// we could either be handleing an OnMessage because the player is connected to a room,
				// or something else like DB requests, authentication, e.t.c

				if (onMsg != null) {
					var room = this.FindOrCreateRoom(p.Connect.RoomId, p.Connect.RoomType);

					// send the message the player has
					room.Message(p, onMsg);
				}
			}
		}
	}
}