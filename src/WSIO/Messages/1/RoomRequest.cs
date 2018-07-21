﻿using ProtoBuf;
using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 4)]
	internal class RoomRequest : ProtoMessage {
		[ProtoMember(3)]
		public string RoomType { get; set; }

		[ProtoMember(4)]
		public string Roomid { get; set; }
	}
}