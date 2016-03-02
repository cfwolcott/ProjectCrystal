using UnityEngine;
using System.Collections;

public class Strobe : MonoBehaviour {
	public Color FlashColor = Color.white;
	public float FlashIntensity = 8.0f;
	public int FlashPeriod = 120;
	public int FlashDuration = 2;
	public int Flash2ndDelay = 10;
	public float Flash2ndIntensity = 4.0f;

	private Color OffColor;
	private float OffIntensity = 0;
	private int count;
	private Light lite;

	// Use this for initialization
	void Start () {
		count = -1;
		lite = GetComponent<Light>();
		OffIntensity = lite.intensity;
		OffColor = lite.color;
	}
	
	// Update is called once per frame
	void Update () {
		if (++count >= FlashPeriod)
			count = 0;

		if (0 == count) {
			// primary strobe
			lite.intensity = FlashIntensity;
			lite.color = FlashColor;
		}

		if ((FlashDuration + Flash2ndDelay) == count) {
			// secondary strobe
			lite.intensity = Flash2ndIntensity;
			lite.color = FlashColor;
		}

		if ((FlashDuration == count) ||
			((FlashDuration + Flash2ndDelay + FlashDuration) == count)) {
			// strobe off
			lite.intensity = OffIntensity;
			lite.color = OffColor;
		}
	}
}
