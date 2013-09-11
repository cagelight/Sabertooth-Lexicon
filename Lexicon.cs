using System;
using System.Collections.Generic;

namespace Sabertooth.Lexicon {
	public struct Statement {
		string Key;
		string Value;
		public Statement(string Key, string Value) {
			this.Key = Key;
			this.Value = Value;
		}
		public override string ToString () {
			return String.Format ("{0}={1}", Key, Value);
		}
	}
	public class HTTPResponse {
		public struct Code {
			int Number;
			string Description;
			public Code (int code, string description) {
				this.Number = code; this.Description = description;
			}
			public override string ToString () {
				return String.Format ("HTTP/1.0 {0} {1}", Number, Description);
			}
			public static Code N200 = new Code (200, "OK");
			public static Code N400 = new Code (400, "Bad Request");
			public static Code N403 = new Code (403, "Forbidden");
			public static Code N404 = new Code (404, "Not Found");
		}
		public struct Instruction {
			string Key;
			string Value;
			List<Statement> Options;
			public Instruction (string Key, string Value) {
				this.Key = Key;
				this.Value = Value;
				this.Options = new List<Statement>();
			}
			public Instruction (string Key, string Value, params Statement[] Options) : this(Key, Value) {
				foreach(Statement S in Options) {
					AddOption(S);
				}
			}
			public void AddOption(Statement S) {
				Options.Add (S);
			}
			public override string ToString () {
				string OptionsString = String.Empty;
				foreach(Statement S in Options) {
					OptionsString += S+"; ";
				}
				return String.Format ("{0}: {1}; {2}", Key, Value, OptionsString);
			}
			public static Instruction SetCookie(Statement value, string domain, string path = "/") {
				return new Instruction ("Set-Cookie", value.ToString(), new Statement("Domain",domain), new Statement("Path",path));
			}
		}
		private Code responseCode;
		private MIME responseMIME;
		private List<Instruction> responseInstructions = new List<Instruction>();

		public HTTPResponse(Code C, MIME M) {
			responseCode = C;
			responseMIME = M;
		}

		public void AddInstruction(Instruction I) {
			responseInstructions.Add (I);
		}

		public override string ToString () {
			string stringInstructions = String.Empty;
			foreach(Instruction I in responseInstructions) {
				stringInstructions += I+"\n";
			}
			return String.Format ("{0}\n{1}", responseCode, stringInstructions);
		}
	}
}

