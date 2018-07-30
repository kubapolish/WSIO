using Fleck;

using System;
using System.IO;
using System.Net;
using WSIO.Messages;

namespace WSIO {

	public class Server<TPlayer, TAuth> : IDisposable
		where TPlayer : Player, new()
		where TAuth : Authentication.IAuthModule {
		private WebSocketServer _server;
		private TAuth _auth;

		public string Location => this._server.Location;
		public ushort Port => (ushort)this._server.Port;

		public Server(string[] commandLineArgs, TAuth auther, ushort port, params Type[] enabledRooms) {
			this._auth = auther;

			this._players = new PlayerManager<TPlayer>();
			this._rooms = new RoomManager<TPlayer>(enabledRooms);

			this._server = new WebSocketServer($"ws://0.0.0.0:{port}");
			this._server.RestartAfterListenError = true;
		}

		private RoomManager<TPlayer> _rooms { get; }
		private PlayerManager<TPlayer> _players { get; }

		public void Dispose() {
			this._server.Dispose();
		}

		public void Start() {
			this._server.Start((socket) => {
				socket.OnOpen = () => {
					var req = new PlayerRequest(null, null, socket);

					var p = new TPlayer();
					p.SetupBy(req);

					if (!this._players.ExistsBy(req, out var __))
						this._players.Add(p);
				};

				socket.OnBinary = (data) => {
					var player = default(TPlayer);

					var players = this._players.Items;
					foreach(var i in players) {
						if (i.Socket == socket)
							player = i;
					}

					if (player != default(TPlayer)) {
						
						using (var ms = new MemoryStream(data))
							ProtoSerializer.Handle(player, this._auth, this._rooms, ms);
					} else Console.WriteLine($"Could not find player for {socket?.ConnectionInfo?.ClientIpAddress}");
				};

				socket.OnClose = () => {
					foreach (var i in this._players.Items)
						if (i?.Socket?.ConnectionInfo?.Id == socket.ConnectionInfo.Id) {
							
							if(i.ConnectedTo != null) {
								MessageHandler.DisconnectPlayerFrom(this._rooms, ((Room<TPlayer>)i.ConnectedTo), i);
							}

							if (i.Username != null && i.Password != null) {
								//TODO: log them out
							}
							
							this._players.Delete(i);

							break;
						}
				};
			});
		}
	}
}