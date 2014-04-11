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

	public struct SetCookie {
		public string Key;
		public string Value;
		public string Domain;
		public DateTime Expiration;
		public bool HttpOnly;
		public string Path;
		public SetCookie(string key, string value) {
			Key = key;
			Value = value;
			Domain = String.Empty;
			Expiration = new DateTime ();
			HttpOnly = false;
			Path = "/";
		}
		public override string ToString () {
			return String.Format ("{0}{1}{2}{3}{4}", Key+"="+Value+"; ", Domain == String.Empty ? String.Empty : "Domain="+Domain+"; ", Path == String.Empty ? String.Empty : "Path="+Path+"; ", Expiration == new DateTime() ? String.Empty : "Expires="+Expiration.ToString("R")+"; ", HttpOnly ? "HttpOnly; " : String.Empty);
		}
	}
}

