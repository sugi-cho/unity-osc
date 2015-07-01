using System.Net.Sockets;
using System;
using System.Net;
using Osc;

namespace OSC.Simple {
	public class OscServer : IDisposable {
		public event Action<Message, IPEndPoint> OnReceive;
		public event Action<Exception> OnError;
		
		private UdpClient _udp;
		private AsyncCallback _callback;
		private IPEndPoint _serverEndpoint;
		private Parser _oscParser;
		private bool _disposed = false;
	
		public OscServer(IPEndPoint serverEndpoint) {
			_serverEndpoint = serverEndpoint;
			_udp = new UdpClient(_serverEndpoint);
			_callback = new System.AsyncCallback(HandleReceive);
			_oscParser = new Parser();
			
			_udp.BeginReceive(_callback, null);			
		}
		
		
		public void Send(byte[] oscPacket, IPEndPoint clientEndpoint) {
			try {
				_udp.Send(oscPacket, oscPacket.Length, clientEndpoint);
			} catch (Exception e) {
				if (OnError != null)
					OnError(e);
			}
		}
			
		private void HandleReceive(System.IAsyncResult ar) {
			try {
				if (_udp == null)
					return;
				var clientEndpoint = new IPEndPoint(0, 0);
				byte[] receivedData = _udp.EndReceive(ar, ref clientEndpoint);
				_oscParser.FeedData(receivedData);
				while (_oscParser.MessageCount > 0) {
					var m = _oscParser.PopMessage();
					if (OnReceive != null)
						OnReceive(m, clientEndpoint);
				}
				_udp.BeginReceive(_callback, null);
			} catch (Exception e) {
				if (OnError != null)
					OnError(e);
			}
		}
		
		#region IDisposable implementation
		public void Dispose () {
			if (_disposed)
				return;
			_disposed = true;
			
			if (_udp != null) {
				_udp.Close();
				_udp = null;
			}
		}
		#endregion
		
		~OscServer() { Dispose(); }
	}
}