using Fleck;
using System;
using System.Collections.Generic;
using System.Text;
using WSIO.Authentication;

namespace WSIO.Authentication {
	public interface IPassword : IEquatable<IPassword> {
		string Password { get; set; }
		bool Hashed { get; set; }
		void Hash();
	}

	internal struct PasswordStruct : IPassword {
		public PasswordStruct(string password) {
			this.Password = password;
			this.Hashed = false;
		}

		public bool Hashed { get; set; }
		public string Password { get; set; }

		public bool Equals(IPassword other) {
			if (this.Hashed && !other.Hashed) {
				return BCrypt.Net.BCrypt.Verify(other.Password, this.Password);
			} else if (!this.Hashed && other.Hashed) {
				return BCrypt.Net.BCrypt.Verify(this.Password, other.Password);
			} else {
				// u h o h D :
				return false;
			}
		}

		public void Hash() {
			if(!this.Hashed) {
				this.Password = BCrypt.Net.BCrypt.HashPassword(this.Password);

				this.Hashed = true;
			}
		}
	}

	public interface ICredentials {
		string Email { get; }
		string Username { get; }
		IPassword Password { get; }
		IWebSocketConnection Socket { get; }
	}

	internal struct Credentials : ICredentials {
		public Credentials(string u, IWebSocketConnection socket, IPassword p, string e = null) {
			this.Username = u;
			this.Socket = socket;
			this.Password = p;
			this.Email = e;
		}

		public string Email { get; }
		public string Username { get; }
		public IWebSocketConnection Socket { get; }
		public IPassword Password { get; set; }
	}

	public interface IAuthModule {
		IAuthResult Login(ICredentials credentials, out IAuthToken token);
		IAuthResult Register(ICredentials credentials, out IAuthToken token);
	}

	public interface IAuthToken {
		string Username { get; }
	}

	public interface IAuthResult {
		bool Successful { get; }
		string Error { get; }
	}

	public class AuthResult : IAuthResult {
		public AuthResult(bool status, string error = null) {
			this.Successful = status;
			this.Error = error;
		}

		public AuthResult(string error = null) {
			if (error == null)
				this.Successful = true;
			else {
				this.Successful = false;
				this.Error = error;
			}
		}

		public bool Successful { get; }
		public string Error { get; }
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