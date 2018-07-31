using System;

using WSIO.Attributes;

namespace WSIO {

	public class RoomManager<TPlayer> : ICreator<Room<TPlayer>, RoomRequest>, IManager<Room<TPlayer>, RoomRequest>
		where TPlayer : Player, new() {
		private readonly Manager<Room<TPlayer>, RoomRequest> _manager;

		public RoomManager(Type[] enabledRooms) {
			foreach (var i in enabledRooms)
				if (!i.IsSubclassOf(typeof(Room<TPlayer>)))
					throw new Exception($"{i} does not inherit from Room<{typeof(TPlayer)}>");

			this._enabledRoomTypes = enabledRooms;

			this._manager = new Manager<Room<TPlayer>, RoomRequest>();
		}

		public Room<TPlayer>[] Items => this._manager.Items;

		private Type[] _enabledRoomTypes { get; }

		public void Add(Room<TPlayer> item) => this._manager.Add(item);

		public void Delete(Room<TPlayer> item) => this._manager.Delete(item);

		public bool CanCreate(RoomRequest request) => GetRoomForType(request.RoomType) != null && !this._manager.ExistsBy(request, out var __);

		public Room<TPlayer> CreateBy(RoomRequest request) {
			var t = GetRoomForType(request.RoomType) ?? throw new InvalidOperationException("No room of the roomtype requested exists.");

			return Create(t, request);
		}

		public bool ExistsBy(RoomRequest request, out Room<TPlayer> item) => this._manager.ExistsBy(request, out item);

		public Room<TPlayer> FindBy(RoomRequest request) => this._manager.FindBy(request);

		public Room<TPlayer> FindOrAddBy(Room<TPlayer> item) => this._manager.FindOrAddBy(item);

		public Room<TPlayer> FindOrCreateBy(RoomRequest request) {
			lock (this._manager._lock) {
				var t = GetRoomForType(request.RoomType);
				if (t == null) return null;

				var rm = (Room<TPlayer>)Activator.CreateInstance(t);
				rm.SetupBy(request);

				return this._manager.UnlockedExistsBy(request, out var __) ? __ : this.Create(t, request);
			}
		}

		private Room<TPlayer> Create(Type t, RoomRequest request) {
			var rm = (Room<TPlayer>)Activator.CreateInstance(t);
			rm.SetupBy(request);
			this._manager.UnlockedAdd(rm);
			rm.Creation();

			return rm;
		}

		private Type GetRoomForType(string roomType) {
			if (roomType == null) return null;
			foreach (var i in this._enabledRoomTypes)
				if (i.GetAttribute<RoomTypeAttribute>(out var attrib) && attrib.RoomType == roomType)
					return i;
			return null;
		}
	}
}