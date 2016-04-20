using System.Net.Sockets;
using System;
using System.Net;
using Osc;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace OSC.Simple {
	public abstract class OscPort : MonoBehaviour {
		public CapsuleEvent OnReceive;
		public ExceptionEvent OnError;
		
		protected AsyncCallback _callback;
		protected IPEndPoint _serverEndpoint;
		protected Parser _oscParser;
		protected Queue<Capsule> _received;

		protected UdpClient _udp;
		protected bool _disposed = false;

		protected virtual void Init(IPEndPoint serverEndpoint) {
			_serverEndpoint = serverEndpoint;
			_udp = GenerateUdpClient(_serverEndpoint);
			_callback = new System.AsyncCallback(HandleReceive);
			_oscParser = new Parser();
			_received = new Queue<Capsule> ();
			
			_udp.BeginReceive(_callback, null);			
		}

		public abstract UdpClient GenerateUdpClient (IPEndPoint serverEndPoint);

		public void RaiseError(System.Exception e) {
			OnError.Invoke(e);
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
			} catch (Exception e) {
				OnError.Invoke(e);
			}
			_udp.BeginReceive(_callback, null);
		}

		void Update() {
			while (true) {
				lock (_received) {
					if (_received.Count == 0)
						break;
					OnReceive.Invoke(_received.Dequeue());
				}
			}
		}
		
		void OnDestroy() {
			if (_udp != null) {
				_udp.Close();
				_udp = null;
			}
		}

		public struct Capsule {
			public Message message;
			public IPEndPoint ip;

			public Capsule(Message message, IPEndPoint ip) {
				this.message = message;
				this.ip = ip;
			}
		}
	}

	[System.Serializable]
	public class ExceptionEvent : UnityEvent<Exception> { }
	[System.Serializable]
	public class CapsuleEvent : UnityEvent<OscPort.Capsule> {}
}