using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Mono.Data.Sqlite;

namespace squirrel
{
	public class Database
	{
		const bool HIDE_FILE_PATHS = false;
		
		SqliteConnection dbcon;

		public Database (string filename)
		{
			string connectionString = "URI=file:" + filename;
			dbcon = new SqliteConnection(connectionString);
			dbcon.Open();
			IDbCommand dbcmd = dbcon.CreateCommand();

			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS artists (name TEXT, songid INTEGER)";
			dbcmd.ExecuteNonQuery ();
			//Console.WriteLine("Create table artist {0}", dbcmd.ExecuteNonQuery ().ToString());
			dbcmd = dbcon.CreateCommand ();
			dbcmd.CommandText = "CREATE TABLE IF NOT EXISTS tracks (songid INTEGER PRIMARY KEY AUTOINCREMENT, path TEXT, album TEXT, title TEXT, track TEXT, genre TEXT, date TEXT, albumartist TEXT)";
			dbcmd.ExecuteNonQuery ();
			//Console.WriteLine("Create table artist {0}", dbcmd.ExecuteNonQuery ().ToString());
			dbcmd.Dispose();
		}

		~Database(){
			dbcon.Close ();
		}

		public System.IO.FileStream getFileContents(int id){
			var amd = getAudioMetaDataForSongid (id);
			return System.IO.File.OpenRead (amd.path);
		}

		public List<int> searchIDs(string term){
			var results = new List<int> ();
			if (term != null && term != "") {
				var cmd = dbcon.CreateCommand ();
				cmd.CommandText = "SELECT t.songid FROM tracks AS t WHERE t.path LIKE @query OR t.album LIKE @query OR t.title LIKE @query OR t.albumartist LIKE @query OR EXISTS (SELECT a.name FROM artists as a WHERE a.name LIKE @query AND a.songid = t.songid)";
				cmd.Parameters.AddWithValue ("@query", "%" + term + "%");
				cmd.Prepare ();
				SqliteDataReader rdr = cmd.ExecuteReader ();
				while (rdr.Read ()) {
					results.Add(rdr.GetInt32(0));
				}
			}
			return results;
		}

		public List<int> searchIDsLimited (string term, int limit){
			var results = new List<int> ();
			if (term != null && term != "") {
				var cmd = dbcon.CreateCommand ();
				cmd.CommandText = "SELECT t.songid FROM tracks AS t WHERE t.path LIKE @query OR t.album LIKE @query OR t.title LIKE @query OR t.albumartist LIKE @query OR EXISTS (SELECT a.name FROM artists as a WHERE a.name LIKE @query AND a.songid = t.songid) LIMIT @limit";
				cmd.Parameters.AddWithValue ("@query", "%" + term + "%");
				cmd.Parameters.AddWithValue ("@limit", limit);
				cmd.Prepare ();
				SqliteDataReader rdr = cmd.ExecuteReader ();
				while (rdr.Read ()) {
					results.Add(rdr.GetInt32(0));
				}
			}
			return results;
		}

		public List<AudioMetaData> searchFull(string term){
			var results = new List<AudioMetaData> ();
			foreach(var id in searchIDs(term)){
				results.Add(getAudioMetaDataForSongid(id));
			}
			return results;
		}

		public List<AudioMetaData> searchFullLimited(string term, int limit){
			var results = new List<AudioMetaData> ();
			foreach(var id in searchIDsLimited(term, limit)){
				results.Add(getAudioMetaDataForSongid(id));
			}
			return results;
		}

		public AudioMetaData getAudioMetaDataForSongid(int songid){
			var cmd = dbcon.CreateCommand ();
			cmd.CommandText = "SELECT path, album, title, track, genre, date, albumartist FROM tracks WHERE songid = @songid";
			cmd.Parameters.AddWithValue ("@songid", songid);
			cmd.Prepare ();
			SqliteDataReader rdr = cmd.ExecuteReader ();
			if (rdr.Read ()) {
				var amd = new AudioMetaData ();
				amd.songid = songid;
				amd.path = rdr.GetString (0);
				if (!rdr.IsDBNull (1)) {
					amd.album = rdr.GetString (1);
				}
				if (!rdr.IsDBNull (2)) {
					amd.title = rdr.GetString (2);
				}
				if (!rdr.IsDBNull (3)) {
					amd.track = rdr.GetString (3);
				}
				if (!rdr.IsDBNull (4)) {
					amd.genre = rdr.GetString (4);
				}
				if (!rdr.IsDBNull (5)) {
					amd.album_artist = rdr.GetString (5);
				}
				cmd.Dispose ();
				cmd = dbcon.CreateCommand ();
				cmd.CommandText = "SELECT name FROM artists WHERE songid = @songid";
				cmd.Parameters.AddWithValue ("@songid", songid);
				cmd.Prepare ();
				rdr = cmd.ExecuteReader ();
				while (rdr.Read ()) {
					amd.artists.Add (rdr.GetString (0));
				}
				cmd.Dispose ();
				return amd;
			} else {
				cmd.Dispose ();
				return null;
			}
		}

