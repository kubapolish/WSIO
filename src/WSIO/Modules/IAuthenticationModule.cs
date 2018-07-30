using Fleck;
using System;
using System.Collections.Generic;
using System.Text;
using WSIO.Authentication;

namespace WSIO.Authentication {
	public interface IPassword {
		string Password { get; set; }
	}

	internal struct PasswordStruct : IPassword {
		public PasswordStruct(string password) {
			this.Password = password;
		}

		public string Password { get; set; }
	}

	public interface ICredentials : IPassword {
		string Email { get; }
		string Username { get; }
		// IPassword // string Password { get; }
		IWebSocketConnection Socket { get; }
	}

	internal struct Credentials : ICredentials {
		public Credentials(string u, IWebSocketConnection socket, string p, string e = null) {
			this.Username = u;
			this.Socket = socket;
			this.Password = p;
			this.Email = e;
		}

		public string Email { get; }
		public string Username { get; }
		public IWebSocketConnection Socket { get; }
		public string Password { get; set; }
	}

	public interface IAuthModule {
		bool Login(ICredentials credentials, out IAuthToken token);
		bool Register(ICredentials credentials, out IAuthToken token);
	}

	public interface IAuthToken {
		string Username { get; }
	}

	public interface IAuthResult {
		bool Successful { get; }
		string Error { get; }
	}
}

namespace WSIO {
	internal static partial class Helper {
		public static IPassword Hash(this IPassword password) {
			var ps = new PasswordStruct(password.Password);

			ps.Password = BCrypt.Net.BCrypt.HashPassword(ps.Password);

			return ps;
		}
	}
}