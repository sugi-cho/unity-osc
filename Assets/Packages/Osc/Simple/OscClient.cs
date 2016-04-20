using System.Net;
using System.Net.Sockets;

namespace OSC.Simple {
	public class OscClient : OscPort {
		public string remoteHost = "localhost";
		public int remotePort = 10000;

		void Start() {
			try {
				var addresses = Dns.GetHostAddresses (remoteHost);
				IPAddress address = IPAddress.None;
				for (var i = 0; i < addresses.Length; i++) {
					if (addresses[i].AddressFamily == AddressFamily.InterNetwork) {
						address = addresses[i];
						break;
					}
				}
				var serverEndpoint = new IPEndPoint(address, remotePort);
				Init (serverEndpoint);
			} catch (System.Exception e) {
				RaiseError (e);
			}
		}

		public override UdpClient GenerateUdpClient (IPEndPoint serverEndPoint) {
			return new UdpClient (AddressFamily.InterNetwork);
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