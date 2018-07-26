using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;
using WSIO.Attributes;

namespace WSIO.Messages.v1 {
	[ProtoContract]
	[MessageVersion(1, 5)]
	internal class Message : IMessage {

		[ProtoMember(1)]
		public ProtocolDefinition ProtoDefs { get; set; }

		[ProtoMember(2)]
		public string Type { get; set; }

		[ProtoMember(3)]
		public List<IMessageItem> Items { get; set; }
	}

	[ProtoContract]
	internal class MessageItem : IMessageItem {

		public MessageItem() {

		}

		public MessageItem(object i) {
			var t = i.GetType();
			if (!WSIO.Message.MessageItemTypeIds.TryGetValue(t, out var val)) throw new Exception($"Invalid object type {t}");

			this.Type = val;

			switch (this.Type) {
				case 1: {
					this.DoubleValues = AsList<double>(i);
				} break;

				case 2: {
					this.FloatValues = AsList<float>(i);
				} break;

				case 3: {
					this.IntValues = AsList<int>(i);
				} break;

				case 4: {
					this.LongValues = AsList<long>(i);
				} break;

				case 5: {
					this.UintValues = AsList<uint>(i);
				} break;

				case 6: {
					this.UlongValues = AsList<ulong>(i);
				} break;

				case 7: {
					this.BoolValues = AsList<bool>(i);
				} break;

				case 8: {
					this.StringValues = AsList<string>(i);
				} break;

				case 9: {
					this.ByteArrayValues = AsList<byte[]>(i);
				} break;

				case 10: {
					this.InnerMessageItems = AsList<IMessageItem>(i);
				} break;

				default: throw new Exception("Unexpected Case");
			}
		}

		private static List<T> AsList<T>(object i) {
			if (i.GetType().IsArray)
				return new List<T>((T[])i);
			else return new List<T>(new T[] { (T)i });
		}

		[ProtoMember(1)]
		public uint Type { get; set; }

		[ProtoMember(2)]
		public List<double> DoubleValues { get; set; }

		[ProtoMember(3)]
		public List<float> FloatValues { get; set; }

		[ProtoMember(4)]
		public List<int> IntValues { get; set; }

		[ProtoMember(5)]
		public List<long> LongValues { get; set; }

		[ProtoMember(6)]
		public List<uint> UintValues { get; set; }

		[ProtoMember(7)]
		public List<ulong> UlongValues { get; set; }

		[ProtoMember(8)]
		public List<bool> BoolValues { get; set; }
		
		[ProtoMember(9)]
		public List<string> StringValues { get; set; }

		[ProtoMember(10)]
		public List<byte[]> ByteArrayValues { get; set; }

		[ProtoMember(11)]
		public List<IMessageItem> InnerMessageItems { get; set; }

		[ProtoIgnore]
		public object Value() {

		}
	}
}
