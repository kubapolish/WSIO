namespace WSIO {

	public class PlayerManager<TPlayer> : Creator<TPlayer, PlayerRequest>
		where TPlayer : Player, new() {
	}
}