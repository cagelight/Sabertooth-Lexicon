using System;

namespace Sabertooth.Lexicon {
	public struct MIME : ITextable{
		public readonly string Type;
		public readonly string Format;
		public MIME(string type, string format) {
			Type = type;
			Format = format;
		}
		public string GetText () {
			return String.Format ("{0}/{1}", Type, Format);
		}
		public static MIME Plaintext = new MIME("text", "plain");
		public static MIME HTML = new MIME("text", "html");
	}
}

