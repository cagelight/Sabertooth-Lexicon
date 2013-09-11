using System;

namespace Sabertooth.Lexicon {
	public struct MIME {
		public readonly string Type;
		public readonly string Format;
		public MIME(string type, string format) {
			Type = type;
			Format = format;
		}
		public override string ToString () {
			return String.Format ("{0}/{1}", Type, Format);
		}
		public static MIME Plaintext = new MIME("text", "plain");
		public static MIME HTML = new MIME("text", "html");
	}
}

