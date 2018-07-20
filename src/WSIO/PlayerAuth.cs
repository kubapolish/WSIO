namespace WSIO {
	//TODO: not this

	/// <summary>
	/// Authentication information for a player.
	/// </summary>
	public class PlayerAuth : IUserAuth {

		internal PlayerAuth() => this.Authenticated = false;

		/// <summary>
		/// If the player is logged in
		/// </summary>
		public bool Authenticated { get; private set; }

		/// <summary>
		/// Their SHA256 hashed password
		/// </summary>
		public string Password { get; private set; }

		/// <summary>
		/// Their username
		/// </summary>
		public string Username { get; private set; }

		/// <summary>
		/// Sets their username/password
		/// </summary>
		internal void Upgrade(string u, string p) {
			if (u == null || p == null) this.Authenticated = false;
			else {
				this.Authenticated = true;
				this.Username = u;
				this.Password = p;
			}
		}
	}
}