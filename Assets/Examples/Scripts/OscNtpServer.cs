using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using OSC.Simple;
using Osc;

public class OscNtpServer : MonoBehaviour {
	public const string NTP_REQUEST = "/ntp/request";
	public const string NTP_RESPONSE = "/ntp/response";	
	
	public int listenPort;
	
	private OscServer _server;
	private Queue<NtpRequest> _requests;

	// Use this for initialization
	void Start () {
		_requests = new Queue<NtpRequest>();
		var serverEndpoint = new IPEndPoint(IPAddress.Any, listenPort);
		_server = new OscServer(serverEndpoint);
		_server.OnError += delegate(System.Exception obj) {
			Debug.Log(obj);
		};	
	}
	
	// Update is called once per frame
	void Update () {
		try {
			lock (_requests) {
				while (_requests.Count > 0) {
					var req = _requests.Dequeue();
					var oscEnc = new MessageEncoder(NTP_RESPONSE);
					oscEnc.Add(req.t0);
					oscEnc.Add(req.t1);
					var now = HighResTime.UtcNow;
					var t2 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(now.ToBinary()));
					oscEnc.Add(t2);
					var bytedata = oscEnc.Encode();
					_server.Send(bytedata, req.remote);
					Debug.LogFormat("Server Send {0}", now);
				}
			}
		} catch (System.Exception e) {
			Debug.Log(e);
		}

		foreach (var cap in _server)
			HandleReceived (cap.message, cap.ip);
	}
		
	void HandleReceived(Message m, IPEndPoint remoteEndpoint) {
		if (m.path != NTP_REQUEST)
			return;
		
		lock (_requests) {
			_requests.Enqueue(new NtpRequest() {
				remote = remoteEndpoint,
				t0 = (byte[])m.data[0],
				t1 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(HighResTime.UtcNow.ToBinary())),
			});
		}
	}
	
	void OnDestroy() {
		if (_server != null) {
			_server.Dispose();
			_server = null;
		}
	}
	
	public struct NtpRequest {
		public IPEndPoint remote;
		public byte[] t0;
		public byte[] t1;
	}
}
