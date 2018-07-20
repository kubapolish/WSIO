namespace WSIO {

	public interface IManager<TItem, TRequest>
		where TItem : IManagerItem<TRequest>
		where TRequest : IManagerRequest {
		TItem[] Items { get; }

		void Add(TItem item);

		bool ExistsBy(TRequest request, out TItem item);

		TItem FindBy(TRequest request);

		TItem FindOrAddBy(TItem item);
	}
}