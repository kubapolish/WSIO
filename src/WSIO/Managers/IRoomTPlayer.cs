namespace WSIO {

	public interface IRoom<TPlayer> : IRoom
		where TPlayer : Player, new() {
		TPlayer[] Players { get; }
	}
}