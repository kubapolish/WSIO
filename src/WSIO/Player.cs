using Fleck;

namespace WSIO {

	public class Player : IManagerItem<PlayerRequest> {
		public PlayerRequest RequestInfo { get; private set; }

		public string Username => this.RequestInfo?.Username;
		public string Password => this.RequestInfo?.Password;
		public IWebSocketConnection Socket => this.RequestInfo?.Socket;

		public IRoom ConnectedTo { get; private set; }

		/// <remarks>Please use the room.ConnectTo method instead if you can</remarks>
		internal void ConnectTo(IRoom room) => this.ConnectedTo = room;

		public void SetupBy(PlayerRequest request)
			=> this.RequestInfo = request ?? new PlayerRequest(null, null, null);
	}
}