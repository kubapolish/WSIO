using System;

namespace WSIO {

	/// <summary>
	/// Use this attribute to define the RoomType of a room.
	/// Example:
	///
	/// [RoomType("MyRoom")]
	/// public class WhateverRoom : Room...
	///
	/// Now you just need to specify any 'RoomType' as "MyRoom" to connect to it.
	/// </summary>
	public sealed class RoomTypeAttribute : Attribute {

		public RoomTypeAttribute(string type) => this.RoomType = type;

		public string RoomType { get; }
	}
}