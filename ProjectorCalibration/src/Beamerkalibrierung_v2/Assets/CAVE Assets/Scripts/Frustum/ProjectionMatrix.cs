using CaveAsset.CavePlayer;
using CaveAsset.VirtualScreens;
using Emgu.CV;
using System;
using System.IO;
using UnityEngine;

namespace CaveAsset
{
	namespace Frustum
	{
		public class ProjectionMatrix : MonoBehaviour
		{
			[Header("Bimber Matrix File Name")]
			[Tooltip("Name of the (.txt) file which contains the bimber matrix")]
			public string bimberMatrixFile = "";
			[Header("Virtual Screen Settings")]
			[Tooltip("Virtual screen for which the projection matrix is to be calculated")]
			public VirtualScreen virtualScreen;
			[Header("Head Up Display")]
			public HUD hud;

			private Camera cam;
			private CavePlayerController cavePlayerController;

			private Matrix4x4 bimberMatrix = Matrix4x4.identity;
			private Matrix4x4 perspectiveMatrix = Matrix4x4.identity;
			private Matrix4x4 perspectiveMatrixLeftEye = Matrix4x4.identity;
			private Matrix4x4 perspectiveMatrixRightEye = Matrix4x4.identity;

			private GameObject eyeAnchor;
			private GameObject leftEye;
			private GameObject rightEye;

			private Vector2[] cornorPoints;

			private readonly string[] cameraNames = { "Left", "Front", "Right", "Bottom" };
			private const float SMALL_STEP_SIZE = 0.001f;
			private const float LARGE_STEP_SIZE = 0.01f;
			private int currentlyScreenSelected = 0;
			private int currentlyCornorSelected = 0;
			private float stepSize = 0.001f;
			private bool shouldSave = false;

			private void Start()
			{
				cam = GetComponent<Camera>();
				cavePlayerController = GetComponentInParent<CavePlayerController>();

				eyeAnchor = new GameObject("EyeAnchor");
				eyeAnchor.transform.SetParent(transform.parent);
				eyeAnchor.transform.localPosition = Vector3.zero;
				eyeAnchor.transform.localRotation = transform.localRotation;

				leftEye = new GameObject("LeftEye");
				leftEye.transform.SetParent(eyeAnchor.transform);
				leftEye.transform.localPosition = new Vector3(-cam.stereoSeparation / 2.0f, 0.0f, 0.0f);
				leftEye.transform.localRotation = Quaternion.identity;

				rightEye = new GameObject("RightEye");
				rightEye.transform.SetParent(eyeAnchor.transform);
				rightEye.transform.localPosition = new Vector3(cam.stereoSeparation / 2.0f, 0.0f, 0.0f);
				rightEye.transform.localRotation = Quaternion.identity;

				LoadHomographyMatrix();
			}

			public void SwitchSaveMode()
			{
				shouldSave = !shouldSave;

				if (tag == cameraNames[currentlyScreenSelected])
				{
					if (shouldSave)
						hud.SetText("Save-mode:\nYes");
					else
						hud.SetText("Save-mode:\nNo");
				}
			}

			public void SelectNextScreen()
			{
				++currentlyScreenSelected;

				if (currentlyScreenSelected > 3)
					currentlyScreenSelected = 0;

				if (tag == cameraNames[currentlyScreenSelected])
					hud.SetActiveScreen(GetComponent<Camera>());
			}

			public void SelectPreviousScreen()
			{
				--currentlyScreenSelected;

				if (currentlyScreenSelected < 0)
					currentlyScreenSelected = 3;

				if (tag == cameraNames[currentlyScreenSelected])
					hud.SetActiveScreen(GetComponent<Camera>());
			}

			public void SelectNextCornor()
			{
				++currentlyCornorSelected;

				if (currentlyCornorSelected > 3)
					currentlyCornorSelected = 0;

				if (tag == cameraNames[currentlyScreenSelected])
					hud.SetActiveCornor(currentlyCornorSelected);
			}

			public void SelectPreviousCornor()
			{
				--currentlyCornorSelected;

				if (currentlyCornorSelected < 0)
					currentlyCornorSelected = 3;

				if (tag == cameraNames[currentlyScreenSelected])
					hud.SetActiveCornor(currentlyCornorSelected);
			}

			public void SwitchStepSize()
			{
				if (stepSize == SMALL_STEP_SIZE)
					stepSize = LARGE_STEP_SIZE;
				else
					stepSize = SMALL_STEP_SIZE;

				if (tag == cameraNames[currentlyScreenSelected])
				{
					if (stepSize == SMALL_STEP_SIZE)
						hud.SetCornorSize(true);
					else
						hud.SetCornorSize(false);
				}
			}

			public void StepLeft()
			{
				if (tag == cameraNames[currentlyScreenSelected])
				{
					cornorPoints[currentlyCornorSelected].x -= stepSize;

					CalculateBimberMatrix();
				}
			}

