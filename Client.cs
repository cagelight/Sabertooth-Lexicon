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
		public readonly string ETag = string.Empty;
		public readonly DateTime LastModified;
		public Dictionary<string,string> Arguments = new Dictionary<string, string> ();
		public Dictionary<string,string> Cookies = new Dictionary<string, string> ();
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
				this.path = decpath.Trim ('?');
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
				case "Cookie":
					this.Cookies = lineInstruction[1].Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().Split(new char[] {'='}, StringSplitOptions.None)).ToDictionary(x => x[0], x => x[1]);
					break;
				case "If-Modified-Since":
					DateTime.TryParseExact (lineInstruction[1], "R", System.Globalization.DateTimeFormatInfo.CurrentInfo, System.Globalization.DateTimeStyles.AllowWhiteSpaces, out LastModified);
				case "If-None-Match":
					this.ETag = lineInstruction [1].Trim ('\"');
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
		public string GetArgumentString() {
			return "?" + String.Join ("&", this.Arguments.Select (x => String.Format ("{0}={1}", x.Key, x.Value)));
		}
		public string GetArgumentStringAddOverride(string key, string value) {
			return "?" + String.Join("", this.Arguments.Where(x => x.Key != key).Select (x => String.Format ("{0}={1}&", x.Key, x.Value))) + String.Format("{0}={1}", key, value);
		}
		public string GetArgumentStringAddOverrideRemove(string key, string value, params string[] removekeys) {
			return "?" + String.Join("", this.Arguments.Where(x => !removekeys.Contains(x.Key)).Where(x => x.Key != key).Select (x => String.Format ("{0}={1}&", x.Key, x.Value))) + String.Format("{0}={1}", key, value);
		}
		public string GetArgumentStringRemove(params string[] keys) {
			return "?" + String.Join ("&", this.Arguments.Where(x => !keys.Contains(x.Key)).Select (x => String.Format ("{0}={1}", x.Key, x.Value)));
		}
		/// <summary>
		/// Generates a string representing a new arguments list with one argument.
		/// </summary>
		/// <returns>The generated argument string.</returns>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		public string GetArgumentStringNew(string key, string value) {
			return "?" + String.Format("{0}={1}", key, value);
		}
		/// <summary>
		/// Generates a string representing a new arguments list with one new argument, and the option of preserving some pairs from the previous arguments.
		/// </summary>
		/// <returns>The argument string new.</returns>
		/// <param name="key">Key</param>
		/// <param name="value">Value</param>
		/// <param name="preserve">Keys to preserve.</param>
		public string GetArgumentStringNew(string key, string value, params string[] preserve) {
			return "?" + String.Join("", this.Arguments.Where(x => preserve.Contains(x.Key)).Select (x => String.Format ("{0}={1}&", x.Key, x.Value))) + String.Format("{0}={1}", key, value);
		}
		/// <summary>
		/// Generates a string representing a new arguments list with a set of new arguments, and the option of preserving some pairs from the previous arguments.
		/// </summary>
		/// <returns>The argument string new.</returns>
		/// <param name="keyvals">Set of new Key/Value pairs.</param>
		/// <param name="preserve">Keys to preserve.</param>
		public string GetArgumentStringNew(IEnumerable<KeyValuePair<string,string>> keyvals, params string[] preserve) {
			return "?" + String.Join("", this.Arguments.Where(x => preserve.Contains(x.Key)).Select (x => String.Format ("{0}={1}&", x.Key, x.Value))) + String.Join("&", keyvals.Select(x => String.Format("{0}={1}", x.Key, x.Value)));
		}
	}

	public class ClientBody {
		public Dictionary<string, string> FormData = new Dictionary<string, string> ();
		public Dictionary<string, Tuple<string, IStreamableContent>> FormDataBodies = new Dictionary<string, Tuple<string, IStreamableContent>> ();
		public ClientBody () {

		}
	}

	public class ClientReturn {
		protected IStreamableContent body;
		public IStreamableContent Body {
			get { return this.body; }
			set { this.body = value; }
		}
		public List<SetCookie> SetCookies = new List<SetCookie> ();
		public int MaxAge = 360;
		public string Redirect;
		public ClientReturn () {

		}
		public ClientReturn (IStreamableContent body) {
			this.body = body;
		}
		public void RemoveRedirect() {
			this.Redirect = null;
		}
	}

	public class CacheData {
		public int MaxAge = 360;
		public DateTime LastModified;
		public string ETag = String.Empty;
		public CacheData() {

		}
	}
}

