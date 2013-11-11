using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using WebSharp;

namespace Sabertooth.Lexicon {
	public class ClientRequest {
		protected List<string> headerList;
		private string type;
		public string Type { get{return type;} }
		private string path;
		public string Path { get{return path;} }
		protected int contentlength = 0;
		public int ContentLength { get {return contentlength;} }
		protected MIME contenttype = null;
		public MIME ContentType { get{return contenttype;} }
		protected string host = null;
		public string Host {get {return host;}}
		public Dictionary<string,string> Arguments;
		public IPAddress IP;
		protected IStreamableContent body;
		public IStreamableContent Body { get {return body;} }
		public readonly TimeTracker RequestTime;
		public string Header { get { return String.Join ("\r\n", this.headerList) + "\r\n\r\n"; } }
		public ClientRequest(List<string> header) {
			headerList = header;
			RequestTime = new TimeTracker ();
			Arguments = new Dictionary<string, string> ();
			this.ProcessRawHeader ();
			Console.WriteLine (this.Header);
		}
		public bool ProcessRawHeader() { //returns true if header expects a body to follow. (i.e. Content-Length is present and greater than 0.)
			string[] requestLine = headerList [0].Split (' ');
			this.type = requestLine[0];

			int argindex = requestLine[1].IndexOf ('?');
			if (argindex == -1 || argindex == requestLine[1].Length-1) {
				this.path = requestLine[1];
			} else {
				string[] rawargs = requestLine[1].Substring(argindex+1).Split('&');
				this.path = requestLine[1].Substring(0,argindex);
				foreach(string S in rawargs) {
					if (S.Length < 3) {continue;}
					string[] splitargs = S.Split('=');
					if (splitargs.Length != 2) {continue;}
					this.Arguments[splitargs[0]] = splitargs[1];
				}
			}
			foreach (string h in headerList) {
				string[] lineInstruction = h.Split(new string[] {": "}, StringSplitOptions.None);
				switch(lineInstruction[0]) {
				case "Content-Type":
					this.contenttype = MIME.FromText (lineInstruction [1]);
					break;
				case "Content-Length":
					this.contentlength = Convert.ToInt32 (lineInstruction [1]);
					break;
				case "Host":
					this.host = lineInstruction [1];
					break;
				}
			}
			if(this.ContentLength > 0)
				return true;
			return false;
		}

		public void SetBody (byte[] bodybytes) {
			if (bodybytes.Length != this.ContentLength)
				throw new ArgumentOutOfRangeException ("Byte array passed to ClientRequest via SetBody cannot be longer or shorter than the number of bytes specified in the header's Content-Length property.");
			this.body = new GeneratedResource (bodybytes, this.ContentType);
		}
	}
}

