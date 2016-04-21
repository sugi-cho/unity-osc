using System.Net.Sockets;
using System;
using System.Net;
using Osc;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace OSC {
	public class OscPort : MonoBehaviour {
		public CapsuleEvent OnReceive;
		public ExceptionEvent OnError;

		public int localPort = 0;
		public string defaultRemoteHost = "localhost";
		public int defaultRemotePort = 10000;
		public int limitReceiveBuffer = 10;
		
		AsyncCallback _callback;
		Parser _oscParser;
		Queue<Capsule> _received;
		Queue<System.Exception> _errors;

		UdpClient _udp;
		IPEndPoint _defaultRemote;

		void Awake() {
			_callback = new System.AsyncCallback (HandleReceive);
			_oscParser = new Parser ();
			_received = new Queue<Capsule> ();
			_errors = new Queue<Exception> ();
		}
		void OnEnable() {
			try {
				_defaultRemote = new IPEndPoint (FindFromHostName (defaultRemoteHost), defaultRemotePort);

				_udp = new UdpClient (localPort, AddressFamily.InterNetwork);
				_udp.BeginReceive(_callback, null);
			} catch (System.Exception e) {
				RaiseError (e);
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
				_udp.Close();
				_udp = null;
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
				_udp.Send (oscData, oscData.Length, remote);
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
			
		void RaiseError(System.Exception e) {
			_errors.Enqueue (e);
		}
		void HandleReceive(System.IAsyncResult ar) {
			try {
				if (_udp == null)
					return;
				var clientEndpoint = new IPEndPoint(0, 0);
				byte[] receivedData = _udp.EndReceive(ar, ref clientEndpoint);
				_oscParser.FeedData(receivedData);
				while (_oscParser.MessageCount > 0) {
					lock (_received) {
						var msg = _oscParser.PopMessage();
						if (limitReceiveBuffer > 0 && _received.Count < limitReceiveBuffer)
							_received.Enqueue(new Capsule(msg, clientEndpoint));
					}
				}
			} catch (Exception e) {
				RaiseError (e);
			}
			_udp.BeginReceive(_callback, null);
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