using MongoDB.Bson.Serialization.Attributes;

using System;
using System.Collections.Generic;
using System.Linq;

namespace WSIO {

	/// <summary>
	/// A Room - provides a simple way to group your players together in one place.
	/// </summary>
	/// <typeparam name="TPlayer">The player that this room requires.</typeparam>
	public class Room<TPlayer>
		where TPlayer : Player, new() {

		//TODO: figure out if `public` can be set to `internal`
		/// <summary>Used for MongoDB stuff, don't use it.</summary>
		[BsonId]
		public RoomInfo Info => new RoomInfo(this.RoomId, this.GetRoomType<TPlayer>());

		/// <summary>
		/// Gets a list of your players
		/// Useful if you need to interact with each one.
		/// </summary>
		public TPlayer[] Players => this._players.ToArray();

		/// <summary>
		/// The RoomId of this room.
		/// Don't change it, you'll break stuff D:
		/// </summary>
		public string RoomId { get; private set; }

		internal Server<TPlayer> _serverRef { get; set; }

		internal List<TPlayer> _players { get; set; } = new List<TPlayer>();

		// public helper functions

		public void Broadcast(string type, params object[] args)
			=> Broadcast(WSIO.ProtoSerializedMessage.Create(type, args));

		public void Broadcast(ProtoSerializedMessage m) {
			foreach (var i in this._players) i.Send(m);
		}

		//TODO: figure out how to get the server to realize that this room has closed.
		public void CloseRoom(string reason) {
			this._serverRef.CloseRoom(this);
		}

		// overrideable stuff below - the end user should override and add functionality

		/// <summary>
		/// Must be overriden for your room to work :p
		///
		/// Override this function with some logic to determine if a player can join.
		/// If a player is allowed to join, set the reason to null. This is what you should do.
		/// </summary>
		/// <param name="p">The player to check if they can join</param>
		public virtual RoomRequestAnswer CanJoin(TPlayer p)
			=> new RoomRequestAnswer(false, "This room has not overridden the CanJoin property.");

		// i gave each function definition personality
		// what are you going to do about it, submit a pull request?

		/// <summary>Override me! I execute when the room closes.</summary>
		public virtual void OnClose() { }

		/// <summary>Override, me. I, the OnCreation function, execute ON CREATION. It's, pretty, self explanitory.</summary>
		public virtual void OnCreation() { }

		/// <summary>whenever a player joins i'm called... pretty important... override me...</summary>
		/// <param name="p">the... th-player</param>
		public virtual void OnJoin(TPlayer p) { }

		/// <summary>I AM THE ONLEAVE FUNCTION. OVERRIDE ME TO EXECUTE CODE WHEN A PLAYER LEAVES.</summary>
		/// <param name="p">THE PLAYER THAT LEFT!</param>
		public virtual void OnLeave(TPlayer p) { }

		/// <summary>Override me. Now. I'm the on message function. Y'know, whenever a message gets called? I'm a pretty big deal...</summary>
		/// <param name="p">The player.</param>
		/// <param name="m">The message. The type is `Message`, should be pretty clear.</param>
		public virtual void OnMessage(TPlayer p, ProtoSerializedMessage m) { }

		// internal stuff - the code calls these functions, and these functions call the virtual functions.

		internal void Close() {
			if (this._players.Count > 0) // this should never happen
				throw new Exception("We still got some extra players, this is odd...");

			this.OnClose();
		}

		internal void Creation()
			=> this.OnCreation();

		internal void Join(TPlayer p) {
			this._players.Add(p);

			this.OnJoin(p);
		}

		internal void Leave(TPlayer p) {
			this.OnLeave(p);

			this._players.Remove(p);
		}

		internal void Message(TPlayer p, ProtoSerializedMessage m)
			=> this.OnMessage(p, m);

		//TODO: figure out why this function exists
		internal void Upgrade(string roomId)
			=> this.RoomId = roomId;
	}

	// for MongoDB's BsonId property for serialization purposes.
	//TODO: figure out if we can simply not do this.

	/// <summary>For MongoDB. Un-needed in any practical purpose, and if you're using it, you're doing it wrong.</summary>
	public class RoomInfo {

		public RoomInfo(string roomId, string roomType) {
			this.RoomId = roomId;
			this.RoomType = roomType;
		}

		public string RoomId { get; }
		public string RoomType { get; }
	}

	/// <summary>Used for rooms for if a player can join.</summary>
	public class RoomRequestAnswer {

		/// <summary>Create a new answer.</summary>
		/// <param name="canJoin">If the player is allowed to join</param>
		/// <param name="reason">The reason why the player isn't allowed to join - leave it as null if they're allowed to join.</param>
		public RoomRequestAnswer(bool canJoin, string reason = null) {
			this.CanJoin = canJoin;

			//TODO: convince myself that this should go into all production releases
			/*
			if (this.CanJoin &&
				(string.IsNullOrEmpty(reason) ||
				string.IsNullOrWhiteSpace(reason)))
				throw new Exception("have you been reading ANY documentation?!?! this should be NULL if the player is ALLOWED to join");
			*/

			this.Reason = reason;
		}

		/// <summary>
		/// If the player can join
		/// </summary>
		public bool CanJoin { get; }

		/// <summary>
		/// The reason why they can't join.
		/// Set to null if they can join.
		/// </summary>
		public string Reason { get; }
	}

	internal static partial class Helper {

		/// <summary>
		/// Uses reflection to get the RoomTypeAttribute and return the value of it.
		/// </summary>
		/// <returns>Null if it's not set - which is odd.</returns>
		public static string GetRoomType<TPlayer>(this Room<TPlayer> room)
			where TPlayer : Player, new() {
			var attribute = room.GetType().GetCustomAttributes(typeof(RoomTypeAttribute), true).FirstOrDefault() as RoomTypeAttribute;

			return attribute?.RoomType;
		}
	}
}