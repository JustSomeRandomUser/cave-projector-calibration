using System.Collections.Generic;
using WiimoteApi;

namespace CaveAsset
{
	namespace Wii
	{
		public class WiiRemote
		{
			private Dictionary<WiiRemoteButton, WiiRemoteButtonState> buttonStates = null;

			public WiiRemote()
			{
				buttonStates = new Dictionary<WiiRemoteButton, WiiRemoteButtonState>()
			{
				{ WiiRemoteButton.A, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.B, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.HOME, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.ONE, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.TWO, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.PLUS, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.MINUS, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.CROSS_LEFT, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.CROSS_RIGHT, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.CROSS_UP, WiiRemoteButtonState.NONE },
				{ WiiRemoteButton.CROSS_DOWN, WiiRemoteButtonState.NONE }
			};
			}

			public void Update(ButtonData buttonData)
			{
				UpdateButtonState(WiiRemoteButton.A, buttonData.a);
				UpdateButtonState(WiiRemoteButton.B, buttonData.b);
				UpdateButtonState(WiiRemoteButton.HOME, buttonData.home);
				UpdateButtonState(WiiRemoteButton.ONE, buttonData.one);
				UpdateButtonState(WiiRemoteButton.TWO, buttonData.two);
				UpdateButtonState(WiiRemoteButton.PLUS, buttonData.plus);
				UpdateButtonState(WiiRemoteButton.MINUS, buttonData.minus);
				UpdateButtonState(WiiRemoteButton.CROSS_LEFT, buttonData.d_left);
				UpdateButtonState(WiiRemoteButton.CROSS_RIGHT, buttonData.d_right);
				UpdateButtonState(WiiRemoteButton.CROSS_UP, buttonData.d_up);
				UpdateButtonState(WiiRemoteButton.CROSS_DOWN, buttonData.d_down);
			}

			public bool GetButton(WiiRemoteButton wiiRemoteButton, WiiRemoteButtonState wiiRemoteButtonState)
			{
				if (buttonStates[wiiRemoteButton] == wiiRemoteButtonState)
					return true;
				else
					return false;
			}

			private void UpdateButtonState(WiiRemoteButton wiiRemoteButton, bool newState)
			{
				if (newState)
				{
					if (buttonStates[wiiRemoteButton] == WiiRemoteButtonState.NONE || buttonStates[wiiRemoteButton] == WiiRemoteButtonState.UP)
						buttonStates[wiiRemoteButton] = WiiRemoteButtonState.DOWN;
					else if (buttonStates[wiiRemoteButton] == WiiRemoteButtonState.DOWN)
						buttonStates[wiiRemoteButton] = WiiRemoteButtonState.HOLD;
				}
				else
				{
					if (buttonStates[wiiRemoteButton] == WiiRemoteButtonState.DOWN || buttonStates[wiiRemoteButton] == WiiRemoteButtonState.HOLD)
						buttonStates[wiiRemoteButton] = WiiRemoteButtonState.UP;
					else if (buttonStates[wiiRemoteButton] == WiiRemoteButtonState.UP)
						buttonStates[wiiRemoteButton] = WiiRemoteButtonState.NONE;
				}
			}
		}
	}
}
