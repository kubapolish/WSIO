using System;

namespace WSIO {

	public sealed class RoomTypeAttribute : Attribute {

		public RoomTypeAttribute(string roomType) => this.RoomType = roomType;

		public string RoomType { get; }
	}
}