using UnityEngine;
using System.Collections;

namespace Osc {

	public class UtcClock : MonoBehaviour {
		public const float SEC2ANGLE = 360f;
		public const long MILLISECOND_IN_TICKS = 10000L;
		public const double TICKS2SECOND = 1e-7;
		
		public OscNtpClient client;
		public GUIText text;
		public Transform clock;
		public BeatInfo[] beats;
		public float speed = 1f;
		
		private bool _synch = true;

		void Start () {
			if (text == null)
				text = GetComponent<GUIText>();
			
			var dspEndTime = AudioSettings.dspTime;
			foreach (var bi in beats) {
				bi.source = gameObject.AddComponent<AudioSource>();
				bi.source.bypassEffects = true;
				bi.source.clip = bi.clip;
				bi.dspEndTime = dspEndTime;
			}
		}
		
		void OnGUI() {
			_synch = GUILayout.Toggle(_synch, "Sync");
		}

		void Update() {
			var now = HighResTime.UtcNow;
			var delay = client.AverageDelay();
			if (_synch)
				now = now.AddSeconds(delay);
			
			text.text = string.Format("{0}\nDelay {1:E3}", now, delay);
			var angle = (float)(speed * SEC2ANGLE * (System.DateTime.Today - now).TotalSeconds);
			clock.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
			
			var dspTime = AudioSettings.dspTime;
			foreach (var bi in beats) {
				if (bi.dspEndTime < dspTime) {
					var intervalInTicks = bi.intervalInMills * MILLISECOND_IN_TICKS;
					var dt = (intervalInTicks - (now.Ticks % intervalInTicks)) *TICKS2SECOND;
					var startDspTime = dspTime + dt;
					bi.dspEndTime = startDspTime + bi.source.clip.length;
					bi.source.PlayScheduled(startDspTime);
				}
			}
		}
		
		[System.Serializable]
		public class BeatInfo {
			public int intervalInMills = 1000;
			public AudioClip clip;
			[HideInInspector]
			public AudioSource source;
			[HideInInspector]
			public double dspEndTime;
		}
	}
}
