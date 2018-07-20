using System;

namespace WSIO {

	public class Creator<TItem, TRequest> : IManager<TItem, TRequest>, ICreator<TItem, TRequest>
		where TItem : IManagerItem<TRequest>, new()
		where TRequest : IManagerRequest {
		private readonly Manager<TItem, TRequest> _manager;

		public Creator() => this._manager = new Manager<TItem, TRequest>();

		public bool CanCreate(TRequest request) => true;

		public TItem[] Items => this._manager.Items;

		public void Add(TItem item) => this._manager.Add(item);

		public TItem CreateBy(TRequest request) {
			if (this.ExistsBy(request, out var __)) throw new Exception($"Duplicate item found");
			else return Create(request);
		}

		public bool ExistsBy(TRequest request, out TItem item) => this._manager.ExistsBy(request, out item);

		public TItem FindBy(TRequest request) => this._manager.FindBy(request);

		public TItem FindOrAddBy(TItem item) => this._manager.FindOrAddBy(item);

		public TItem FindOrCreateBy(TRequest request) => this.ExistsBy(request, out var __) ? __ : Create(request);

		private TItem Create(TRequest req) {
			var item = new TItem();
			item.SetupBy(req);

			this.Add(item);
			return item;
		}
	}
}