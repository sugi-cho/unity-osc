using UnityEngine;
using System.Collections;
using OSC.Simple;

namespace Osc {
	
	public class OscTestUI : MonoBehaviour {
		public const string OSC_PATH = "/data";

		public OscServer server;
		public OscClient client;

		Rect _window = new Rect(10, 10, 200, 200);
		int _counter = 0;

		void Update() {
			client.enabled = !server.enabled;
		}
		void OnGUI() {
			_window = GUILayout.Window (0, _window, Window, "UI");
		}

		public void OnReceive(OscServer.Capsule c) {
			if (c.message.path == OSC_PATH)
				_counter++;
		}
		public void OnError(System.Exception e) {
			Debug.LogFormat ("Exception {0}", e);
		}

		void Window(int id) {
			GUILayout.BeginVertical ();

			if (GUILayout.Button (server.enabled ? "Client" : "Server")) {
				server.enabled = !server.enabled;
				client.enabled = !server.enabled;
			}

			GUI.enabled = client.enabled;
			if (GUILayout.Button ("Count")) {
				var osc = new Osc.MessageEncoder (OSC_PATH);
				client.Send (osc.Encode ());
			}
			GUI.enabled = true;
			GUILayout.Label (string.Format ("Counter {0}", _counter));

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}
	}
}