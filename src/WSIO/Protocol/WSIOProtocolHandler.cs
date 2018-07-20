using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using WSIO.Protobuf;

namespace WSIO.Protocol {

	/// <summary>
	/// This class is used to handle different protocol versions of the server
	/// incase a client connects with an old version and needs to do something
	/// that the newer versions differ from. Hopefully, i won't need this
	/// functionality  - but it's implemented just in case :p
	/// </summary>
	public class WSIOProtocolHandler {

		// if you add protocol versions, make sure to change these!
		// ( increment the counter if a headache was caused )
		// times_caused_headache = 0;
		public const uint MAX_PROTOCOL = 1;

		public const uint MIN_PROTOCOL = 1;

		/// <summary>Stores a cache of the WSIOProtocolHandlers so we won't have to use reflection to get a WSIOProtocolHandler every time</summary>
		private static Dictionary<uint, WSIOProtocolHandler> _cache { get; set; } = new Dictionary<uint, WSIOProtocolHandler>();

		public static IProtoMessage GetProtoMessage(uint version, uint messageType) {
			if (IsInvalid(version)) return null;
			if (messageType == 0) return null;

			//TODO: implement cache for message types
			foreach (var type in Assembly.GetAssembly(typeof(ProtocolVersionAttribute)).GetTypes()) // for every type in this assembly
				foreach (var protocolVersionAttribute in type.GetCustomAttributes<ProtocolVersionAttribute>()) // if they have the protocol version attribute
					if (protocolVersionAttribute.Version == version && protocolVersionAttribute.MessageType == messageType) { // if it matches the version we need
						var protomessage = Activator.CreateInstance(type); // create an instance of it
						if (protomessage is IProtoMessage p) // pass it on
							return p;
					}

			return null;
		}

		/// <summary>
		/// Uses reflection to find the appropriate protocol handler for the version specified.
		/// </summary>
		/// <param name="version">The version of the protocol you want</param>
		/// <returns>A protocol associated with the version you want.</returns>
		public static WSIOProtocolHandler GetWSIOProtocolHandler(uint version) {
			if (IsInvalid(version)) return null;

			if (_cache.TryGetValue(version, out var res)) return res; // so we don't have to do assembly searching every time

			foreach (var type in Assembly.GetAssembly(typeof(ProtocolHandlerAttribute)).GetTypes()) // for every type in this assembly
				foreach (var protocolVersionAttribute in type.GetCustomAttributes<ProtocolHandlerAttribute>()) // protocol versions supported by this
					if (protocolVersionAttribute.Version == version) { // if it matches the version we need
						var attributeFromType = Activator.CreateInstance(type); // create an instance of it
						if (attributeFromType is WSIOProtocolHandler protocolHandler) { // if it's a protocol handler
							_cache[version] = protocolHandler;
							return protocolHandler; // pass it on
						}
					}

			_cache[version] = null;

			return null; //unable to find any
		}

		/// <summary>
		/// Checks if a protocol version is valid.
		/// </summary>
		/// <param name="version">The version</param>
		/// <returns>true if it's within the range of MIN_PROTOCOL and MAX_PROTOCOL</returns>
		public static bool IsInvalid(uint version)
			=> version < MIN_PROTOCOL || version > MAX_PROTOCOL;

		//EDIT: it exists for the creation of new protobuf things ( the protocol ) /// Might want to reconsider it's very existence.

		/// <summary>
		/// Throws an exception instead of returning false like IsInvalid does.
		/// </summary>
		/// <param name="version">The version</param>
		public static void ThrowExceptionsIfInvalid(uint version) {
			if (version < MIN_PROTOCOL) throw new IndexOutOfRangeException($"{nameof(version)} conflicts with {nameof(MIN_PROTOCOL)} ({MIN_PROTOCOL})");
			if (version > MAX_PROTOCOL) throw new IndexOutOfRangeException($"{nameof(version)} conflicts with {nameof(MAX_PROTOCOL)} ({MAX_PROTOCOL})");
		}

		//TODO: simplify this - it's getting messily complicated
		/// <summary>
		/// Handle some OnBinary event
		/// </summary>
		/// <returns>
		/// Returns a `Message` that you need to send to the TPlayer's room,
		/// or null if it's handleing some internal function.
		/// </returns>
		public virtual ProtoSerializedMessage Handle<TPlayer>(Server<TPlayer> server, TPlayer p, out TPlayer changed, ProtocolDefinition def, Stream data)
			where TPlayer : Player, new() {
			changed = default(TPlayer);
			return null;
		}
	}
}