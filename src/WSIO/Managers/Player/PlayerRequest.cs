using vtortola.WebSockets;

namespace WSIO {

	public class PlayerRequest : IManagerRequest {

		public PlayerRequest(string username, string password, WebSocket socket) {
			this.Username = username;
			this.Password = password;
			this.Socket = socket;
		}

		public string Username { get; }
		public string Password { get; }
		public WebSocket Socket { get; }

		public bool Equals(IManagerRequest other)
			=> other is PlayerRequest req ?
				(req.Username == this.Username)
				: false;
	}
}