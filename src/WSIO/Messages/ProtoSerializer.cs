using Fleck;

using ProtoBuf;

using System.IO;

namespace WSIO.Messages {

	internal static class ProtoSerializer {

		public static bool Deserialize<T>(Stream data, out T result)
			where T : ProtoMessage {
			var success = false;

			try {
				result = Serializer.Deserialize<T>(data);
				success = true;
			} catch (ProtoException) {
				result = default(T);
			}

			if (data.CanSeek)
				data.Seek(0, SeekOrigin.Begin);

			return success;
		}

		public static byte[] Serialize<T>(T item)
			where T : ProtoMessage {
			using (var ms = new MemoryStream()) {
				//TODO: try catch protoexception?
				Serializer.Serialize<T>(ms, item);
				return ms.ToArray();
			}
		}

		public static T CreateInstance<T>(this T msg)
			where T : ProtoMessage
			=> ProtoMessage.Create<T>();

		private static V1Handler _v1 = null;
		public static MessageHandler V1 => _v1 ?? (_v1 = new V1Handler());

		public static void Handle<TPlayer>(TPlayer player, RoomManager<TPlayer> rooms, MemoryStream ms)
			where TPlayer : Player, new() {
			//TODO: handle different protocol versions

			// :p

			V1.Handle(player, rooms, ms);
		}
	}
}