using ProtoBuf;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using WSIO.Protobuf;
using WSIO.Protocol;

namespace WSIO {

	public class Message : ICollection<object> {
		internal ProtoSerializedMessage _message;

		public Message(string type, params object[] args)
					: this(ProtoSerializedMessage.Create(type, args)) { }

		public static Message Create(string type, params object[] args)
			=> new Message(type, args);

		internal Message(ProtoSerializedMessage message)
			=> this._message = message;

		public int Count => this._message.Items.Count;
		public bool IsReadOnly => false;

		public string Type => this._message.Type;

		public object this[uint index] {
			get {
				if (index >= this._message.Items.Count)
					throw new IndexOutOfRangeException(nameof(index));

				return this._message.Items[(int)index].Value;
			}
		}

		public void Add(object item) => this._message.Items.Add(new MessageItem(item));

		public void Clear() => this._message.Items.Clear();

		public bool Contains(object item) => this._message.Items.Contains(new MessageItem(item));

		public void CopyTo(object[] array, int arrayIndex) => GetValues().CopyTo(array, arrayIndex);

		public IEnumerator<object> GetEnumerator() => GetValues().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetValues().GetEnumerator();

		public bool Remove(object item) => this._message.Items.Remove(new MessageItem(item));

		private List<object> GetValues() {
			var objs = new List<object>();
			foreach (var i in this._message.Items)
				objs.Add(i.Value);
			return objs;
		}
	}

	//TODO: hide the proto members for message behind the creation of a message
	//TODO: the thing above
	//TODO: the thing above it's really important
	//TODO: it's really important
	//TODO: don't delay
	//TODO: i said don't delay

	[ProtoContract]
	public class MessageItem {

		public MessageItem() {
		}

		//TODO: not this
		// this.Type = 1; e.t.c is bad hardcoding
		// maybe i need to do it, maybe i don't
		// i'll find out later

		public MessageItem(object j) {
			if (j is double d)
				this.Set(d);
			else if (j is float f)
				this.Set(f);
			else if (j is int i)
				this.Set(i);
			else if (j is uint ui)
				this.Set(ui);
			else if (j is long l)
				this.Set(l);
			else if (j is ulong ul)
				this.Set(ul);
			else if (j is bool b)
				this.Set(b);
			else if (j is string s)
				this.Set(s);
			else if (j is byte[] barr)
				this.Set(barr);
			else if (j is MessageItem m)
				this.Set(m);
			else throw new Exception($"Unsupported type: {j.GetType()}");
		}

		public MessageItem(double val)
			=> Set(val);

		public MessageItem(float val)
			=> Set(val);

		public MessageItem(int val)
			=> Set(val);

		public MessageItem(long val)
			=> Set(val);

		public MessageItem(uint val)
			=> Set(val);

		public MessageItem(ulong val)
			=> Set(val);

		public MessageItem(bool val)
			=> Set(val);

		public MessageItem(string val)
			=> Set(val);

		public MessageItem(byte[] val)
			=> Set(val);

		public MessageItem(MessageItem val)
			=> this.Set(val);

		[ProtoMember(1, IsRequired = true)]
		public int Type { get; set; }

		[ProtoMember(2, IsRequired = false)]
		public double DoubleValue { get; set; }

		[ProtoMember(3, IsRequired = false)]
		public float FloatValue { get; set; }

		//TODO: does DataFormat really need to be set?
		[ProtoMember(4, IsRequired = false, DataFormat = DataFormat.Default)]
		public int IntValue { get; set; }

		[ProtoMember(5, IsRequired = false, DataFormat = DataFormat.Default)]
		public long LongValue { get; set; }

		[ProtoMember(6, IsRequired = false, DataFormat = DataFormat.Default)]
		public uint UIntValue { get; set; }

		[ProtoMember(7, IsRequired = false, DataFormat = DataFormat.Default)]
		public ulong ULongValue { get; set; }

