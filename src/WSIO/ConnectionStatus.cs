namespace WSIO {

	/// <summary>
	/// Represents the status of a player's connection to WSIO.
	/// </summary>
	public class ConnectionStatus {

		internal ConnectionStatus() => this.ConnectedToRoom = false;

		/// <summary>
		/// If they're connected to a room
		/// </summary>
		public bool ConnectedToRoom { get; private set; }

		/// <summary>
		/// The RoomId they're connected to.
		/// </summary>
		public string RoomId { get; private set; }

		/// <summary>
		/// The RoomType they're connected to.
		/// </summary>
		public string RoomType { get; private set; }

		internal void Upgrade(string roomid, string roomtype) {
			if (roomid == null || roomtype == null) this.ConnectedToRoom = false;
			else {
				this.ConnectedToRoom = true;
				this.RoomType = roomtype;
				this.RoomId = roomid;
			}
		}
	}
}