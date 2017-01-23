using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

		public static Scanner scanner;
		public static bool rescanActive = false;
		static LinkedList<string> rescanStatus = new LinkedList<string> ();
		static Thread rescanThread;
		static void rescan_routine(){
			putScanStatus("Scan started at " + DateTime.Now.ToString());
			try{
			Global.scanner.rescan ();
			}catch(Exception ex){
				putScanStatus ("Error in rescan: " + ex.Message);
			}
			Global.rescanActive = false;
			putScanStatus("Scan finished at " + DateTime.Now.ToString());
		}

		public static void runRescan(){
			if (!rescanActive) {
				rescanActive = true;
				ThreadStart ts = new ThreadStart (rescan_routine);
				rescanThread = new Thread (ts);
				rescanThread.Start ();
			}
		}
		public static void putScanStatus(string text){
			rescanStatus.AddLast (text);
			if (rescanStatus.Count > 5000) {
				rescanStatus.RemoveFirst ();
			}
		}
		public static string getScanStatus(){
			StringBuilder sb = new StringBuilder ();
			foreach (string s in rescanStatus.Reverse()) {
				sb.AppendLine (s);
			}
			return sb.ToString ();
		}
	}
}

