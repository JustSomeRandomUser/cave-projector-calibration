using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	public Text text;
	public RawImage rawImage;
	public Texture arrowSmall;
	public Texture arrowLarge;

	private Canvas canvas;

	private void Start()
	{
		canvas = GetComponent<Canvas>();
	}

	public void SetText(string newText)
	{
		text.text = newText;
	}

	public void SetTextColor(Color color)
	{
		text.color = color;
	}

	// left-top: 0, right-top: 1, right-bottom: 2, left-bottom: 3
	public void SetActiveCornor(int cornor)
	{
		if (cornor == 0)
			rawImage.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		else if (cornor == 1)
			rawImage.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
		else if (cornor == 2)
			rawImage.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
		else if (cornor == 3)
			rawImage.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
	}

	public void SetCornorSize(bool small)
	{
		if (small)
			rawImage.texture = arrowSmall;
		else
			rawImage.texture = arrowLarge;
	}

	public void SetActiveScreen(Camera camera)
	{
		canvas.worldCamera = camera;
	}
}
