using System;
using System.Net;
using System.Net.Sockets;

namespace WSIO.Tester
{
	public class MyPlayer : Player {

		public MyPlayer() => this.Regular = true;

		public bool Administrator { get; set; }
		public bool Moderator { get; set; }
		public bool Regular { get; set; }
	}

	[RoomType("blocks")]
	public class BlockRoom : Room<MyPlayer> {
		public override void OnCreation() => Console.WriteLine($"Creation! {this.RoomId} {this.RoomType}");

		public override void OnJoin(MyPlayer p) {
			Console.WriteLine($"{p.Username} joined!");
		}

		public override void OnMessage(MyPlayer p, Message msg) => Console.WriteLine($"{p.Username} sent a message!");

		public override void OnLeave(MyPlayer p) => Console.WriteLine($"{p.Username} left!");

		public override void OnDeletion() => Console.WriteLine($"Deletion! {this.RoomId} {this.RoomType}");
	}

    class Program
    {
        static void Main(string[] args) {
			/*
			using (var server = new Server<MyPlayer>(args, 80, typeof(BlockRoom))) {

				//var p = server.Generate(new PlayerRequest("John Doe", "password1", null));
				//var room = server.Send(p, new RoomRequest("wee fun", "blocks"));

				Console.WriteLine("Hello World!");
				Console.WriteLine(server.Location);
				Console.WriteLine(server.Port);
				server.Start();
				Console.ReadLine();
			}/**/
        }
    }
}
