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
		}
		public MIME GetFormat() {
			return format;
		}
	}
}

