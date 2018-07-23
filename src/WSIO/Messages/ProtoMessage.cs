using ProtoBuf;

using System;
using System.Reflection;

using WSIO.Attributes;

namespace WSIO.Messages {

	[ProtoContract]
	internal class SimpleIProtoMessageInheriter : IProtoMessage {

		[ProtoMember(1)]
		public ProtoMessage ProtoDefs { get; set; }
	}

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
			where T : IProtoMessage
		=> (T)Create(typeof(T));

		public static IProtoMessage Create(Type type) {
			var instance = (IProtoMessage)Activator.CreateInstance(type);
			if (!type.GetAttribute<MessageVersionAttribute>(out var attribute)) {
				throw new Exception($"Please put a MessageVersionAttribute on {type}");
			}

			instance.ProtoDefs = new ProtoMessage {
				MessageType = attribute.MessageType,
				ProtocolVersion = attribute.ProtocolVersion,
			};

			return instance;
		}

		public static bool FindTypeFor(IProtoMessage protocolDefinitions, out Type t) {
			foreach (var i in Assembly.GetAssembly(typeof(IProtoMessage)).GetTypes())
				if (i.GetAttribute<MessageVersionAttribute>(out var attribute))
					if (attribute.MessageType == protocolDefinitions.ProtoDefs.MessageType &&
						attribute.ProtocolVersion == protocolDefinitions.ProtoDefs.ProtocolVersion) {
						t = i;
						return true;
					}
			t = default(Type);
			return false;
		}
	}
}