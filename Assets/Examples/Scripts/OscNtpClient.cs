using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using OSC.Simple;
using Osc;

public class OscNtpClient : MonoBehaviour {
	public float interval = 1f;
	public int nSamplesPOT = 64;
	public RingBuffer<NtpStat> stats;
	
	public OscClient client;

	// Use this for initialization
	void Start () {
		stats = new RingBuffer<NtpStat>(nSamplesPOT);
		client.OnError.AddListener (delegate(System.Exception obj) {
			Debug.LogError (obj);
		});
		client.OnReceive.AddListener((cap) => HandleReceived (cap.message));
		
		StartCoroutine("Request");
	}

	void HandleReceived(Message m) {
		if (m.path != OscNtpServer.NTP_RESPONSE)
			return;
			
		var t3 = HighResTime.UtcNow;
		var t0 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[0], 0)));
		var t1 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[1], 0)));
		var t2 = System.DateTime.FromBinary(IPAddress.NetworkToHostOrder(System.BitConverter.ToInt64((byte[])m.data[2], 0)));
		var roundtrip = NtpUtil.Roundtrip(t0, t1, t2, t3);
		var delay = NtpUtil.Delay(t0, t1, t2, t3);
		stats.Add(new NtpStat() { delay = delay, roundtrip = roundtrip });
		Debug.LogFormat("Receive Delay {0:e2}", delay);
	}
	
	IEnumerator Request() {
		while (true) {
			yield return new WaitForSeconds(interval);
			var oscEnc = new MessageEncoder(OscNtpServer.NTP_REQUEST);
			var now = HighResTime.UtcNow;
			var t0 = System.BitConverter.GetBytes(IPAddress.HostToNetworkOrder(now.ToBinary()));
			oscEnc.Add(t0);
			var bytedata = oscEnc.Encode();
			client.Send(bytedata);
			Debug.LogFormat("Client Send {0}", now);
		}
	}
	
	public struct NtpStat {
		public double delay;
		public double roundtrip;
	}
	
	public double AverageDelay() {
		var count = 0;
		var total = 0.0;
		foreach (var s in stats) {
			if (s.roundtrip <= 0)
				continue;
			count++;
			total += s.delay;
		}
		if (count > 0)
			total /= count;
		return total;
	}
}
