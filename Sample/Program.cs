using System;
using System.Threading;

namespace PeerDiscovery.Sample
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var sender = new Sender ();
			var listener = new Listener ();
			listener.DiscoveredEndPoint += delegate(object obj, Listener.DiscoveredEndPointEventArgs e) {
				Console.WriteLine("DiscoveredEndPoint event was raised.  End Point is {0}", e.EndPoint.ToString());
			};

			Console.WriteLine ("Starting Listener");
			listener.Start ();

			Console.WriteLine ("Choose either 's' to send, or 'x' to exit");
			ConsoleKeyInfo cki;

			bool continueLoop = true;
			do
			{
				if (Console.KeyAvailable)
				{
					cki = Console.ReadKey(true);
					switch (cki.KeyChar)
					{
					case 's':
						sender.Send();
						break;
					case 'x':
						Console.WriteLine("Try exiting...");

						listener.Stop();
						continueLoop = false;
						break;
					}
				}

				Thread.Sleep(10);
			} while (continueLoop);

			Console.WriteLine ("Please press any key to exit...");
			Console.ReadKey (true);
		}
	}
}
