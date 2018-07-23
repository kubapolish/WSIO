﻿using ProtoBuf;

using WSIO.Attributes;

namespace WSIO.Messages.v1 {

	[ProtoContract]
	[MessageVersion(1, 1)]
	internal class SuccessState : IProtoMessage, ISuccessState {

		[ProtoMember(1)]
		public ProtoMessage ProtoDefs { get; set; }

		[ProtoMember(2)]
		public bool State { get; set; }

		[ProtoMember(3)]
		public string Reason { get; set; }
	}
}