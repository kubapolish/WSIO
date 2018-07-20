namespace WSIO {

	[RoomType(null)]
	public class Room<TPlayer> : IRoom, IRoom<TPlayer>, IManagerItem<RoomRequest>
		where TPlayer : Player, new() {
		internal PlayerManager<TPlayer> _players;

		public Room() => this._players = new PlayerManager<TPlayer>();

		public TPlayer[] Players => this._players.Items;

		public RoomRequest RequestInfo { get; private set; }

		public void SetupBy(RoomRequest request)
			=> this.RequestInfo = request;
	}
}