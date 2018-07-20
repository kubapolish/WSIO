using System;
using System.Collections.Generic;
using System.Drawing;

namespace WSIO.Example
{
	public class MyPlayer : Player {
		public int Counter { get; set; }
	}

	[RoomType("blockroom")]
	public class BlockRoom : Room<MyPlayer> {
		public override RoomRequestAnswer CanJoin(MyPlayer p)
			=> new RoomRequestAnswer(true);

		// this is just a demo and i'm lazy, pls don't - dear reader - take this as professional code, or die D:
		public bool[,] BlockWorld = new bool[16,16];

		public override void OnJoin(MyPlayer p) {
			Console.WriteLine($"{p.Username} joined!");
			for (int i = 0; i < 16; i++)
				for (int j = 0; j < 16; j++)
					if(BlockWorld[i, j])
						p.Send("block", i, j, BlockWorld[i, j]);
		}

		public override void OnMessage(MyPlayer p, ProtoSerializedMessage m) {
			if (m.Type == "block") {
				if (m.Items.Count > 1) {
					//TODO: Message.Get<> feature
					var x = Convert.ToUInt32(m.Items[0].Value);
					var y = Convert.ToUInt32(m.Items[1].Value);

					if (x < BlockWorld.GetLength(0) && y < BlockWorld.GetLength(1)) {
						BlockWorld[x, y] = !BlockWorld[x, y];

						Broadcast("block", x, y, BlockWorld[x, y]);
					}
				} else {
					p.Send("error", "you didn't send the right amount of stuff D:");
				}
			} else if (m.Type == "kick") p.Kick("dead");
		}

		public override void OnCreation()
			=> Console.WriteLine($"Room Created!");

		public override void OnClose()
			=> Console.WriteLine($"Room Closed!");

		public override void OnLeave(MyPlayer p)
			=> Console.WriteLine($"Player left {p.Username}");
	}

	// demo room
	[RoomType("myroom")]
	public class MyRoom : Room<MyPlayer> {
		public int startedUpTimes;

		public override RoomRequestAnswer CanJoin(MyPlayer p)
			=> new RoomRequestAnswer(true);

		public override void OnCreation()
			=> Console.WriteLine($"Room Created! ({this.startedUpTimes++})");

		public override void OnClose()
			=> Console.WriteLine($"Room Closed!");

		public override void OnJoin(MyPlayer p) {
			Console.WriteLine($"{p.Username} joined!");

			p.Send("Hello, World!");
			p.Send("welcome", "we hope you enjoy your stay!");
		}

		public override void OnLeave(MyPlayer p)
			=> Console.WriteLine($"{p.Username} left!");

		public override void OnMessage(MyPlayer p, ProtoSerializedMessage m) {
			Console.WriteLine($"\r\n{p.Username} sent a message!");

			Console.WriteLine(m.Type);
			foreach (var i in m.Items) Console.WriteLine(i.Value);
		}
	}
	
	class Program
    {
        static void Main()
        {
			var dbchoice = new MongoDB<MyPlayer>("mongodb://localhost:27017", "demo");
			//var dbchoice = new DictionaryDatabase<MyPlayer>();

			using (var server = new Server<MyPlayer>("ws://0.0.0.0:80/ws", dbchoice, new Type[] { typeof(MyRoom), typeof(BlockRoom) })) {
				Console.ReadLine();
			}
        }
    }
}
