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
		public List<MessageItem> Items { get; set; }
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
		
		public object Value() {
			switch(Type) {
				case 1: {
					if (this.DoubleValues.Count < 2)
						return this.DoubleValues[0];
					else return this.DoubleValues.ToArray();
				}
				case 2: {
					if (this.FloatValues.Count < 2)
						return this.FloatValues[0];
					else return this.FloatValues.ToArray();
				}
				case 3: {
					if (this.IntValues.Count < 2)
						return this.IntValues[0];
					else return this.IntValues.ToArray();
				}
				case 4: {
					if (this.LongValues.Count < 2)
						return this.LongValues[0];
					else return this.LongValues.ToArray();
				}
				case 5: {
					if (this.UintValues.Count < 2)
						return this.UintValues[0];
					else return this.UintValues.ToArray();
				}
				case 6: {
					if (this.UlongValues.Count < 2)
						return this.UlongValues[0];
					else return this.UlongValues.ToArray();
				}
				case 7: {
					if (this.BoolValues.Count < 2)
						return this.BoolValues[0];
					else return this.BoolValues.ToArray();
				}
				case 8: {
					if (this.StringValues.Count < 2)
						return this.StringValues[0];
					else return this.StringValues.ToArray();
				}
				case 9: {
					if (this.ByteArrayValues.Count < 2)
						return this.ByteArrayValues[0];
					else return this.ByteArrayValues.ToArray();
				}
				case 10: {
					if (this.InnerMessageItems.Count < 2)
						return this.InnerMessageItems[0];
					else return this.InnerMessageItems.ToArray();
				}
				default: throw new Exception();
			}
		}
	}
}
