using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PingQRS.Qlik.Sense.RestClient;

namespace ForceRuleEvaluation
{
	class Program
	{
		private static readonly StreamWriter LogFile = new StreamWriter("Logfile.txt");

		static void Main(string[] args)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			if (args.Length < 4)
			{
				PrintUsage();
			}

			var url = args[0];
			int port = 0;
			try
			{
				port = int.Parse(args[1]);
			}
			catch
			{
				Console.WriteLine("Failed to parse \""+ args[1] + "\" as int.");
				PrintUsage();
			}

			if (!File.Exists(args[3]))
			{
				Console.WriteLine("Could not find file \"" + args[3] + "\".");
				Environment.Exit(1);
			}

			var users = File.ReadAllLines(args[3]).ToArray();
			Log("Connecting to " + url + ":" + port);
			Log("Read " + users.Length + " users.");
			var certs = RestClient.LoadCertificateFromStore();
			foreach (var user in users)
			{
				using (var client = new RestClient(url))
				{
					var d0 = DateTime.Now;

					client.AsDirectConnection(args[2], user, port, false, certs);
					var appTxtSize = client.Get("qrs/app").Length;
					var objTxtSize = client.Get("qrs/app/object").Length;
					var d1 = DateTime.Now;
					var dt = d1 - d0;
					Log(d1 + " - Connected as user \"" + user + "\". (" + appTxtSize + "," + objTxtSize + ") dt=" + dt);
				}
			}
		}

		private static void PrintUsage()
		{
			Console.WriteLine("Usage:   ForceRuleEvaluation.exe <url> <port> <domain> <pathToUserList>");
			Console.WriteLine("Example: ForceRuleEvaluation.exe https://my.server.url 4242 MyDomain C:\\Tmp\\MyUserList.txt");
			Environment.Exit(1);
		}

		private static void Log(string msg)
		{
			LogFile.WriteLine(msg);
			LogFile.Flush();
			Console.WriteLine(msg);
		}
	}
}
