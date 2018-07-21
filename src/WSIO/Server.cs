﻿using Fleck;

using System;
using System.IO;

using WSIO.Messages;

namespace WSIO {

	public class Server<TPlayer> : IDisposable
		where TPlayer : Player, new() {
		private WebSocketServer _server;

		public string Location => this._server.Location;
		public ushort Port => (ushort)this._server.Port;

		public Server(string[] commandLineArgs, ushort port, params Type[] enabledRooms) {
			this._players = new PlayerManager<TPlayer>();
			this._rooms = new RoomManager<TPlayer>(enabledRooms);

			this._server = new WebSocketServer($"ws://0.0.0.0:{port}");
			this._server.RestartAfterListenError = true;
		}

		private RoomManager<TPlayer> _rooms { get; }
		private PlayerManager<TPlayer> _players { get; }

		public void Dispose() {
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

					if (player != default(TPlayer))
						using (var ms = new MemoryStream(data))
							ProtoSerializer.Handle(player, this._rooms, ms);
					else Console.WriteLine($"Could not find player for {socket?.ConnectionInfo?.ClientIpAddress}");
				};

				socket.OnClose = () => {
					foreach (var i in this._players.Items)
						if (i.Socket == socket)
							this._players.Delete(i);
				};
			});
		}
	}
}