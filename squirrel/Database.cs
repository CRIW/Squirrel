using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Mono.Data.Sqlite;

namespace squirrel
{
	public class Database
	{
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

