using System.Net.Sockets;
using System;
using System.Net;
using Osc;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace OSC {
	public class OscPort : MonoBehaviour {
		public enum SendModeEnum { Normal = 0, Broadcast }

		public CapsuleEvent OnReceive;
		public ExceptionEvent OnError;

		public SendModeEnum sendMode;
		public int localPort = 0;
		public string remoteHost = "localhost";
		public int remotePort = 10000;
		
		AsyncCallback _callback;
		Parser _oscParser;
		Queue<Capsule> _received;
		Queue<System.Exception> _errors;

		UdpClient _udp;
		IPEndPoint _remoteEndPoint;

		void Awake() {
			_callback = new System.AsyncCallback (HandleReceive);
			_oscParser = new Parser ();
			_received = new Queue<Capsule> ();
			_errors = new Queue<Exception> ();
		}
		void OnEnable() {
			try {
				switch (sendMode) {
				default:
					_remoteEndPoint = new IPEndPoint (FindFromHostName (remoteHost), remotePort);
					break;
				case SendModeEnum.Broadcast:
					_remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, remotePort);
					break;
				}

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
			Send (oscMessage.Encode ());
		}
		public void Send(byte[] oscData) {
			try {
				_udp.Send (oscData, oscData.Length, _remoteEndPoint);
			} catch (System.Exception e) {
				RaiseError (e);
			}
		}
		public IPAddress FindFromHostName(string hostname) {
			var addresses = Dns.GetHostAddresses (remoteHost);
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
					lock (_received)
						_received.Enqueue(new Capsule(_oscParser.PopMessage(), clientEndpoint));
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