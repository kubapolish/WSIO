using System;
using System.Linq;

using WSIO.Protobuf;

namespace WSIO.Protocol {

	public sealed class ProtocolVersionAttribute : Attribute {
		public const uint MAX_PROTOCOL = WSIOProtocolHandler.MAX_PROTOCOL;
		public const uint MIN_PROTOCOL = WSIOProtocolHandler.MIN_PROTOCOL;

		public ProtocolVersionAttribute(uint version, uint messageType) {
			WSIOProtocolHandler.ThrowExceptionsIfInvalid(version);

			this.Version = version;
			this.MessageType = messageType;
		}

		/// <summary>
		/// The type of message this is for
		/// </summary>
		public uint MessageType { get; }

		/// <summary>
		/// The version that this is for
		/// </summary>
		public uint Version { get; }
	}

	internal static partial class Helper {

		/// <summary>
		/// Gets the uint associated with the message for whatever type it is
		/// </summary>
		/// <typeparam name="T">The message we want</typeparam>
		/// <returns>The type as a uint, or 0 if we couldn't find it.</returns>
		public static uint GetMessageType<T>()
			where T : IProtoMessage
			=> !(typeof(T).GetCustomAttributes(typeof(ProtocolVersionAttribute), true).FirstOrDefault() is ProtocolVersionAttribute attribute) ?
				0 // should never happen
				: attribute.MessageType;
	}
}