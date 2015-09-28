using System.Net;
using System.Net.Sockets;

namespace OSC.Simple {
	public class OscServer : OscPort {
		public int listenPort;
	
		void Start() {
			var serverEndpoint = new IPEndPoint (IPAddress.Any, listenPort);
			Init (serverEndpoint);
		}

		public override UdpClient GenerateUdpClient(IPEndPoint serverEndPoint) {
			return new UdpClient(_serverEndpoint);
		}

		public void Send(byte[] oscPacket, IPEndPoint clientEndpoint) {
			try {
				_udp.Send(oscPacket, oscPacket.Length, clientEndpoint);
			} catch (System.Exception e) {
				RaiseError(e);
			}
		}

	}
}