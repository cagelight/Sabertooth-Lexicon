using System;
using System.IO;
using System.Text;

namespace Sabertooth.Lexicon {
	public interface ITextable {string GetText();}
	public struct GenericTextable : ITextable {object text; public GenericTextable(object o){text = o;} public string GetText(){return text.ToString ();}}
	public struct Statement : ITextable {
		public string Key;
		public string Value;
		public Statement(string Key, string Value) {
			this.Key = Key;
			this.Value = Value;
		}
		public string GetText() {
			return String.Format ("{0}={1}", Key, Value);
		}
		public override string ToString () {
			return GetText ();
		}
	}
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

