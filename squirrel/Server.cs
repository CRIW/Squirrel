﻿using System;
using Nancy;
using Nancy.Responses;

namespace squirrel
{
	public class Server : NancyModule
	{
		public Server ()
		{
			Get ["/"] = parameters => "HelloWorld!";
			Get ["/songs"] = _ => Global.db.getAllArtists ().ToArray ();
			Get ["/paths"] = _ => Global.StringArrayToJSON( Global.db.getAllPaths ().ToArray ());
			Get ["/search/ids/{term}"] = parameters => Global.db.searchIDs (parameters.term).ToArray ();
			Get ["/search/full/{term}"] = parameters => Global.db.searchFull(parameters.term).ToArray ();
			Get ["/search/ids/{term}/limit/{limit}"] = parameters => Global.db.searchIDsLimited (parameters.term, parameters.limit).ToArray ();
			Get ["/search/full/{term}/limit/{limit}"] = parameters => Global.db.searchFullLimited(parameters.term, parameters.limit).ToArray ();
			Get ["/details/{id}"] = parameters => Global.db.getAudioMetaDataForSongid (parameters.id);
			Get ["file/{id}"] = parameters => new StreamResponse( () => Global.db.getFileContents (parameters.id),"audio/mpeg");
		}

	}
}
