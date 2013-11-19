using System;
using System.Collections.Generic;
using System.IO;

namespace Sabertooth.Lexicon {
	public abstract class SiteBase {
		protected readonly string AssetSubdir;
		public SiteBase(string assetsubdir) {
			this.AssetSubdir = Path.Combine (Environment.CurrentDirectory, "Assets", assetsubdir);
			if (!Directory.Exists (assetsubdir))
				Directory.CreateDirectory (assetsubdir);
		}
		public virtual void Upkeep () {}
		public abstract IStreamableContent Get(ClientRequest CR);
		public abstract IStreamableContent Post(ClientRequest CR, ClientBody CB);
		public virtual bool IsAuthorized(ClientRequest CR, Tuple<string, string> auth, out string realm) {realm = "Authorize"; return true;}
		protected IStreamableContent OpenFile(string urlpath) {
			string fullpath = Path.Combine (AssetSubdir, urlpath.Trim ('.', ' ', Path.DirectorySeparatorChar));
			if (File.Exists (fullpath))
				return new FileResource (fullpath);
			else {
				return null;
			}
		}
		protected bool SaveFile(string urlpath, IStreamableContent isc) {
			string fullpath = Path.Combine (AssetSubdir, urlpath);
			if (File.Exists(fullpath)) {
				return false;
			} else {
				using (FileStream FS = File.Create(fullpath)) {
					isc.StreamTo (FS);
				}
				return true;
			}
		}
	}
}

