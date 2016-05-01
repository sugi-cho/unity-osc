using UnityEngine;
using System.Collections;

namespace Osc {

	public class OscPerformanceTest : MonoBehaviour {
		public const string OSC_PATH = "/path";

		public bool sending = true;
		public OscPort sender;
		public int numSendsPerFrame = 100;

		int _sendCount;
		int _receiveCount;

		void Start() {
			_sendCount = 0;
			_receiveCount = 0;
			StartCoroutine (Logger());
		}
		void Update () {
			if (sending) {
				for (var i = 0; i < numSendsPerFrame; i++) {
					_sendCount++;
					var oscdata = new MessageEncoder (OSC_PATH);
					oscdata.Add (_sendCount);
					sender.Send (oscdata);
				}
			}
		}
		
		public void OnReceive(OscPort.Capsule c) {
			_receiveCount++;
		}
		public void OnError(System.Exception e) {
			Debug.LogFormat ("Error {0}", e);
		}

		IEnumerator Logger() {
			while (true) {
				yield return new WaitForSeconds(1f);
				Debug.LogFormat ("Count {0}/{1}", _receiveCount, _sendCount);
			}
		}
	}
}
