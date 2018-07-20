using WSIO.Protobuf;

namespace WSIO {

	/// <summary>
	/// Doesn't save nor load players.
	/// See DictionaryDatabase if you'd like an in-memory database
	/// use this if you want nothing stored
	/// </summary>
	/// <typeparam name="TPlayer"></typeparam>
	public class UnknownDatabase<TPlayer> : Database<TPlayer>
		where TPlayer : Player, new() {

		public override void Dispose() {
		}

		public override bool GetPlayer(string username, out TPlayer player) {
			player = default(TPlayer);
			return false;
		}

		public override bool GetRoom<TRoom>(RoomRequest request, out TRoom room) {
			room = default(TRoom);
			return false;
		}

		public override void Setup() {
		}

		public override void Store(TPlayer p) {
		}

		public override void Store(Room<TPlayer> room) {
		}
	}
}