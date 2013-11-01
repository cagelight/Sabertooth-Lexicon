using System;

namespace Sabertooth.Lexicon.Attributes {
	[AttributeUsage(AttributeTargets.Class)]
	public class Subdomains : Attribute {
		public readonly string[] subdomains;
		public Subdomains(params string[] subdomains) {
			this.subdomains = subdomains;
		}
	}
	[AttributeUsage(AttributeTargets.Class)]
	public class Root : Attribute {}
	[AttributeUsage(AttributeTargets.Assembly)]
	public class RefreshTime : Attribute {
		public readonly int msec;
		public RefreshTime(int msec) {
			this.msec = msec;
		}
	}
}

