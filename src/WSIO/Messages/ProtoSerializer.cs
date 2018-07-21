using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

		public static async Task Write<T>(T msg, WebSocket ws)
			where T : ProtoMessage {
			using (var ms = new MemoryStream()) {
				Serializer.Serialize<T>(ms, msg);
				await Write(ms.ToArray(), ws);
			}
		}

		public static async Task Write(byte[] data, WebSocket ws) {
			var writer = ws.CreateMessageWriter(WebSocketMessageType.Binary);
			await writer.WriteAsync(data, 0, data.Length);
			await writer.FlushAsync();
		}

		public static T CreateInstance<T>(this T msg)
			where T : ProtoMessage
			=> ProtoMessage.Create<T>();

		//TODO: put protocol generators in a custom class

		public static v1.SuccessState GetV1Success(bool state, string reason = null) {
			var instance = new v1.SuccessState().CreateInstance();

			instance.State = state;
			instance.Reason = reason;

			return instance;
		}
	}
}