using System;

namespace WSIO.Protocol {

	/// <summary>
	/// Signals that "Hey, I can handle this version of the protocol!"
	/// </summary>
	public sealed class ProtocolHandlerAttribute : Attribute, IProtocolVersion {

		public ProtocolHandlerAttribute(uint version) {
			WSIOProtocolHandler.ThrowExceptionsIfInvalid(version);

			this.Version = version;
		}

		/// <summary>
		/// The version that it manages
		/// </summary>
		public uint Version { get; }
	}
}