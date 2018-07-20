using System.Collections.Generic;

using WSIO.Protobuf;

namespace WSIO {
	//TODO: spell peculiar right
	//TODO: comment

	/// <summary>
	/// Stores everything in memory.
	/// Despite being called a DictionaryDatabase, it, in fact uses lists.
	/// how peculiar :D
	/// </summary>
	public class DictionaryDatabase<TPlayer> : Database<TPlayer>
		where TPlayer : Player, new() {
		private List<TPlayer> _players;
		private List<Room<TPlayer>> _rooms;

		public override void Dispose() {
		}

		public override bool GetPlayer(string username, out TPlayer player) {
			var res = this._players.Find(x => x.Username == username);
			player = res;
			return res != default(TPlayer);
		}

		public override bool GetRoom<TRoom>(RoomRequest request, out TRoom room) {
			var res = this._rooms.Find(x => x.RoomId == request.RoomId && x.GetRoomType() == request.RoomType);
			room = (TRoom)res;
			return res != default(TRoom);
		}

		public override void Setup() {
			this._players = new List<TPlayer>();
			this._rooms = new List<Room<TPlayer>>();
		}

		public override void Store(TPlayer player) {
			if (this.GetPlayer(player.Username, out var existing))
				this._players[this._players.IndexOf(existing)] = player;
			else this._players.Add(player);
		}

		public override void Store(Room<TPlayer> room) {
			if (this.GetRoom<Room<TPlayer>>(new RoomRequest { RoomId = room.RoomId, RoomType = room.GetRoomType() }, out var existing))
				this._rooms[this._rooms.IndexOf(existing)] = room;
			else this._rooms.Add(room);
		}
	}
}