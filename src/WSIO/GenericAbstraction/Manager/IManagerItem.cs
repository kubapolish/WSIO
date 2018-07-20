namespace WSIO {

	public interface IManagerItem<TRequest>
			where TRequest : IManagerRequest {
		TRequest RequestInfo { get; }

		void SetupBy(TRequest request);
	}
}