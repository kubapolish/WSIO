using System;
using System.Collections.Generic;
using System.Text;
using TimedQuery;

namespace WSIO
{
	/*
	/// <summary>
	/// Not to be used in production use.
	/// Simply acts as something to add stuff onto the console.
	/// </summary>
    public static class Logger
    {
		private static QueryExecutioner<string> _console;
		private static List<string> _itms;

		static Logger() {
			_itms = new List<string>();
			new System.Threading.Thread(() => {
				while(true) { // lazy way to just get the stuff on the console and process it
					if (_itms.Count > 0) {
						_console.AddQueryItem(_itms[0]);
						_itms.RemoveAt(0);
					} else System.Threading.Thread.Sleep(1000);
				}
			}).Start();
			_console = new QueryExecutioner<string>();
			_console.ProcessQueryItem += _console_ProcessQueryItem;
		}

		private static void _console_ProcessQueryItem(string item) => Console.WriteLine(item);

		public static void WriteLine(string msg) {
			_itms.Add(msg);
		}
    }
	*/
}
