using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Configuration;

namespace PeerDiscovery
{
	public class Sender
	{
		public int UDPDiscoveryPort { get; private set; }

		public string UDPDiscoveryApplicationId { get; private set; }

		public int TCPCommunicationPort { get; private set; }


		public Sender ()
		{
			SetPropertiesFromAppConfiguration ();
		}

		protected void SetPropertiesFromAppConfiguration()
		{
			int discoveryPort;
			int.TryParse(ConfigurationManager.AppSettings.Get ("UDPDiscoveryPort"), out discoveryPort);
			this.UDPDiscoveryPort = discoveryPort;

			int communicationPort;
			int.TryParse(ConfigurationManager.AppSettings.Get ("TCPCommunicationPort"), out communicationPort);
			this.TCPCommunicationPort = communicationPort;

			this.UDPDiscoveryApplicationId = ConfigurationManager.AppSettings.Get ("UDPDiscoveryApplicationId");
		}

		public void Send()
		{
			UdpClient client = new UdpClient();
			IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, this.UDPDiscoveryPort);

			byte[] message = this.CreateMessage ();

			client.Send(message, message.Length, ip);
			client.Close();
		}

		protected byte[] CreateMessage()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append (this.UDPDiscoveryApplicationId);
			sb.Append (";");
			sb.Append (this.GetIPAddress());
			sb.Append (":");
			sb.Append (this.TCPCommunicationPort);
			return Encoding.ASCII.GetBytes (sb.ToString ());
		}

		protected string GetIPAddress()
		{
			IPHostEntry host;
			string localIP = "?";
			host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (IPAddress ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIP = ip.ToString();
				}
			}
			return localIP;
		}
	}
}

