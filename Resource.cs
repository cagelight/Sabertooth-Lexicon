using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using WebSharp;

namespace Sabertooth.Lexicon {
	/// <summary>
	/// IStreamable Interface.
	/// GetSize() should return -1 if the size cannot be obtained without significant work. This indicates that if utilizing GetBytes(), the size should be obtained from that. If using StreamTo, the size is unobtainable.
	/// </summary>
	public interface IStreamable {bool IsLoaded {get;} byte[] GetBytes(); int GetSize(); void StreamTo(Stream S);}
	public interface IStreamableContent : IStreamable {MIME Format {get;}}
	public class WebpageResource : IStreamableContent {
		public bool IsLoaded {get {return true;}}
		public MIME Format {get{return MIME.HTML;}}
		public Webpage Content;
		public TimeTracker TT;
		public WebpageResource(Webpage WP, TimeTracker TT = null) {
			if (WP != null) {
				this.Content = WP;
				this.TT = TT;
			} else {
				throw new ArgumentNullException ("WebpageResource: Webpage WP argument is essential and cannot be null.");
			}
		}
		public byte[] GetBytes() {
			string wp = this.Content.ToString ();
			if (TT != null)
				wp += String.Format ("<!-- {0}ms -->", TT.Mark.TotalMilliseconds);
			return Encoding.UTF8.GetBytes(wp);
		}
		public int GetSize() {
			return -1;
		}
		public void StreamTo(Stream S) {
			MemoryStream MS = new MemoryStream (this.GetBytes());
			MS.CopyTo (S, 32768);
			S.Flush ();
			MS.Close ();
		}
	}

	public class TextResource : IStreamableContent {
		public string content;
		public bool IsLoaded {get {return true;}}
		public MIME Format {get{return MIME.Plaintext;}}
		public TextResource(string text) {
			this.content = text;
		}
		public byte[] GetBytes() {
			return Encoding.UTF8.GetBytes(content);
		}
		public int GetSize() {
			return content.Length;
		}
		public void StreamTo(Stream S) {
			MemoryStream MS = new MemoryStream (this.GetBytes());
			MS.CopyTo (S, 32768);
			S.Flush ();
			MS.Close ();
		}
	}

	public class GeneratedResource : IStreamableContent {
		public byte[] content;
		public MIME format;
		public bool IsLoaded {get {return true;}}
		public MIME Format {get{return format;}}
		public GeneratedResource(byte[] content, MIME format) {
			this.content = content;
			this.format = format;
		}
		public byte[] GetBytes() {
			return content;
		}
		public int GetSize() {
			return content.Length;
		}
		public void StreamTo(Stream S) {
			MemoryStream MS = new MemoryStream (content);
			MS.CopyTo (S, 32768);
			S.Flush ();
			MS.Close ();
		}
	}

	public class FileResource : IStreamableContent {
		public FileInfo file;
		public bool IsLoaded {get {return false;}}
		public MIME Format {get{return MIME.FromExtension (file.Extension);}}
		public FileResource(string fullpath) {
			file = new FileInfo (fullpath);
		}
		public byte[] GetBytes() {
			FileStream FS = file.OpenRead ();
			byte[] fileBytes = new byte[file.Length];
			FS.Read (fileBytes, 0, (int)file.Length);
			FS.Close ();
			return fileBytes;
		}
		public int GetSize() {
			return (int)file.Length;
		}
		public void StreamTo(Stream S) {
			FileStream FS = file.OpenRead ();
			FS.CopyTo (S, 32768);
			S.Flush ();
			FS.Close ();
		}
	}
}

