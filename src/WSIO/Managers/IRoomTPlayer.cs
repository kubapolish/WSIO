namespace WSIO {

	public interface IRoom<TPlayer>
		where TPlayer : Player, new() {
		TPlayer[] Players { get; }
	}
}