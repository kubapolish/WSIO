using System;

using WSIO.Protobuf;

namespace WSIO {
	// abstract database implementation so we can implement more dbs, e.g. LiteDB or some other db

	/// <summary>
	/// Abstract class so the end user can implement other database types, e.g. LiteDB
	/// </summary>
	public abstract class Database<TPlayer> : IDisposable
		where TPlayer : Player, new() {

		public abstract void Dispose();

		public abstract void Setup();

		public abstract bool GetPlayer(string username, out TPlayer player);

		public abstract bool GetRoom<TRoom>(RoomRequest request, out TRoom room)
			where TRoom : Room<TPlayer>, new();

		public abstract void Store(TPlayer player);

		public abstract void Store(Room<TPlayer> room);
	}
}