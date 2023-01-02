using UnityEngine;
using System.Collections;

public class UIShake : MonoBehaviour
{
	public int shakeTimes = 3;

	public int shakeInterval = 1;

	public int shakeAngle = 5;

	public bool autoStart = false;

	public int shakeTimeCounter = 0;

	public float shakeIntervalTimer = 0;

	public bool startShake = false;

	public bool startShakeWait = false;

	public Quaternion mStartRotation;

	// Use this for initialization
	public void Start ()
	{
		mStartRotation = this.transform.localRotation;
		if (autoStart) {
			StartShake ();
		}
	}

	// Update is called once per frame
	public void Update ()
	{
		if (startShakeWait) {
			shakeIntervalTimer += Time.deltaTime;
			if (shakeIntervalTimer >= shakeInterval) {
				shakeIntervalTimer = 0;
				startShakeWait = false;
				DoStartShake ();
			}
		}
	}

	public void StartShake (bool startWait = false)
	{
		if (startShake != true) {
			startShake = true;
			shakeTimeCounter = 1;
			if (startWait) {
				startShakeWait = true;
				shakeIntervalTimer = 0;
			}
			else {
				DoStartShake ();
			}
		}
	}

	public void StopShake ()
	{
        startShake = false;
        if (startShake != false) {
			DestroyImmediate (this.GetComponent<TweenRotation> ());
			//this.transform.localRotation = Quaternion.Euler(Vector3.zero);
		}
		this.transform.localRotation = mStartRotation;
	}

	public void DoStartShake ()
	{
		if (startShake) {
			Quaternion rotation = Quaternion.Euler (new Vector3 (0, 0, shakeAngle));
			TweenRotation tweenRotation = TweenRotation.Begin (this.gameObject, 0.03f, rotation);
			tweenRotation.from = new Vector3 (0, 0, 0);
			tweenRotation.eventReceiver = this.gameObject;
			tweenRotation.callWhenFinished = "ShakeAnim1Finish";
		}
	}

	public void ShakeAnim1Finish ()
	{
		if (startShake) {
			Quaternion rotation = Quaternion.Euler (new Vector3 (0, 0, 0));
			TweenRotation tweenRotation = TweenRotation.Begin (this.gameObject, 0.03f, rotation);
			tweenRotation.eventReceiver = this.gameObject;
			tweenRotation.callWhenFinished = "ShakeAnim2Finish";
		}
	}

	public void ShakeAnim2Finish ()
	{
		if (startShake) {
			Quaternion rotation = Quaternion.Euler (new Vector3 (0, 0, 360 - shakeAngle));
			TweenRotation tweenRotation = TweenRotation.Begin (this.gameObject, 0.03f, rotation);
			tweenRotation.from = new Vector3 (0, 0, 360);
			tweenRotation.eventReceiver = this.gameObject;
			tweenRotation.callWhenFinished = "ShakeAnim3Finish";
		}
	}

	public void ShakeAnim3Finish ()
	{
		if (startShake) {
			Quaternion rotation = Quaternion.Euler (new Vector3 (0, 0, 360));
			TweenRotation tweenRotation = TweenRotation.Begin (this.gameObject, 0.03f, rotation);
			tweenRotation.from = new Vector3 (0, 0, 360 - shakeAngle);
			tweenRotation.to = new Vector3 (0, 0, 360);
			tweenRotation.eventReceiver = this.gameObject;
			tweenRotation.callWhenFinished = "ShakeAnim4Finish";
		}
	}

	public void ShakeAnim4Finish ()
	{
		if (startShake) {
			if (shakeTimeCounter < shakeTimes) {
				shakeTimeCounter++;
				DoStartShake ();
			}
			else {
				shakeTimeCounter = 1;
				startShakeWait = true;
				shakeIntervalTimer = 0;
			}
		}
	}
}
