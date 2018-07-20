using System;

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

	}

    class Program
    {
        static void Main()
        {
			using (var server = new Server<MyPlayer>(typeof(BlockRoom))) {

				var p = server.Generate(new PlayerRequest("John Doe", "password1", null));
				var room = server.Send(p, new RoomRequest("wee fun", "blocks"));

				Console.WriteLine("Hello World!");
				Console.ReadLine();
			}
        }
    }
}
