using System;
using System.IO;

namespace Sabertooth.Lexicon {
	public interface IStreamable {byte[] GetBytes(); int GetSize(); void StreamTo(Stream S);}
	public interface IStreamableContent : IStreamable {MIME GetFormat();}

	public class GeneratedResource : IStreamableContent {
		public byte[] content;
		public MIME format;
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
			MS.CopyTo (S, 4096);
			S.Flush ();
			MS.Close ();
		}
		public MIME GetFormat() {
			return format;
		}
	}

	public class FileResource : IStreamableContent {
		public FileInfo file;
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
			FS.CopyTo (S, 4096);
			S.Flush ();
			FS.Close ();
		}
		public MIME GetFormat() {
			return MIME.FromExtension (file.Extension);
		}
	}
}

