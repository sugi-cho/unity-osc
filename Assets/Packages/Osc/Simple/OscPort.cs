using System.Net.Sockets;
using System;
using System.Net;
using Osc;
using System.Collections.Generic;

namespace OSC.Simple {
	public abstract class OscPort : IDisposable, IEnumerable<OscPort.Capsule> {
		public event Action<Exception> OnError;
		
		protected readonly AsyncCallback _callback;
		protected readonly IPEndPoint _serverEndpoint;
		protected readonly Parser _oscParser;
		protected readonly Queue<Capsule> _received;

		protected UdpClient _udp;
		protected bool _disposed = false;

		public OscPort(IPEndPoint serverEndpoint) {
			_serverEndpoint = serverEndpoint;
			_udp = GenerateUdpClient(_serverEndpoint);
			_callback = new System.AsyncCallback(HandleReceive);
			_oscParser = new Parser();
			_received = new Queue<Capsule> ();
			
			_udp.BeginReceive(_callback, null);			
		}

		public abstract UdpClient GenerateUdpClient (IPEndPoint serverEndPoint);

		public void RaiseError(System.Exception e) {
			if (OnError != null)
				OnError (e);
		}
			
		private void HandleReceive(System.IAsyncResult ar) {
			try {
				if (_udp == null)
					return;
				var clientEndpoint = new IPEndPoint(0, 0);
				byte[] receivedData = _udp.EndReceive(ar, ref clientEndpoint);
				_oscParser.FeedData(receivedData);
				while (_oscParser.MessageCount > 0) {
					lock (_received)
						_received.Enqueue(new Capsule(_oscParser.PopMessage(), clientEndpoint));
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

		#region IEnumerable implementation
		public IEnumerator<Capsule> GetEnumerator () {
			while (true) {
				lock (_received) {
					if (_received.Count == 0)
						yield break;
					yield return _received.Dequeue();
				}
			}
		}
		#endregion

		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator (){
			return GetEnumerator();
		}
		#endregion
		
		~OscPort() { Dispose(); }

		public struct Capsule {
			public Message message;
			public IPEndPoint ip;

			public Capsule(Message message, IPEndPoint ip) {
				this.message = message;
				this.ip = ip;
			}
		}
	}
}