using System.Net.Sockets;
using System;
using System.Net;
using Osc;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Threading;

namespace Osc {
	public class OscPort : MonoBehaviour {
		public const int BUFFER_SIZE = 1 << 16;
		public CapsuleEvent OnReceive;
		public ExceptionEvent OnError;

		public int localPort = 0;
		public string defaultRemoteHost = "localhost";
		public int defaultRemotePort = 10000;
		public int limitReceiveBuffer = 10;
		
		Parser _oscParser;
		Queue<Capsule> _received;
		Queue<System.Exception> _errors;

		Socket _udp;
		byte[] _receiveBuffer;
		IPEndPoint _defaultRemote;

		Thread _reader;

		void Awake() {
			_oscParser = new Parser ();
			_received = new Queue<Capsule> ();
			_errors = new Queue<Exception> ();
		}
		void OnEnable() {
			try {
				_defaultRemote = new IPEndPoint (FindFromHostName (defaultRemoteHost), defaultRemotePort);

				_udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				_udp.Bind(new IPEndPoint(IPAddress.Any, localPort));

				_receiveBuffer = new byte[BUFFER_SIZE];

				_reader = new Thread(Reader);
				_reader.Start();
			} catch (System.Exception e) {
				RaiseError (e);
				enabled = false;
			}
		}
		void Update() {
			lock (_received)
				while (_received.Count > 0)
					OnReceive.Invoke (_received.Dequeue ());
			lock (_errors)
				while (_errors.Count > 0)
					OnError.Invoke (_errors.Dequeue ());
		}
		void OnDisable() {
			if (_udp != null) {
				_udp.Close ();
				_udp = null;
			}
			if (_reader != null) {
				_reader.Abort ();
				_reader = null;
			}
		}

		public void Send(MessageEncoder oscMessage) {
			Send (oscMessage, _defaultRemote);
		}
		public void Send(MessageEncoder oscMessage, IPEndPoint remote) {
			Send (oscMessage.Encode (), remote);
		}
		public void Send(byte[] oscData) {
			Send (oscData, _defaultRemote);
		}
		public void Send(byte[] oscData, IPEndPoint remote) {
			try {
				_udp.SendTo(oscData, remote);
			} catch (System.Exception e) {
				RaiseError (e);
			}
		}
		public IPAddress FindFromHostName(string hostname) {
			var addresses = Dns.GetHostAddresses (defaultRemoteHost);
			IPAddress address = IPAddress.None;
			for (var i = 0; i < addresses.Length; i++) {
				if (addresses[i].AddressFamily == AddressFamily.InterNetwork) {
					address = addresses[i];
					break;
				}
			}
			return address;
		}
			
		void Reader() {
			while (_udp != null) {
				try {
					var clientEndpoint = new IPEndPoint (IPAddress.Any, 0);
					var fromendpoint = (EndPoint)clientEndpoint;
					var length = _udp.ReceiveFrom(_receiveBuffer, ref fromendpoint);
					if (length == 0 || (clientEndpoint = fromendpoint as IPEndPoint) == null)
						continue;
					
					_oscParser.FeedData (_receiveBuffer, length);
					while (_oscParser.MessageCount > 0) {
						lock (_received) {
							var msg = _oscParser.PopMessage ();
							if (limitReceiveBuffer > 0 && _received.Count < limitReceiveBuffer)
								_received.Enqueue (new Capsule (msg, clientEndpoint));
						}
					}
				} catch (Exception e) {
					RaiseError (e);
				}
			}
		}
		void RaiseError(System.Exception e) {
			_errors.Enqueue (e);
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
	public class ExceptionEvent : UnityEvent<Exception> {}
	[System.Serializable]
	public class CapsuleEvent : UnityEvent<OscPort.Capsule> {}
	[System.Serializable]
	public class MessageEvent : UnityEvent<Message> {}
}