using System;
using Nancy;

namespace squirrel
{
	public class Server : NancyModule
	{
		public Server ()
		{
			Get ["/"] = parameters => "HelloWorld!";
			Get ["/songs"] = _ => Global.db.getAllArtists ().ToArray ();
			Get ["/paths"] = _ => Global.StringArrayToJSON( Global.db.getAllPaths ().ToArray ());

		}

	}
}

