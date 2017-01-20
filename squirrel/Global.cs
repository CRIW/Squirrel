using System;
using System.Collections.Generic;
using System.Linq;

namespace squirrel
{
	public class Global
	{
		public static string escapeArgument(string argument){
			return "\"" + argument.Replace("\"","\\\"") + "\"";
		}

		public static string StringArrayToJSON(string[] input){
			return "[" + string.Join (",", input.Select (i => escapeArgument (i))) + "]";
		}
			
		public static Database db;
	}
}

