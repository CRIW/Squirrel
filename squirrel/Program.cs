using System;
using System.IO;
using Nancy.Hosting.Self;

namespace squirrel
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var dirpath = "";
			if (!File.Exists ("config")) {
				Console.WriteLine ("No config file with basedir exists!");
				Environment.Exit (0);
			} else {
				dirpath = File.ReadAllText ("config").Trim();
			}

			Console.WriteLine ("Starting squirrel service for " + dirpath);

			var db = new Database ("data.base");
			Global.db = db;
			db.refreshSearchIndex ();
			Global.scanner = new Scanner (db, dirpath);
			Global.runRescan ();


			//Nancy.StaticConfiguration.DisableErrorTraces = false;

			using (var host = new NancyHost(new Uri("http://localhost:1234")))
			{
				host.Start();
				Console.WriteLine("Running on http://localhost:1234");
				Console.ReadLine();
			}

		}
	}
}
