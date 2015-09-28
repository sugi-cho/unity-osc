using System.Net;
using System.Net.Sockets;

namespace OSC.Simple {
	public class OscClient : OscPort {
		public string remoteHost = "localhost";
		public int remotePort = 10000;

		void Start() {
			var address = Dns.GetHostAddresses (remoteHost) [0];
			var serverEndpoint = new IPEndPoint (address, remotePort);
			Init (serverEndpoint);
		}

		public override UdpClient GenerateUdpClient (IPEndPoint serverEndPoint) {
			return new UdpClient ();
		}

		public void Send(byte[] oscPacket) {
			try {
				_udp.Send(oscPacket, oscPacket.Length, _serverEndpoint);
			} catch (System.Exception e) {
				RaiseError(e);
			}
		}
	}
}