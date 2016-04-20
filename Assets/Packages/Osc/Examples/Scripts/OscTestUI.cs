using UnityEngine;
using System.Collections;

namespace Osc {
	
	public class OscTestUI : MonoBehaviour {
		public enum ConnectionEnum { None = 0, Server, Client }
		public static readonly string[] CONNECTION_LABELS = new string[]{ "Server", "Client" };

		public ConnectionEnum connection = ConnectionEnum.None;

		Rect _window = new Rect(10, 10, 200, 200);
		string _hostname = "localhost";
		string _portnumText = "10000";

		void OnGUI() {
			_window = GUILayout.Window (0, _window, Window, "UI");
		}

		void Window(int id) {
			GUILayout.BeginVertical ();

			switch (connection) {
			default:
				GUILayout.BeginHorizontal ();
				var startServer = GUILayout.Button ("Server");
				var startClient = GUILayout.Button ("Client");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Host name : ");
				_hostname = GUILayout.TextField (_hostname);
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Port number : ");
				_hostname = GUILayout.TextField (_hostname);
				GUILayout.EndHorizontal ();



				break;
			case ConnectionEnum.Server:
				break;
			case ConnectionEnum.Client:
				break;
			}

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}
	}
}