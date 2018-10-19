using UnityEngine;
using Windows.Kinect;

namespace CaveAsset
{
	namespace Kinect
	{
		public class KinectController : MonoBehaviour
		{
			[Header("Options")]
			[Tooltip("The body which is the closest to that position is used for camera head tracking")]
			public Vector3 centerPosition = new Vector3(0.0f, 1.7f, 0.0f);
			[Tooltip("This is the position where the Kinect camera is mounted inside the CAVE")]
			public Vector3 kinectCameraPosition = new Vector3(0.0f, 2.5f, 1.5f);

			private KinectSensor kinectSensor;
			private MultiSourceFrameReader frameReader;
			private Body[] bodyData;
			private Floor floor;

			private bool isNewFrame;
			private int centerBodyIndex;

			private void Start()
			{
				kinectSensor = KinectSensor.GetDefault();

				if (kinectSensor != null)
				{
					frameReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.BodyIndex | FrameSourceTypes.Depth);

					if (!kinectSensor.IsOpen)
						kinectSensor.Open();
				}

				isNewFrame = false;

				centerBodyIndex = -1;
			}

			private void Update()
			{
				if (frameReader != null)
				{
					MultiSourceFrame multiSourceFrame = frameReader.AcquireLatestFrame();

					if (multiSourceFrame != null)
					{
						BodyFrame bodyFrame = multiSourceFrame.BodyFrameReference.AcquireFrame();

						if (bodyFrame != null)
						{
							if (bodyData == null)
								bodyData = new Body[kinectSensor.BodyFrameSource.BodyCount];

							bodyFrame.GetAndRefreshBodyData(bodyData);

							floor = new Floor(bodyFrame.FloorClipPlane);

							bodyFrame.Dispose();
							bodyFrame = null;
						}

						DetectCenterBody();

						multiSourceFrame = null;

						isNewFrame = true;
					}
					else
						isNewFrame = false;
				}
			}

			private void OnApplicationQuit()
			{
				if (frameReader != null)
				{
					frameReader.Dispose();
					frameReader = null;
				}

				if (kinectSensor != null)
				{
					if (kinectSensor.IsOpen)
						kinectSensor.Close();

					kinectSensor = null;
				}
			}

			private void DetectCenterBody()
			{
				float shortestDistanceToCenter = float.PositiveInfinity;
				Vector3 headPosition = Vector3.zero;

				for (int i = 0; i < bodyData.Length; ++i)
				{
					if (bodyData[i].IsTracked)
					{
						headPosition = JointPositionToCAVESpace(bodyData[i].Joints[JointType.Head].Position);

						float distanceToCheck = (headPosition - centerPosition).magnitude;

						if (distanceToCheck < shortestDistanceToCenter)
						{
							shortestDistanceToCenter = distanceToCheck;
							centerBodyIndex = i;
						}
					}
				}
			}
			
			private Vector3 JointPositionToCAVESpace(CameraSpacePoint cameraSpacePoint)
			{
				Vector3 cameraPoint = new Vector3
				{
					x = kinectCameraPosition.x + cameraSpacePoint.X,
					y = floor.DistanceFromCameraSpacePoint(cameraSpacePoint),
					z = kinectCameraPosition.z - cameraSpacePoint.Z
				};

				return cameraPoint;
			}

			public bool IsNewFrame()
			{
				return isNewFrame;
			}

			public Body[] GetBodyData()
			{
				return bodyData;
			}

			public Vector3 GetJointPosition(JointType jointType)
			{
				Vector3 jointPosition = Vector3.zero;

				if (bodyData != null)
				{
					if (centerBodyIndex == -1)
						return jointPosition;

					jointPosition = JointPositionToCAVESpace(bodyData[centerBodyIndex].Joints[jointType].Position);
				}

				return jointPosition;
			}
		}
	}
}