			public void StepRight()
			{
				if (tag == cameraNames[currentlyScreenSelected])
				{
					cornorPoints[currentlyCornorSelected].x += stepSize;

					CalculateBimberMatrix();
				}
			}

			public void StepUp()
			{
				if (tag == cameraNames[currentlyScreenSelected])
				{
					cornorPoints[currentlyCornorSelected].y += stepSize;

					CalculateBimberMatrix();
				}
			}

			public void StepDown()
			{
				if (tag == cameraNames[currentlyScreenSelected])
				{
					cornorPoints[currentlyCornorSelected].y -= stepSize;

					CalculateBimberMatrix();
				}
			}

			private void OnApplicationQuit()
			{
				if (shouldSave)
					SaveFiles();
			}

			private void OnPreCull()
			{
				if (cavePlayerController.testMode)
				{
					perspectiveMatrix = PerspectiveOffCenter(CalculateNearPlane(virtualScreen, transform));

					cam.projectionMatrix = bimberMatrix * perspectiveMatrix;
				}
				else
				{
					eyeAnchor.transform.rotation = transform.rotation;

					perspectiveMatrixLeftEye = PerspectiveOffCenter(CalculateNearPlane(virtualScreen, leftEye.transform));
					perspectiveMatrixRightEye = PerspectiveOffCenter(CalculateNearPlane(virtualScreen, rightEye.transform));

					cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, bimberMatrix * perspectiveMatrixLeftEye);
					cam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, bimberMatrix * perspectiveMatrixRightEye);
				}
			}

			private void CalculateBimberMatrix()
			{
				CreatePixelMask(new Quad(cornorPoints));

				Vector2[] src = { new Vector2(-1.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, -1.0f), new Vector2(-1.0f, -1.0f) };
				Vector2[] des = new Vector2[cornorPoints.Length];

				for (int i = 0; i < des.Length; ++i)
				{
					des[i] = cornorPoints[i] * 2.0f;
					des[i] -= Vector2.one;
				}

				bimberMatrix = GetBimberMatrixFormHomography(GetHomography(src, des));
			}

			private Matrix4x4 GetBimberMatrixFormHomography(Matrix<double> homography)
			{
				Matrix4x4 bimber = new Matrix4x4();

				bimber[0] = Convert.ToSingle(homography[0, 0]);
				bimber[1] = Convert.ToSingle(homography[1, 0]);
				bimber[2] = 0.0f;
				bimber[3] = Convert.ToSingle(homography[2, 0]);
				bimber[4] = Convert.ToSingle(homography[0, 1]);
				bimber[5] = Convert.ToSingle(homography[1, 1]);
				bimber[6] = 0.0f;
				bimber[7] = Convert.ToSingle(homography[2, 1]);
				bimber[8] = 0.0f;
				bimber[9] = 0.0f;
				bimber[10] = 1 - (bimber[3] < 0.0f ? -bimber[3] : bimber[3]) - (bimber[7] < 0.0f ? -bimber[7] : bimber[7]);
				bimber[11] = 0.0f;
				bimber[12] = Convert.ToSingle(homography[0, 2]);
				bimber[13] = Convert.ToSingle(homography[1, 2]);
				bimber[14] = 0.0f;
				bimber[15] = 1.0f;

				return bimber;
			}

			private Matrix<double> GetHomography(Vector2[] src, Vector2[] dest)
			{
				Matrix<double> srcMatrix;
				Matrix<double> destMatrix;
				Matrix<double> homography = new Matrix<double>(3, 3);

				{
					double[,] sourcePoints =
					{
				{ src[0].x, src[0].y },
				{ src[1].x, src[1].y },
				{ src[2].x, src[2].y },
				{ src[3].x, src[3].y }
			};
					srcMatrix = new Matrix<double>(sourcePoints);
				}

				{
					double[,] destinationPoints =
					{
				{ dest[0].x, dest[0].y },
				{ dest[1].x, dest[1].y },
				{ dest[2].x, dest[2].y },
				{ dest[3].x, dest[3].y }
			};
					destMatrix = new Matrix<double>(destinationPoints);
				}

				CvInvoke.FindHomography(srcMatrix, destMatrix, homography, Emgu.CV.CvEnum.HomographyMethod.Default);

				return homography;
			}

