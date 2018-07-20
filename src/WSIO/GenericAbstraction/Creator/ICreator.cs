namespace WSIO {

	public interface ICreator<TItem, TRequest>
		where TItem : IManagerItem<TRequest>
		where TRequest : IManagerRequest {

		TItem CreateBy(TRequest request);

		TItem FindOrCreateBy(TRequest request);

		bool CanCreate(TRequest request);
	}
}