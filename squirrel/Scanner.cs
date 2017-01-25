using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

namespace squirrel
{
	public class Scanner
	{
		Database db;
		string directory;

		public Scanner (Database db, string directory)
		{
			this.db = db;
			this.directory = directory;
		}

		public void rescan(){
			Global.putScanStatus ("Validating existing paths");
			foreach (var path in db.getAllPaths()) {
				if (!File.Exists (path)) {
					db.deletePath (path);
					Global.putScanStatus ("Deleted invalid path " + path);
				}
			}
			Global.putScanStatus ("Looking for new files");
			scanDirectory (directory);
			Global.putScanStatus ("Finished descent");
		}

		void scanDirectory(string filename){
			Global.putScanStatus ("Descending into " + filename);

			//DFS FTW
			foreach (var dir in Directory.GetDirectories(filename)) {
				scanDirectory (dir);
			}

			foreach (var file in Directory.GetFiles(filename)) {
				if (file.EndsWith (".mp3")) {
					scanFile (file);
				}
			}
		}


		void scanFile(string filename){
			if (!this.db.hasPath (filename)) {
				var psi = new ProcessStartInfo ("ffmpeg", "-i " + Global.escapeArgument (filename) + " -f ffmetadata -");
				Global.putScanStatus("Scanning " + filename);
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
				var p = Process.Start (psi);
				p.WaitForExit ();

				var lines = p.StandardOutput.ReadToEnd ().Split ('\n');
				var amd = new AudioMetaData ();

				foreach (var line in lines) {
					if (line.StartsWith ("title")) {
						amd.title = line.Substring ("title=".Length);
					} else if (line.StartsWith ("artist")) {
						foreach(var artist in line.Substring ("artist=".Length).Split (',')){
							amd.artists.Add (artist);
						}
					} else if (line.StartsWith ("album_artist")) {
						amd.album_artist = line.Substring ("album_artist=".Length);
					} else if (line.StartsWith ("track")) {
						amd.track = line.Substring ("track=".Length);
					} else if (line.StartsWith ("genre")) {
						amd.genre = line.Substring ("genre=".Length);
					} else if (line.StartsWith ("date")) {
						amd.date = line.Substring ("date=".Length);
					} else if (line.StartsWith ("album")) {
						amd.album = line.Substring ("album=".Length);
					}
				}

				var e = p.StandardError.ReadToEnd ();
				var r = new Regex("\\s+Duration\\: (\\S+)\\,");
				Match m = r.Match (e);
				if (m.Success) {
					var d = m.Groups [0].ToString ().Trim();
					if (d != null && d.Length > 0) {
						try{
						var parts = d.Split (':');
						var hourpart = parts [1].Trim();
						var hours = Double.Parse(hourpart);
						var minutes = Double.Parse (parts[2].Trim());
						var seconds = Double.Parse (parts[3].Trim(' ',','));
						minutes = minutes + hours * 60;
						seconds = seconds + minutes * 60;
						amd.duration = seconds;
						}catch{
							Global.putScanStatus ("Error parsing duration in " + filename);
						}
					}
				}

				amd.path = filename;
				amd.validate ();
				this.db.addSong (amd);
			}
		}




	}
}

