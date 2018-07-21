using ProtoBuf;

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using vtortola.WebSockets;
using WSIO.Messages;

namespace WSIO {

	public class Server<TPlayer> : IDisposable
		where TPlayer : Player, new() {
		private WebSocketListener _server;

		public Server(string[] commandLineArgs, params Type[] enabledRooms) {
			this._players = new PlayerManager<TPlayer>();
			this._rooms = new RoomManager<TPlayer>(enabledRooms);

			this._server = new WebSocketListener(new IPEndPoint(IPAddress.Any, 80));
			this._server.Standards.RegisterStandard(new WebSocketFactoryRfc6455());

			this.CancellationToken = new CancellationTokenSource();
		}

		public CancellationTokenSource CancellationToken { get; }
		private PlayerManager<TPlayer> _players { get; }
		private RoomManager<TPlayer> _rooms { get; }

		public void Dispose() {
		}

		public async Task StartAsync() {
			var task = Task.Run(() => AcceptClientsAsync(this.CancellationToken.Token));
			await this._server.StartAsync(this.CancellationToken.Token);
		}

		private async Task AcceptClientsAsync(CancellationToken cancellationToken) {
			while (!cancellationToken.IsCancellationRequested) {
				try {
					var ws = await this._server.AcceptWebSocketAsync(cancellationToken).ConfigureAwait(false);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					if (ws != null) Task.Run(() => HandleConnectionAsync(ws, cancellationToken));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				} catch (Exception aex) {
					Console.WriteLine($"Error accepting clients: " + aex.GetBaseException().Message);
				}
			}
		}

		private async Task HandleConnectionAsync(WebSocket ws, CancellationToken cancellationToken) {
			try {
				var p = new TPlayer();
				p.SetupBy(new PlayerRequest(null, null, ws));
				this._players.Add(p);

				while (ws.IsConnected && !cancellationToken.IsCancellationRequested) {
					var msg = await ws.ReadMessageAsync(cancellationToken).ConfigureAwait(false);
					if (msg != null) {
						await HandleMessageAsync(ws, MakeCopy(msg));
					}
				}
			} catch (Exception aex) {
				Console.WriteLine("Error Handling connection: " + aex.GetBaseException().Message);
				try { ws.Close(); } catch { }
			} finally {
				ws.Dispose();
			}
		}

		//TODO: move functionality into another class

		private async Task HandleMessageAsync(WebSocket ws, Stream data) {
			if (!ProtoSerializer.Deserialize<ProtoMessage>(data, out var res)) {
				await ProtoSerializer.Write(ProtoSerializer.GetV1Success(false, "Unable to deserialize your message."), ws);
				return;
			}

			if(!ProtoMessage.FindTypeFor(res, out var type)) {
				await ProtoSerializer.Write(ProtoSerializer.GetV1Success(false, "Unable to find the type for your message."), ws);
				return;
			}

			switch(Activator.CreateInstance(type)) {
				case Messages.v1.Authentication auth: {

				} break;

				default: break;
			}
		}
		private Stream MakeCopy(Stream original) {
			var ms = new MemoryStream();
			original.CopyTo(ms);
			return ms;
		}
	}
}