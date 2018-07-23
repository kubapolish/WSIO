using System;
using System.Collections.Generic;

namespace WSIO {

	public class Manager<TItem, TRequest> : IManager<TItem, TRequest>
	where TItem : IManagerItem<TRequest>, new()
	where TRequest : IManagerRequest {
		private readonly List<TItem> _items;
		internal readonly object _lock = new object();

		public Manager() => this._items = new List<TItem>();

		public TItem[] Items {
			get {
				lock (this._lock)
					return new List<TItem>(this._items).ToArray();
			}
		}

		public void Add(TItem item) {
			lock (this._lock) this.UnlockedAdd(item);
		}

		public void Delete(TItem item) {
			lock (this._lock) UnlockedDelete(item);
		}

		public bool ExistsBy(TRequest request, out TItem item) {
			lock (this._lock)
				return UnlockedExistsBy(request, out item);
		}

		public TItem FindBy(TRequest request) {
			lock (this._lock) return UnlockedFindBy(request);
		}

		public TItem FindOrAddBy(TItem item) {
			lock (this._lock) return this.UnlockedFindOrAddBy(item);
		}

		internal void UnlockedAdd(TItem item) {
			if (this.UnlockedExistsBy(item.RequestInfo, out var __)) throw new Exception("Duplicate item found");
			else
				this._items.Add(item);
		}

		internal void UnlockedDelete(TItem item) {
			if (!this.UnlockedExistsBy(item.RequestInfo, out var __)) throw new Exception("Item doesn't exist to delete");
			else {
				foreach (var i in this._items)
					if (i.RequestInfo.Equals(item.RequestInfo)) {
						this._items.Remove(i);
						return;
					}
				throw new Exception("Unable to find item... huh?");
			}
		}

		internal bool UnlockedExistsBy(TRequest request, out TItem item) {
			item = this._items.Find(x => x.RequestInfo.Equals(request));

			if (item == null) return false;
			else return true;
		}

		internal TItem UnlockedFindBy(TRequest request) {
			if (this.UnlockedExistsBy(request, out var __)) return __;
			else throw new Exception($"Item doesn't exist");
		}

		internal TItem UnlockedFindOrAddBy(TItem item) {
			if (this.UnlockedExistsBy(item.RequestInfo, out var __)) return __;
			else {
				this.Add(item);
				return item;
			}
		}
	}
}