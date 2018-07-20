using System;
using System.Collections.Generic;

namespace WSIO {

	public class Manager<TItem, TRequest> : IManager<TItem, TRequest>
	where TItem : IManagerItem<TRequest>, new()
	where TRequest : IManagerRequest {
		private readonly List<TItem> _items;

		public Manager() => this._items = new List<TItem>();

		public TItem[] Items => new List<TItem>(this._items).ToArray();

		public void Add(TItem item) {
			if (this.ExistsBy(item.RequestInfo, out var __)) throw new Exception("Duplicate item found");
			else this._items.Add(item);
		}

		public bool ExistsBy(TRequest request, out TItem item) {
			item = this._items.Find(x => x.RequestInfo.Equals(request));

			if (item == null) return false;
			return !item.Equals(default(TItem));
		}

		public TItem FindBy(TRequest request) {
			if (this.ExistsBy(request, out var __)) return __;
			else throw new Exception($"Item doesn't exist");
		}

		public TItem FindOrAddBy(TItem item) {
			if (this.ExistsBy(item.RequestInfo, out var __)) return __;
			else {
				this.Add(item);
				return item;
			}
		}
	}
}