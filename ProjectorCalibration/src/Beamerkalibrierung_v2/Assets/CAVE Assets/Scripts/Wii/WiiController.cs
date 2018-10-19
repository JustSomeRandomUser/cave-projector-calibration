using UnityEngine;
using System.Collections;
using WiimoteApi;

namespace CaveAsset
{
	namespace Wii
	{
		public class WiiController : MonoBehaviour
		{
			private Wiimote wiimote = null;
			private WiiRemote wiiRemote = null;

			private void Start()
			{
				wiiRemote = new WiiRemote();
			}

			private void Update()
			{
				if (wiimote == null)
				{
					if (WiimoteManager.HasWiimote())
						wiimote = WiimoteManager.Wiimotes[0];
					else
						WiimoteManager.FindWiimotes();
				}
				else
				{
					while (wiimote.ReadWiimoteData() > 0)
						;

					wiiRemote.Update(wiimote.Button);
				}
			}

			private void OnApplicationQuit()
			{
				if (wiimote != null)
					WiimoteManager.Cleanup(wiimote);
			}

			private IEnumerator VibrateWiimoteCoroutine(float timeInMilliSeconds)
			{
				wiimote.RumbleOn = true;
				wiimote.SendStatusInfoRequest();

				yield return new WaitForSeconds(timeInMilliSeconds / 1000.0f);

				wiimote.RumbleOn = false;
				wiimote.SendStatusInfoRequest();
			}

			public void VibrateWiimote(float timeInMilliSeconds = 500.0f)
			{
				StopCoroutine(VibrateWiimoteCoroutine(timeInMilliSeconds));
				StartCoroutine(VibrateWiimoteCoroutine(timeInMilliSeconds));
			}

			public bool GetWiimoteButtonDown(WiiRemoteButton wiiRemoteButton)
			{
				return wiiRemote.GetButton(wiiRemoteButton, WiiRemoteButtonState.DOWN);
			}

			public bool GetWiimoteButtonHold(WiiRemoteButton wiiRemoteButton)
			{
				return wiiRemote.GetButton(wiiRemoteButton, WiiRemoteButtonState.HOLD);
			}

			public bool GetWiimoteButtonUp(WiiRemoteButton wiiRemoteButton)
			{
				return wiiRemote.GetButton(wiiRemoteButton, WiiRemoteButtonState.UP);
			}
		}
	}
}
