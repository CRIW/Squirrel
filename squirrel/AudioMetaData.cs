using System;
using System.Collections.Generic;

namespace squirrel
{
	public class AudioMetaData
	{
		public List<string> artists;
		public string album_artist;
		public string album;
		public string title;
		public string track;
		public string path;
		public string genre;
		public string date;

		public AudioMetaData ()
		{
			this.artists = new List<string> ();
		}


		public void validate(){
			if(this.album_artist == null && this.artists.Count > 0){
				this.album_artist = this.artists[0];
			}
		}
	}
}

