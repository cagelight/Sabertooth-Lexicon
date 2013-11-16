using System;
using System.Collections.Generic;
using System.IO;

namespace Sabertooth.Lexicon {
	public abstract class SiteBase {
		public abstract IStreamableContent Get(ClientRequest CR);
		public abstract IStreamableContent Post(ClientRequest CR, ClientBody CB);
		public virtual bool RequiresAuthorization(ClientRequest CR, out string realm) {realm = "Authorize"; return false;}
		public virtual bool IsAuthorized(ClientRequest CR, Tuple<string, string> auth) {return true;}
	}

	public static class SiteHelpers {
		public static IStreamableContent GetFileFromPath(string CRPath, string assetDir = "") {
			string wpath = CRPath;
			wpath = wpath.Trim (new char[] {'.', ' '});
			if (wpath.StartsWith (""+Path.DirectorySeparatorChar))
				wpath = wpath.Substring (1);
			string fullpath = Path.Combine (Environment.CurrentDirectory, "Assets", assetDir, wpath);
			if (File.Exists(fullpath)) {
				return new FileResource (fullpath);
			} else {
				return null;
			}
		}
		public static bool SaveFileToPath(string localpath, IStreamableContent isc, string assetDir = "") {
			string fullpath = Path.Combine (Environment.CurrentDirectory, "Assets", assetDir, localpath);
			if (!File.Exists(fullpath)) {
				FileStream FS = File.Create (fullpath);
				isc.StreamTo (FS);
				FS.Close ();
				FS.Dispose ();
			}
			return false;
		}
	}
}

