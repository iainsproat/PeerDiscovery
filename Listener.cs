using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.Configuration;

namespace PeerDiscovery
{
	public class Listener
	{
		private Thread listenerThread;

		private UdpClient listener;

		public int UDPDiscoveryPort { get; private set; }

		public string UDPDiscoveryApplicationId { get; private set; }

		public event EventHandler<DiscoveredEndPointEventArgs> DiscoveredEndPoint;

		public byte[] UDPDiscoveryApplicationIdAsBytes
		{
			get
			{
				return Encoding.ASCII.GetBytes(this.UDPDiscoveryApplicationId);
			}
		}

		protected int ExpectedMinimumPackageLength {
			get { return 14; }
		}

		public Listener ()
		{
			SetPropertiesFromAppConfiguration ();
		}

		protected void SetPropertiesFromAppConfiguration()
		{
			int discoveryPort;
			int.TryParse(ConfigurationManager.AppSettings.Get ("UDPDiscoveryPort"), out discoveryPort);
			this.UDPDiscoveryPort = discoveryPort;

			this.UDPDiscoveryApplicationId = ConfigurationManager.AppSettings.Get ("UDPDiscoveryApplicationId");
		}

		public void Start()
		{
			listenerThread = new Thread(this.Listen);
			listenerThread.Start();
		}

		public void Stop()
		{
			//listener.Close ();  //FIXME results in acceptCallBack calling a disposed object.
			//TODO stop the listener thread
			//TODO implement the IDisposable interface?
		}

		protected void Listen()
		{
			var endPoint = new IPEndPoint (IPAddress.Any, this.UDPDiscoveryPort);
			listener = new UdpClient (endPoint);
			
			try{
				BeginReceive();
			}
			catch(Exception e)
			{
				Console.WriteLine (e.ToString());
			}

		}

		protected void BeginReceive()
		{
			listener.BeginReceive(
				new AsyncCallback(this.AcceptCallback), 
				null );
		}

		protected void AcceptCallback(IAsyncResult res) {

			IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 8000);  //FIXME is there any significance in the port number here, or is it just a placeholder?

			byte[] received = listener.EndReceive(res, ref RemoteIpEndPoint);

			//get immediately back to listening for the next package
			BeginReceive ();

			// handle received data
			IPEndPoint address;
			if (PackageIsValid (received, out address)) {
				this.OnDiscoveredEndPoint (new DiscoveredEndPointEventArgs (address));
			}

		}

		public class DiscoveredEndPointEventArgs : EventArgs
		{
			public IPEndPoint EndPoint {
				get;
				private set;
			}

			public DiscoveredEndPointEventArgs(IPEndPoint ePnt)
			{
				this.EndPoint = ePnt;
			}
		}

		protected void OnDiscoveredEndPoint(DiscoveredEndPointEventArgs e)
		{
			EventHandler<DiscoveredEndPointEventArgs> evnt = DiscoveredEndPoint;
			if (evnt != null) {
				DiscoveredEndPoint (this, e);
			}
		}

		protected bool PackageIsValid(byte[] package, out IPEndPoint tcpEndPoint)
		{
			tcpEndPoint = null;

			if (package == null) {
				return false;
			}

			if (package.Length < ExpectedMinimumPackageLength) {
				return false;
			}

			string message = Encoding.ASCII.GetString (package);
			string actualApplicationId = message.Substring (0, 6);
			if (actualApplicationId != this.UDPDiscoveryApplicationId) {
				return false;
			}

			int portIndex = message.IndexOf (':');
			string ipAddressPortion = message.Substring (7, portIndex - 7);
			IPAddress address;
			bool ipAddressParsed = IPAddress.TryParse (ipAddressPortion, out address);

			string tcpPortPortion = message.Substring (portIndex + 1, message.Length - (portIndex + 1));
			int tcpPort;
			bool tcpPortParsed = int.TryParse (tcpPortPortion, out tcpPort);
		
			tcpEndPoint = new IPEndPoint (address, tcpPort);

			return ipAddressParsed && tcpPortParsed;
		}
	}
}

