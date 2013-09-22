using System;
using System.Net;

namespace Sabertooth.Lexicon {
	public class ClientRequest {
		public string Type;
		public string Path;
		public string Host;
		public Statement[] Arguments;
		public IPAddress IP;
		public readonly TimeTracker RequestTime;
		public ClientRequest() {
			RequestTime = new TimeTracker ();
		}
		public ClientRequest(string type, string path) {
			Type = type;
			Path = path;
			RequestTime = new TimeTracker ();
		}
	}
}

