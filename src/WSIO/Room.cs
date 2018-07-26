using System;
using System.Threading;
using TimedQuery;
using WSIO.Messages;

namespace WSIO {

	[RoomType(null)]
	public class Room<TPlayer> : IRoom<TPlayer>, IManagerItem<RoomRequest>, IDisposable
		where TPlayer : Player, new() {
		internal PlayerManager<TPlayer> _players;

		//TODO: add disposablility to TimedQuery
		private QueryExecutioner<RoomItem<TPlayer>> _query;

		public Room() {
			this._players = new PlayerManager<TPlayer>();
			this._query = new QueryExecutioner<RoomItem<TPlayer>>();
			this._query.ProcessQueryItem += this.ProcessQuery;
		}

		private void ProcessQuery(RoomItem<TPlayer> item) {
			switch (item.Event) {
				case RoomEvent.Creation: {
					OnCreation();
				} break;

				case RoomEvent.Join: {
					OnJoin(item.PlayerData);
				} break;

				case RoomEvent.Message: {
					OnMessage(item.PlayerData, new Message(item.MessageData));
				} break;

				case RoomEvent.Leave: {
					OnLeave(item.PlayerData);
				} break;

				case RoomEvent.Deletion: {
					OnDeletion();
				} break;

				default: throw new Exception($"Unabled event type {item.Event}");
			}

			item.ManualResetEvent.Set();
			if (item.DoDispose)
				item.Dispose();
		}

		private void WaitForFinish(RoomItem<TPlayer> item) {
			this._query.AddQueryItem(item);
			item.ManualResetEvent.WaitOne();
		}

		public TPlayer[] Players => this._players.Items;

		public RoomRequest RequestInfo { get; private set; }

		public string RoomId => this.RequestInfo?.RoomId;
		public string RoomType => this.RequestInfo?.RoomType;

		internal void Creation() {
			using (var itm = new RoomItem<TPlayer> {
				Event = RoomEvent.Creation,
				DoDispose = false,
			})
				WaitForFinish(itm);
		}

		internal void Connect(TPlayer p) {
			this._players.Add(p);
			p.ConnectTo(this);

			using (var itm = new RoomItem<TPlayer> {
				Event = RoomEvent.Join,
				DoDispose = false,
				PlayerData = p,
			})
				WaitForFinish(itm);
		}

		internal void Message(TPlayer p, IMessage msg) {
			this._query.AddQueryItem(new RoomItem<TPlayer> {
				Event = RoomEvent.Message,
				PlayerData = p,
				MessageData = msg,
			});
		}

		internal void Disconnect(TPlayer p) {
			Console.WriteLine("this._players");
			this._players.Delete(p);
			p.ConnectTo(null);
			
			using (var itm = new RoomItem<TPlayer> {
				Event = RoomEvent.Leave,
				DoDispose = false,
				PlayerData = p,
			})
				WaitForFinish(itm);
		}

		internal void Deletion() {
			using (var itm = new RoomItem<TPlayer> {
				Event = RoomEvent.Deletion,
				DoDispose = false,
			})
				WaitForFinish(itm);
		}

		public void SetupBy(RoomRequest request) {
			//this.Creation();
			this.RequestInfo = request;
		}

		public virtual void OnCreation() { }
		public virtual void OnJoin(TPlayer p) { }
		public virtual void OnMessage(TPlayer p, Message msg) { }
		public virtual void OnLeave(TPlayer p) { }
		public virtual void OnDeletion() { }

		public void Dispose() {
			this._query.Dispose();
		}
	}

	internal class RoomItem<TPlayer> : IDisposable
		where TPlayer : Player, new() {
		public RoomItem() => this.ManualResetEvent = new ManualResetEvent(false);

		public ManualResetEvent ManualResetEvent { get; }
		public RoomEvent Event { get; set; }
		public bool DoDispose { get; set; } = true;

		public TPlayer PlayerData { get; set; }
		public IMessage MessageData { get; set; }

		public void Dispose() {
			if (this.DoDispose) {
				this.ManualResetEvent.Dispose();
				GC.SuppressFinalize(this);
			}
		}
	}

	internal enum RoomEvent {
		Creation,
		Join,
		Message,
		Leave,
		Deletion,
	}
}