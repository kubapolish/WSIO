using Fleck;

using System;

namespace WSIO {

	internal enum TaskType {
		OnBinary = 0,
		OnOpen = 1,
		OnClose = 2,
		OnError = 3,
		OnMessage = 4,
	}

	// allows for less code when creating these tasks
	internal static partial class Helper {

		public static OnBinaryTask CreateBinaryTask(this IWebSocketConnection socket, byte[] data)
			=> new OnBinaryTask { Socket = socket, BinaryData = data };

		public static OnErrorTask CreateErrorTask(this IWebSocketConnection socket, Exception err)
			=> new OnErrorTask { Socket = socket, Error = err };

		public static OnMessageTask CreateMessageTask(this IWebSocketConnection socket, string msg)
			=> new OnMessageTask { Socket = socket, MessageData = msg };

		public static SynchronousTask CreateTask(this IWebSocketConnection socket, TaskType type)
									=> new SynchronousTask { Socket = socket, Type = type };
	}

	// different tasks have special requirements
	internal class OnBinaryTask : SynchronousTask {
		public byte[] BinaryData { get; set; }
		public override TaskType Type => TaskType.OnBinary;
	}

	internal class OnErrorTask : SynchronousTask {
		public Exception Error { get; set; }
		public override TaskType Type => TaskType.OnError;
	}

	internal class OnMessageTask : SynchronousTask {
		public string MessageData { get; set; }
		public override TaskType Type => TaskType.OnMessage;
	}

	/// <summary>
	/// Purpose:
	///		Make the multi-threadedness of WebSockets into a synchronous stack.
	/// </summary>
	internal class SynchronousTask {
		public IWebSocketConnection Socket { get; set; }
		public virtual TaskType Type { get; set; }
	}
}