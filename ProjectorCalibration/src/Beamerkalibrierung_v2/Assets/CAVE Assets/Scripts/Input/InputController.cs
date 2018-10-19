using CaveAsset.CavePlayer;
using CaveAsset.Kinect;
using CaveAsset.Wii;
using CaveAsset.Frustum;
using UnityEngine;

namespace CaveAsset
{
	namespace Input
	{
		public class InputController : MonoBehaviour
		{
			[Header("Input Typs")]
			[Tooltip("Enables the classic input way with WSAD and mouse look")]
			public bool enableMouseAndKeyboard = false;

			[Header("Input Sensitivity")]
			[Tooltip("Walking speed modifier")]
			public float walkingSpeed = 5.0f;
			[Tooltip("Turning speed modifier")]
			public float turningSpeed = 3.0f;

			private CavePlayerController cavePlayerController = null;
			private KinectController kinectController = null;
			private WiiController wiiController = null;

			private ProjectionMatrix[] projectionMatrices;

			private void Awake()
			{
				cavePlayerController = GetComponent<CavePlayerController>();
				kinectController = GetComponent<KinectController>();
				wiiController = GetComponent<WiiController>();

				projectionMatrices = GetComponentsInChildren<ProjectionMatrix>();

				if (cavePlayerController.testMode)
				{
					kinectController.enabled = false;
					wiiController.enabled = false;
				}
			}

			private void Update()
			{
				if (enableMouseAndKeyboard)
				{
					//MouseAndKeyboardMovement();

					if (UnityEngine.Input.GetKeyDown(KeyCode.Z))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SwitchSaveMode();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.P))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SelectNextScreen();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.O))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SelectNextCornor();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.K))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SwitchStepSize();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepLeft();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepRight();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepUp();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepDown();
					}

					if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
						Application.Quit();
				}

				if (!cavePlayerController.testMode)
				{
					//WiimoteMovement();

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.PLUS) || wiiController.GetWiimoteButtonDown(WiiRemoteButton.MINUS))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SwitchSaveMode();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.ONE))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SelectNextScreen();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.TWO))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SelectPreviousScreen();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.A))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SelectNextCornor();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.B))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.SwitchStepSize();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.CROSS_LEFT))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepLeft();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.CROSS_RIGHT))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepRight();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.CROSS_UP))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepUp();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.CROSS_DOWN))
					{
						foreach (ProjectionMatrix projectionMatrix in projectionMatrices)
							projectionMatrix.StepDown();
					}

					if (wiiController.GetWiimoteButtonDown(WiiRemoteButton.HOME))
						Application.Quit();
				}
			}

			private void MouseAndKeyboardMovement()
			{
				float strafe = UnityEngine.Input.GetAxis("Horizontal") * Time.deltaTime * walkingSpeed;
				float forwardBackward = UnityEngine.Input.GetAxis("Vertical") * Time.deltaTime * walkingSpeed;
				float mouseX = UnityEngine.Input.GetAxis("Mouse X") * turningSpeed;

				transform.Translate(strafe, 0.0f, forwardBackward);
				transform.Rotate(0.0f, mouseX, 0.0f);
			}

			private void WiimoteMovement()
			{
				float rotateY = 0.0f;
				float forwardBackward = 0.0f;

				if (wiiController.GetWiimoteButtonHold(WiiRemoteButton.CROSS_UP))
					forwardBackward = Time.deltaTime * walkingSpeed;
				if (wiiController.GetWiimoteButtonHold(WiiRemoteButton.CROSS_DOWN))
					forwardBackward = -1.0f * Time.deltaTime * walkingSpeed;
				if (wiiController.GetWiimoteButtonHold(WiiRemoteButton.CROSS_RIGHT))
					rotateY = turningSpeed;
				if (wiiController.GetWiimoteButtonHold(WiiRemoteButton.CROSS_LEFT))
					rotateY = -1.0f * turningSpeed;

				transform.Translate(0.0f, 0.0f, forwardBackward);
				transform.Rotate(0.0f, rotateY, 0.0f);
			}

			public KinectController GetKinectController()
			{
				if (cavePlayerController.testMode)
					return null;
				else
					return kinectController;
			}

			public WiiController GetWiiController()
			{
				if (cavePlayerController.testMode)
					return null;
				else
					return wiiController;
			}
		}
	}
}
