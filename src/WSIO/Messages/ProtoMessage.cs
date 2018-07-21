using ProtoBuf;
using System;
using System.Reflection;
using WSIO.Attributes;

namespace WSIO.Messages {

	[ProtoContract]
	[ProtoInclude(3, typeof(v1.SuccessState))]
	[ProtoInclude(4, typeof(v1.Authentication))]
	[ProtoInclude(5, typeof(v1.Registration))]
	[ProtoInclude(6, typeof(v1.RoomRequest))]
	internal class ProtoMessage {
		[ProtoMember(1)]
		public uint ProtocolVersion { get; set; }

		[ProtoMember(2)]
		public uint MessageType { get; set; }
		
		public static T Create<T>()
			where T : ProtoMessage
		=> (T)Create(typeof(T));

		public static ProtoMessage Create(Type type) {
			var instance = (ProtoMessage)Activator.CreateInstance(type);
			if (!type.GetAttribute<MessageVersionAttribute>(out var attribute)) {
				throw new Exception($"Please put a MessageVersionAttribute on {type}");
			}

			instance.MessageType = attribute.MessageType;
			instance.ProtocolVersion = attribute.ProtocolVersion;

			return instance;
		}

		public static bool FindTypeFor(ProtoMessage protocolDefinitions, out Type t) {
			foreach (var i in Assembly.GetAssembly(typeof(ProtoMessage)).GetTypes())
				if (i.GetAttribute<MessageVersionAttribute>(out var attribute))
					if (attribute.MessageType == protocolDefinitions.MessageType &&
						attribute.ProtocolVersion == protocolDefinitions.ProtocolVersion) {
						t = i;
						return true;
					}
			t = default(Type);
			return false;
		}
	}
}