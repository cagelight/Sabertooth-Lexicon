using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

using WebSharp;

namespace Sabertooth.Lexicon {
	public class ClientRequest {
		public readonly IPAddress Address;
		protected List<string> headerList;
		protected string type; public string Type { get{return type;} }
		protected string path; public string Path { get{return path;} }
		protected int contentlength = 0; public int ContentLength { get {return contentlength;} }
		protected MIME contenttype ; public MIME ContentType { get{return contenttype;} }
		protected string boundaryMultipart = String.Empty;
		protected string host; public string Host {get {return host;}}
		protected string[] sphost;
		public string TLD {get {IPAddress g; return IPAddress.TryParse(host, out g) ? String.Empty : sphost [0];}}
		public string Domain {get {IPAddress g; return IPAddress.TryParse(host, out g) ? String.Empty : sphost [1];}}
		public string Subdomain {get {IPAddress g; return IPAddress.TryParse(host, out g) || sphost.Length < 3 ? String.Empty : sphost [2];}}
		public Dictionary<string,string> Arguments = new Dictionary<string, string> ();
		public Dictionary<string,string> headerDict = new Dictionary<string, string> ();
		public Tuple<string, string> Authorization { get {
				string b;
				if (!headerDict.TryGetValue ("Authorization", out b) || !b.StartsWith("Basic ")) {
					return new Tuple<string, string> (String.Empty, String.Empty);
				}
				string[] auth = Encoding.UTF8.GetString(Convert.FromBase64String (b.Substring(6))).Split(':');
				if (auth.Length < 2)
					return new Tuple<string, string> (String.Empty, String.Empty);
				return new Tuple<string, string> (auth[0], auth[1]);
			}}
		public readonly TimeTracker RequestTime;
		public string Header { get { return String.Join ("\r\n", this.headerList) + "\r\n\r\n"; } }
		protected BinaryReader Reader;
		public ClientRequest(BinaryReader S, IPAddress source) {
			this.Address = source;
			this.Reader = S;
			this.headerList = new List<string> ();
			string line = String.Empty;
			char cb;
			while (true) {
				while (true) {
					cb = S.ReadChar ();
					if (cb == '\n')
						break;
					if (cb == '\r')
						continue;
					line += cb;
				}
				if (line == String.Empty) {
					break;
				} else {
					headerList.Add (String.Copy (line));
					line = String.Empty;
				}
			}
			this.RequestTime = new TimeTracker ();
			string[] requestLine = headerList [0].Split (' ');
			this.type = requestLine[0];
			string decpath = HttpUtility.UrlDecode (requestLine[1]); 
			int argindex = decpath.IndexOf ('?');
			if (argindex == -1 || argindex == decpath.Length-1) {
				this.path = decpath;
			} else {
				string[] rawargs = decpath.Substring(argindex+1).Split('&');
				this.path = decpath.Substring(0,argindex);
				foreach(string kvp in rawargs) {
					if (kvp.Length < 3) {continue;}
					string[] splitargs = kvp.Split('=');
					if (splitargs.Length != 2) {continue;}
					this.Arguments[splitargs[0]] = splitargs[1];
				}
			}
			foreach (string h in headerList) {
				string[] lineInstruction = h.Split(new string[] {": "}, 2, StringSplitOptions.None);
				if (lineInstruction.Length > 1) {
					this.headerDict [lineInstruction [0]] = lineInstruction [1];
				}
				switch(lineInstruction[0]) {
				case "Content-Type":
					string boundary;
					this.contenttype = MIME.FromText (lineInstruction [1], out boundary);
					if (boundary != String.Empty)
						this.boundaryMultipart = boundary;
					break;
					case "Content-Length":
					this.contentlength = Convert.ToInt32 (lineInstruction [1]);
					break;
				case "Host":
					this.host = lineInstruction [1];
					this.sphost = this.host.Split ('.');
					Array.Reverse (sphost);
					break;
				}
			}
			//Console.WriteLine (this.Header);
		}

		public ClientBody ReadBody() {
			ClientBody CB = new ClientBody ();
			if (this.ContentLength > 0) {
				byte[] bodybytes = Reader.ReadBytes (this.ContentLength);
				byte[] boundarybytes = Encoding.UTF8.GetBytes ("--"+this.boundaryMultipart);
				List<int> indexes = new List<int> ();
				int ct = 0;
				for (int i = 0; i < bodybytes.Length; ++i) {
					if (bodybytes[i] == boundarybytes[ct]) {
						++ct;
						if (ct == boundarybytes.Length) {
							indexes.Add (i);
							ct = 0;
						}
					} else {
						ct = 0;
					}
				}
				List<byte[]> parts = new List<byte[]> (indexes.Count - 1);
				for(int i = 0; i < indexes.Count - 1; ++i) {
					int range = (indexes [i+1] - indexes [i]) - (boundarybytes.Length + 4);
					byte[] part = new byte[range];
					Buffer.BlockCopy (bodybytes, indexes[i]+3, part, 0, range);
					parts.Add (part);
				}
				foreach (byte[] part in parts) {
					List<string> headerLines = new List<string> ();
					int index = 0;
					string line = String.Empty;
					char cb;
					do {
						cb = Convert.ToChar(part[index]);
						if (cb == '\r')
							continue;
						if (cb == '\n') {
							if (line == String.Empty)
								break;
							headerLines.Add(String.Copy(line));
							line = String.Empty;
						} else {
							line += cb;
						}
					} while (index++ < part.Length);
					byte[] partbody = new byte[part.Length - index - 1];
					Buffer.BlockCopy (part, index+1, partbody, 0, partbody.Length);
					Dictionary<string, string> infodict = new Dictionary<string, string> ();
					MIME CT = null;
					foreach(string headerLine in headerLines) {
						string[] sline = headerLine.Split (new string[] {": "}, 2, StringSplitOptions.None);
						switch(sline[0]) {
							case "Content-Disposition":
							foreach (KeyValuePair<string, string> KVP in sline [1].Split (new string[] {"; "}, StringSplitOptions.None).Select((s) => s.Split('=')).Where((sa) => sa.Length > 1).Select((ss) => new KeyValuePair<string, string>(ss.ElementAt(0), ss.ElementAt(1)))) {
								infodict.Add (KVP.Key.Trim('\"'), KVP.Value.Trim('\"'));
							}
							break;
							case "Content-Type":
							CT = MIME.FromText (sline[1]);
							break;
							default:
							break;
						}
					}
					if (CT == null) {
						string name;
						if (infodict.Count > 0 && infodict.TryGetValue ("name", out name)) {
							CB.FormData.Add (name, Encoding.UTF8.GetString(partbody));
						}
					} else {
						string name;
						string filename;
						if (infodict.Count > 1 && infodict.TryGetValue("name", out name) && infodict.TryGetValue("filename", out filename)) {
							CB.FormDataBodies.Add (name, new Tuple<string, IStreamableContent> (filename, new GeneratedResource(partbody, CT)));
						}
					}
				}
				return CB;
			} else {
				return null;
			}
		}
	}

	public class ClientBody {
		public Dictionary<string, string> FormData = new Dictionary<string, string> ();
		public Dictionary<string, Tuple<string, IStreamableContent>> FormDataBodies = new Dictionary<string, Tuple<string, IStreamableContent>> ();
		public ClientBody () {

		}
	}

	public class ClientReturn {
		public IStreamableContent Body;
		public Dictionary<string, string> SetCookies = new Dictionary<string, string> ();
		public ClientReturn (IStreamableContent body) {
			this.Body = body;
		}
	}
}

