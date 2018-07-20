using MongoDB.Driver;

using System;

using WSIO.Protobuf;

namespace WSIO {

	//TODO: comment
	public class MongoDB<TPlayer> : Database<TPlayer>
		where TPlayer : Player, new() {
		private MongoClient _client;
		private string _colName;

		private IMongoDatabase _db;
		private IMongoCollection<TPlayer> _players;
		private IMongoCollection<Room<TPlayer>> _rooms;

		public MongoDB(string connectionString, string serverName) {
			this._client = new MongoClient(connectionString);
			this._colName = serverName;
		}

		public override void Dispose() {
			//TODO: figure out how to dispose a mongodb connection

			this._players = null;
			this._rooms = null;
			this._db = null;
			this._client = null;
			this._colName = null;
		}

		public override bool GetPlayer(string username, out TPlayer player) {
			player = default(TPlayer);

			var find = this._players.Find(x => x.Auth.Username == username);
			var count = find.CountDocuments();

			if (count > 0) {
				player = find.FirstOrDefault();

				if (count > 1) //wow that's odd
					Console.WriteLine("Found more then 2 instances of the requested player...");

				return true;
			}

			return false;
		}

		public override bool GetRoom<TRoom>(RoomRequest request, out TRoom room) {
			room = default(TRoom);

			var find = this._rooms.Find(x => x.Info.RoomId == request.RoomId && x.Info.RoomType == request.RoomType);
			var count = find.CountDocuments();

			if (count > 0) {
				try {
					room = find.As<TRoom>().FirstOrDefault();
				} catch (FormatException) {
					// mongodb couldn't deserialize some data :c
					Console.WriteLine($"Unable to retrieve MongoDB room for {request.RoomId} {request.RoomType}");

					return false;
				}
				if (count > 1) //wow that's odd
					Console.WriteLine("Found more then 2 instances of the requested room...");

				return true;
			}

			return false;
		}

		public override void Setup() {
			this._db = this._client.GetDatabase($"WSIO-{this._colName}");

			this._rooms = this._db.GetCollection<Room<TPlayer>>("rooms");
			this._players = this._db.GetCollection<TPlayer>("players");
		}

		public override void Store(TPlayer player) {
			var find = this._players.Find(x => x.Auth == player.Auth);
			var count = find.CountDocuments();

			if (count > 0) {
				if (count > 1) //wow that's odd
					Console.WriteLine("Found more then 2 instances of the requested player...");

				this._players.ReplaceOne(x => x.Auth == player.Auth, player);
			} else this._players.InsertOne(player);
		}

		public override void Store(Room<TPlayer> room) {
			var find = this._rooms.Find(x => x.Info.RoomId == room.RoomId && x.Info.RoomType == room.GetRoomType<TPlayer>());
			var count = find.CountDocuments();

			if (count > 0) {
				if (count > 1) //wow that's odd
					Console.WriteLine("Found more then 2 instances of the requested player...");

				this._rooms.ReplaceOne(x => x.Info.RoomId == room.RoomId && x.Info.RoomType == room.GetRoomType<TPlayer>(), room);
			} else this._rooms.InsertOne(room);
		}
	}
}