		public List<string> getAllPaths(){
			var cmd = dbcon.CreateCommand ();
			cmd.CommandText = "SELECT path FROM tracks";
			SqliteDataReader rdr = cmd.ExecuteReader();
			List<string> result = new List<string>();
			while (rdr.Read ()) {
				result.Add (rdr.GetString (0));
			}
			return result;
		}

		public List<string> getAllArtists(){
			var cmd = dbcon.CreateCommand ();
			cmd.CommandText = "SELECT DISTINCT name FROM artists";
			SqliteDataReader rdr = cmd.ExecuteReader();
			List<string> result = new List<string>();
			while (rdr.Read ()) {
				result.Add (rdr.GetString (0));
			}
			return result;
		}

		public List<string> getAllAlbumsForArtist(string artist){
			var cmd = dbcon.CreateCommand ();
			cmd.CommandText = "SELECT DISTINCT t.album FROM tracks AS t JOIN artists as a ON t.songid = a.songid WHERE a.name = @artist";
			cmd.Parameters.AddWithValue ("@artist", artist);
			cmd.Prepare ();
			SqliteDataReader rdr = cmd.ExecuteReader();
			List<string> result = new List<string>();
			while (rdr.Read ()) {
				result.Add (rdr.GetString (0));
			}
			return result;
		}

		public bool hasPath(string path){
			return getSongIdForPath(path) != null;
		}

		public int? getSongIdForPath(string path){
			var cmd = dbcon.CreateCommand ();
			cmd.CommandText = "SELECT songid FROM tracks WHERE path = @path";
			cmd.Parameters.AddWithValue ("@path", path);
			cmd.Prepare ();
			object value = cmd.ExecuteScalar ();
			if (value == null) {
				return null;
			} else {
				return Convert.ToInt32(value);
			}

		}

		public void addSong(AudioMetaData amd){
			//Insert into track table
			var cmd = dbcon.CreateCommand();
			cmd.CommandText = "INSERT INTO tracks(path, album, title, track, genre, date, albumartist) values (@path,@album,@title,@track,@genre,@date,@albumartist)";
			cmd.Parameters.AddWithValue ("@path", amd.path);
			cmd.Parameters.AddWithValue ("@album", amd.album);
			cmd.Parameters.AddWithValue ("@title", amd.title);
			cmd.Parameters.AddWithValue ("@track", amd.track);
			cmd.Parameters.AddWithValue ("@genre", amd.genre);
			cmd.Parameters.AddWithValue ("@date",amd.date);
			cmd.Parameters.AddWithValue ("@albumartist", amd.album_artist);
			cmd.Prepare ();
			cmd.ExecuteNonQuery ();

			//Get SongId back
			var songid = getSongIdForPath(amd.path);
			if (songid != null) {
				foreach (var artist in amd.artists) {
					cmd = dbcon.CreateCommand ();
					cmd.CommandText = "INSERT INTO artists(name, songid) values (@name,@id)";
					cmd.Parameters.AddWithValue ("@name", artist);
					cmd.Parameters.AddWithValue ("@id", songid);
					cmd.Prepare ();
					cmd.ExecuteNonQuery();
				}
			} else {
				throw new Exception (String.Format("Inserted path {0}, but got null on query!", amd.path));
			}

		}

		public void deletePath(string path){
			var id = this.getSongIdForPath (path);
			if (id != null) {
				var cmd = dbcon.CreateCommand ();
				cmd.CommandText = "DELETE FROM tracks WHERE songid = @songid";
				cmd.Parameters.AddWithValue ("@songid", id);
				cmd.Prepare ();
				cmd.ExecuteNonQuery ();
				cmd = dbcon.CreateCommand ();
				cmd.CommandText = "DELETE FROM artists WHERE songid = @songid";
				cmd.Parameters.AddWithValue ("@songid", id);
				cmd.Prepare ();
				cmd.ExecuteNonQuery ();
			}
		}

	}
}

