using System;
using Nancy.Hosting.Self;

namespace squirrel
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

			var db = new Database ("data.base");
			Global.db = db;
			var scanner = new Scanner (db, "/home/apexys/Music/Rena");
			System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch ();
			st.Start ();
			scanner.rescan();
			st.Stop ();
			Console.WriteLine (st.ElapsedMilliseconds);

			db.getAudioMetaDataForSongid (1);

			var paths = Global.db.getAllPaths ().ToArray ();
			var json = Global.StringArrayToJSON (paths);

			Nancy.StaticConfiguration.DisableErrorTraces = false;

			using (var host = new NancyHost(new Uri("http://localhost:1234")))
			{
				host.Start();
				Console.WriteLine("Running on http://localhost:1234");
				Console.ReadLine();
			}

		}
	}
}
