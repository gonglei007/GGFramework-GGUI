using UnityEngine;
using System.Collections;

public class UIScale : MonoBehaviour
{
	public float scaleSpeed = 1f;

	public int scaleTimes = 2;

	public int scaleInterval = 3;

	public float scaleFrom = 1;

	public float scaleTo = 1.1f;

	public bool autoStart = false;

	public bool loop = true;

	public int scaleTimeCounter = 0;

	public float scaleIntervalTimer = 0;

	public bool startScale = false;

	public bool startScaleWait = false;

	public Vector3 originScale = Vector3.one;

	// Use this for initialization
	public void Start ()
	{
		originScale = this.transform.localScale;
		if (autoStart) {
			StartScale ();
		}
	}

	// Update is called once per frame
	public void Update ()
	{
		if (startScaleWait) {
			scaleIntervalTimer += Time.deltaTime;
			if (scaleIntervalTimer >= scaleInterval) {
				scaleIntervalTimer = 0;
				startScaleWait = false;
				DoStartScale ();
			}
		}
	}

	public void StartScale ()
	{
		startScale = true;
		scaleTimeCounter = 1;
		DoStartScale ();
	}

	public void StopScale ()
	{
		startScale = false;
		Destroy (this.GetComponent<TweenScale> ());
		this.transform.localScale = originScale;
	}

	public void DoStartScale ()
	{
		if (startScale) {
			TweenScale tweenScale = TweenScale.Begin (this.gameObject, scaleSpeed, originScale * scaleTo);
			tweenScale.from = originScale * scaleFrom;
			tweenScale.method = UITweener.Method.EaseIn;
			EventDelegate.Add (tweenScale.onFinished, ScaleOutAnimFinish, true);
		}
	}

	public void ScaleOutAnimFinish ()
	{
		TweenScale tweenScale = TweenScale.Begin (this.gameObject, scaleSpeed * 0.5f, originScale * scaleFrom);
		tweenScale.method = UITweener.Method.EaseIn;
		EventDelegate.Add (tweenScale.onFinished, ScaleAnimFinish, true);
	}

	public void ScaleAnimFinish ()
	{
		if (startScale) {
			if (scaleTimeCounter < scaleTimes) {
				scaleTimeCounter++;
				DoStartScale ();
			}
			else {
				scaleTimeCounter = 1;
				if (loop) {
					startScaleWait = true;
				}
				scaleIntervalTimer = 0;
			}
		}
	}
}