			private Matrix4x4 PerspectiveOffCenter(NearPlane np)
			{
				float x = (2.0f * np.near) / (np.right - np.left);
				float y = (2.0f * np.near) / (np.top - np.bottom);
				float a = (np.right + np.left) / (np.right - np.left);
				float b = (np.top + np.bottom) / (np.top - np.bottom);
				float c = -(np.far + np.near) / (np.far - np.near);
				float d = -(2.0f * np.far * np.near) / (np.far - np.near);
				float e = -1.0f;

				Matrix4x4 m = new Matrix4x4();

				m[0, 0] = x;
				m[0, 1] = 0;
				m[0, 2] = a;
				m[0, 3] = 0;
				m[1, 0] = 0;
				m[1, 1] = y;
				m[1, 2] = b;
				m[1, 3] = 0;
				m[2, 0] = 0;
				m[2, 1] = 0;
				m[2, 2] = c;
				m[2, 3] = d;
				m[3, 0] = 0;
				m[3, 1] = 0;
				m[3, 2] = e;
				m[3, 3] = 0;

				return m;
			}

			private NearPlane CalculateNearPlane(VirtualScreen window, Transform targetTransform)
			{
				NearPlane nearPlane = new NearPlane();

				Vector3 nearCenter = targetTransform.position + targetTransform.forward * cam.nearClipPlane;
				Plane plane = new Plane(-targetTransform.forward, nearCenter);
				float distance = 0.0f;
				Vector3 direction;
				Ray ray;

				// calculate top left for nearPlane
				direction = (window.topLeft - targetTransform.position).normalized;
				ray = new Ray(targetTransform.position, direction);
				plane.Raycast(ray, out distance);

				Vector3 nearTopLeft = -(targetTransform.InverseTransformPoint(nearCenter) - targetTransform.InverseTransformPoint((targetTransform.position + direction * distance)));
				nearPlane.left = nearTopLeft.x;
				nearPlane.top = nearTopLeft.y;

				// calculate bottom right for nearPlane
				direction = (window.bottomRight - targetTransform.position).normalized;
				ray = new Ray(targetTransform.position, direction);
				plane.Raycast(ray, out distance);

				Vector3 nearBottomRight = -(targetTransform.InverseTransformPoint(nearCenter) - targetTransform.InverseTransformPoint((targetTransform.position + direction * distance)));
				nearPlane.right = nearBottomRight.x;
				nearPlane.bottom = nearBottomRight.y;

				// near and far clipPlane for nearPlane
				nearPlane.near = cam.nearClipPlane;
				nearPlane.far = cam.farClipPlane;

				return nearPlane;
			}

			private void CreatePixelMask(Quad frame)
			{
				Texture2D texture = new Texture2D(cam.pixelWidth, cam.pixelHeight);

				Color[] pixels = texture.GetPixels();

				for (int x = 0; x < texture.width; ++x)
				{
					for (int y = 0; y < texture.height; ++y)
					{
						int pos = (y * texture.width) + x;

						pixels[pos] = Color.black;

						Vector2 position = new Vector2((float)x / texture.width, (float)y / texture.height);

						if (frame.Contains(position))
							pixels[pos].a = 0.0f;
					}
				}

				texture.SetPixels(pixels);
				texture.Apply();

				GetComponent<UnityStandardAssets.ImageEffects.ScreenOverlay>().texture = texture;
			}

			private void SaveFiles()
			{
				Vector2[] pointsTemp = cornorPoints;
				string matrixFileName = cavePlayerController.pathToBimberMatrix + bimberMatrixFile;

				try
				{
					using (StreamWriter writer = new StreamWriter(new FileStream(matrixFileName, FileMode.OpenOrCreate)))
					{
						for (int i = 0; i < 16; ++i)
							writer.WriteLine(bimberMatrix[i]);

						for (int i = 0; i < 4; ++i)
						{
							pointsTemp[i] *= 2.0f;
							pointsTemp[i] -= Vector2.one;

							writer.WriteLine(pointsTemp[i].x);
							writer.WriteLine(pointsTemp[i].y);
						}
					}
				}
				catch (System.IO.IsolatedStorage.IsolatedStorageException e)
				{
					Debug.LogException(e);
				}
			}

			private void LoadHomographyMatrix()
			{
				string matrixFileName = cavePlayerController.pathToBimberMatrix + bimberMatrixFile;
				Matrix4x4 bimber = Matrix4x4.identity;

				try
				{
					bimberMatrix = new Matrix4x4();

					using (StreamReader reader = new StreamReader(new FileStream(matrixFileName, FileMode.Open)))
					{
						for (int i = 0; i < 16; i++)
							bimberMatrix[i] = float.Parse(reader.ReadLine());

						cornorPoints = new Vector2[4];

						for (int i = 0; i < 4; i++)
						{
							cornorPoints[i].x = float.Parse(reader.ReadLine());
							cornorPoints[i].y = float.Parse(reader.ReadLine());

							cornorPoints[i] += Vector2.one;
							cornorPoints[i] /= 2.0f;
						}

						CreatePixelMask(new Quad(cornorPoints));
					}
				}
				catch (System.IO.IsolatedStorage.IsolatedStorageException e)
				{
					Debug.LogException(e);
				}
			}
		}
	}
}
