using System;
using System.Collections.Generic;
using System.Text;
using WSIO.Messages;

namespace WSIO
{
    public class Message
    {
		//TODO: not be gay ( use Google.Protobuf official library & 

		internal static Dictionary<uint, Type> _cache = null;
		internal static Dictionary<Type, uint> _cache2 = null;
		internal static readonly Dictionary<Type, uint> MessageItemTypeIds = _cache2 ?? (_cache2 = Swap(MessageItemTypes));
		internal static readonly Dictionary<uint, Type> MessageItemTypes = _cache ?? (_cache = new Dictionary<uint, Type> {
			{1, typeof(double) },
			{2, typeof(float) },
			{3, typeof(int) },
			{4, typeof(long) },
			{5, typeof(uint) },
			{6, typeof(ulong) },
			{7, typeof(bool) },
			{8, typeof(string) },
			{9, typeof(byte[]) },
			{10, typeof(IMessageItem) },
		});

		private static Dictionary<TValue, TKey> Swap<TKey, TValue>(Dictionary<TKey, TValue> items) {
			var result = new Dictionary<TValue, TKey>();

			foreach (var i in items)
				result.Add(i.Value, i.Key);

			return result;
		}

		internal Message(IMessage msg) {
			this.Type = msg.Type;
			var res = Deserialize(msg.Items);

			this._types = res.Item1;
			this._items = res.Item2;
		}

		internal static List<IMessageItem> Serialize(object[] objs) {
			var msgs = new List<IMessageItem>();

			foreach(var i in objs) {
				msgs.Add(new Messages.v1.MessageItem(i));
			}

			return msgs;
		}

		internal static Tuple<Type[], object[]> Deserialize(List<IMessageItem> items) {
			var typs = new List<Type>();
			var itms = new List<object>();

			foreach(var i in items) {
				var type = default(Type);
				var obj = default(object);
				
				if(MessageItemTypes.TryGetValue(i.Type, out type)) {
					
				}

				if(type != default(Type)) {

					typs.Add(type);
					itms.Add(obj);
				}
			}

			return Tuple.Create(typs.ToArray(), itms.ToArray());
		}

		public string Type { get; }


		private Type[] _types { get; }
		private object[] _items { get; }
    }
}
