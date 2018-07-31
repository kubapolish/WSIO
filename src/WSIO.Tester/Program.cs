using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using WSIO.Authentication;

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

		public override void OnMessage(MyPlayer p, Message msg) {
			Console.WriteLine($"{p.Username} sent a message!");
			Console.WriteLine(msg.Type);
			foreach (var i in msg)
				if (i is Array)
					foreach (var j in i as Array)
						Console.WriteLine(j);
				else Console.WriteLine(i);
		}

		public override void OnLeave(MyPlayer p) => Console.WriteLine($"{p.Username} left!");

		public override void OnDeletion() => Console.WriteLine($"Deletion! {this.RoomId} {this.RoomType}");
	}

	public class AnyAuthenticator : IAuthModule {
		public Dictionary<string, DefaultPassword> _creds = new Dictionary<string, DefaultPassword>();

		public IAuthResult Login(Authentication.ICredentials credentials, out IAuthToken token) {
			Console.WriteLine("login");
			if(_creds.TryGetValue(credentials.Username, out var p)) {
				Console.WriteLine("found val");
				if (credentials.Password.Equals(p)) {
					Console.WriteLine("eq");
					token = new AuthToken(credentials.Username);
					return new AuthResult(true);
				}
			}
			Console.WriteLine("login FAIL");

			token = default(IAuthToken);
			return new AuthResult("Unable to find user.");
		}
		public IAuthResult Register(Authentication.ICredentials credentials, out IAuthToken token) {
			Console.WriteLine("register");
			if (_creds.TryGetValue(credentials.Username, out var p)) {
				Console.WriteLine("found");
				token = default(IAuthToken);
				return new AuthResult("User exists.");
			}
			Console.WriteLine("hashing");

			credentials.Password.Hash();
			_creds[credentials.Username] = (DefaultPassword)credentials.Password;

			token = new AuthToken(credentials.Username);
			return new AuthResult(true);
		}
	}

	public class AuthToken : IAuthToken {
		public AuthToken(string username) {
			this.Username = username;
		}

		public string Username { get; }
	}

	class Program
    {
        static void Main(string[] args) {

			var auth = new AnyAuthenticator();

			if (System.IO.File.Exists("accs.json"))
				auth._creds = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, DefaultPassword>>(System.IO.File.ReadAllText("accs.json"));

			using (var server = new Server<MyPlayer>(args, auth, 80, typeof(BlockRoom))) {

				//var p = server.Generate(new PlayerRequest("John Doe", "password1", null));
				//var room = server.Send(p, new RoomRequest("wee fun", "blocks"));

				Console.WriteLine("Hello World!");
				Console.WriteLine(server.Location);
				Console.WriteLine(server.Port);
				server.Start();
				Console.ReadLine();
			}/**/

			System.IO.File.WriteAllText("accs.json", Newtonsoft.Json.JsonConvert.SerializeObject(auth._creds));
        }
    }
}
