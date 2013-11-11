using System;
using System.IO;
using System.Text;

namespace Sabertooth.Lexicon {
	public class TimeTracker {
		DateTime timeStart;
		public TimeSpan Mark {
			get{return DateTime.Now - timeStart;}
		}
		public TimeTracker() {
			timeStart = DateTime.Now;
		}
		public void Restart() {
			timeStart = DateTime.Now;
		}
	}

	public static class LexiconHelpers {
		public static string PathFromLocal(string local) {
			return Path.Combine (Environment.CurrentDirectory, "Assets", local);
		}
	}
}

