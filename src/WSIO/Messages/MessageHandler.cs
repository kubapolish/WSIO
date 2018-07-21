using Fleck;

using System;
using System.IO;

namespace WSIO.Messages {

	internal interface IMessageHandler {

		void Handler(IWebSocketConnection ws, Stream data);
	}

	internal abstract class MessageHandler {

		public abstract void Handle(IWebSocketConnection ws, Stream data);
	}

	internal class V1Handler : MessageHandler {

		public v1.SuccessState GetSuccess(bool state, string reason = null) {
			var instance = new v1.SuccessState().CreateInstance();

			instance.State = state;
			instance.Reason = reason;

			return instance;
		}

		public override async void Handle(IWebSocketConnection ws, Stream data) {
			if (!ProtoSerializer.Deserialize<ProtoMessage>(data, out var res)) {
				await ws.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to deserialize your message.")));
				return;
			}

			if (!ProtoMessage.FindTypeFor(res, out var type)) {
				await ws.Send(ProtoSerializer.Serialize(GetSuccess(false, "Unable to find the type for your message.")));
				return;
			}

			switch (Activator.CreateInstance(type)) {
				case IAuthentication auth: {
					await ws.Send("w e e");
				}
				break;

				default: break;
			}
		}
	}
}