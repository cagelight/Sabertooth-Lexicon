using System;

namespace Sabertooth.Lexicon {
	public abstract class SiteBase {
		public abstract IStreamableContent Get(ClientRequest CR);
	}
}

