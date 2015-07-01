using System.Net;
using System.Net.Sockets;

namespace OSC.Simple {
	public class OscClient : OscPort {

		public OscClient(IPEndPoint serverEndpoint) : base(serverEndpoint) { }

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