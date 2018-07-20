using System;

namespace WSIO {

	public class RoomRequest : IManagerRequest {

		public RoomRequest(string roomId, string roomType) {
			this.RoomId = roomId ?? throw new ArgumentNullException($"{nameof(roomId)} cannot be null");
			this.RoomType = roomType ?? throw new ArgumentNullException($"{nameof(roomType)} cannot be null.");
		}

		public string RoomId { get; }
		public string RoomType { get; }

		public bool Equals(IManagerRequest other)
			=> other is RoomRequest otherReq ?
				(otherReq.RoomId == this.RoomId &&
				otherReq.RoomType == this.RoomType)
				: false;
	}
}