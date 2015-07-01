using System.Net;
using System.Net.Sockets;

namespace OSC.Simple {
	public class OscServer : OscPort {
	
		public OscServer(IPEndPoint serverEndpoint) : base(serverEndpoint) {}

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