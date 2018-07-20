using System;

namespace WSIO {
	public class Server<TPlayer> : IDisposable
		where TPlayer : Player, new() {
		private PlayerManager<TPlayer> _players { get; }
		private RoomManager<TPlayer> _rooms { get; }

		public Server(params Type[] enabledRooms) {
			this._players = new PlayerManager<TPlayer>();
			this._rooms = new RoomManager<TPlayer>(enabledRooms);
		}

		public TPlayer Generate(PlayerRequest req)
			=> this._players.FindOrCreateBy(req);

		public Room<TPlayer> Send(TPlayer p, RoomRequest req) {
			if (!this._players.ExistsBy(p.RequestInfo, out var __)) throw new Exception("That player is not in our game.");

			var room = this._rooms.FindOrCreateBy(req);
			room.SetupBy(req);
			p.ConnectTo(room);

			return room;
		}

		public void Dispose() {
		}
	}
}