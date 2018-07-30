using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WSIO.Messages;
using WSIO.Messages.v1;

namespace WSIO
{
    public class Message : IEnumerable<object>
    {
		//TODO: not be gay ( use Google.Protobuf official library & 
		
		static Message() {
			Constructor();
		}

		public static void Constructor() {
			MessageItemTypes = new Dictionary<uint, Type> {
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
			};

			MessageItemTypeIds = Swap(MessageItemTypes);
		}

		internal static Dictionary<uint, Type> MessageItemTypes = null;
		internal static Dictionary<Type, uint> MessageItemTypeIds = null;

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

		internal static Tuple<Type[], object[]> Deserialize(List<MessageItem> items) {
			var typs = new List<Type>();
			var itms = new List<object>();

			foreach(var i in items) {
				var obj = default(object);
				
				if(MessageItemTypes.TryGetValue(i.Type, out var type)) {
					
				}

				if(type != default(Type)) {
					typs.Add(type);
					itms.Add(i.Value());
				}
			}

			return Tuple.Create(typs.ToArray(), itms.ToArray());
		}

		public IEnumerator<object> GetEnumerator() => ((IEnumerable<object>)this._items).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<object>)this._items).GetEnumerator();

		public string Type { get; }


		private Type[] _types { get; }
		private object[] _items { get; }
    }
}