		[ProtoMember(8, IsRequired = false)]
		public bool BoolValue { get; set; }

		[ProtoMember(9, IsRequired = false)]
		public string StringValue { get; set; }

		[ProtoMember(10, IsRequired = false)]
		public byte[] ByteArrayValue { get; set; }

		[ProtoMember(11, IsRequired = false)]
		public MessageItem InnerMessageItem { get; set; }

		/// <summary>
		/// Returns whatever the value is as an object
		/// </summary>
		[ProtoIgnore]
		public object Value {
			get {
				//TODO: whoah this is really ugly
				// i could've just now missed a message due to the unfortunate hardcoding that this is
				// maybe we could use a dictionary or something for type values?
				// or reflection?
				// or generics?
				// or both?
				//
				// maybe i have a problem

				switch (this.Type) {
					case 1: return this.DoubleValue;
					case 2: return this.FloatValue;
					case 3: return this.IntValue;
					case 4: return this.LongValue;
					case 5: return this.UIntValue;
					case 6: return this.ULongValue;
					case 7: return this.BoolValue;
					case 8: return this.StringValue;
					case 9: return this.ByteArrayValue;
					case 10: return this.InnerMessageItem;

					default: return null;
				}
			}
		}

		public void Set(double val) {
			this.Type = 1;
			this.DoubleValue = val;
		}

		public void Set(float val) {
			this.Type = 2;
			this.FloatValue = val;
		}

		public void Set(int val) {
			this.Type = 3;
			this.IntValue = val;
		}

		public void Set(long val) {
			this.Type = 4;
			this.LongValue = val;
		}

		public void Set(uint val) {
			this.Type = 5;
			this.UIntValue = val;
		}

		public void Set(ulong val) {
			this.Type = 6;
			this.ULongValue = val;
		}

		public void Set(bool val) {
			this.Type = 7;
			this.BoolValue = val;
		}

		public void Set(string val) {
			this.Type = 8;
			this.StringValue = val;
		}

		public void Set(byte[] val) {
			this.Type = 9;
			this.ByteArrayValue = val;
		}

		private void Set(MessageItem val) {
			this.Type = 10;
			this.InnerMessageItem = val;
		}
	}

	/// <summary>
	/// A message. Can contain:
	/// - float
	/// - double
	/// - int
	/// - uint
	/// - long
	/// - ulong
	/// - bool
	/// - string
	/// - byte[]
	/// - MessageItem ( which can contain all of the above )
	/// Can't contain:
	/// - everything else
	/// </summary>
	[ProtoContract]
	[ProtocolVersion(1, 4)]
	public class ProtoSerializedMessage : IProtoMessage {

		//TODO: figure out if this is allowed to exist
		// right, protobuf deserialization ( maybe? )
		public ProtoSerializedMessage() {
		}

		/// <summary>Please see "Message.Create". It's less typing for you.</summary>
		public ProtoSerializedMessage(string type, params MessageItem[] args) {
			this.Type = type;
			this.Items.AddRange(args.ToList());
			this.ProtoDefs = new ProtocolDefinition {
				ProtocolVersion = 1,
				MessageType = 4
			};
		}

		[ProtoMember(3)]
		public List<MessageItem> Items { get; set; } = new List<MessageItem>();

		[ProtoMember(1)]
		public ProtocolDefinition ProtoDefs { get; set; }

		[ProtoMember(2, IsRequired = true)]
		public string Type { get; set; }

		/// <summary>
		/// Parses an object[] into a List of MessageItems so you don't have to
		/// </summary>
		public static ProtoSerializedMessage Create(string type, object[] args) {
			var msgItms = new List<MessageItem>();

			foreach (var j in args)
				msgItms.Add(new MessageItem(j));

			return new ProtoSerializedMessage(type, msgItms.ToArray());
		}
	}

	//TODO: method to change the value
}