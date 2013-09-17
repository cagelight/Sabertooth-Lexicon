using System;
using System.Text;

namespace Sabertooth.Lexicon {
	public interface ITextable {string GetText();}
	public interface IStreamable {byte[] GetBytes();}
	public struct GenericTextable : ITextable {object text; public GenericTextable(object o){text = o;} public string GetText(){return text.ToString ();}}
	public struct TextStreamable : IStreamable {
		string text;
		public TextStreamable(string t){
			text = t;
		}
		public byte[] GetBytes(){
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			return bytes;
		}
	}
	public struct Statement : ITextable {
		string Key;
		string Value;
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
}

