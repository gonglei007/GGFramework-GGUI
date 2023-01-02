//----------------------------------------------
//   NGUI: Next-Gen UI kit Addon
// Copyright © 2011-2015 Ren Yunhao
//----------------------------------------------
using UnityEngine;

/// <summary>
/// Help class to quickly set Tween Alpha animation
/// </summary>
public class UIWidgetFlash : MonoBehaviour
{
	public enum FlashType
	{
		Immediately,
		Tween
	}

	public enum EndAction
	{
		None,
		Replay,
		Destroy,
		Hide
	}

	public delegate void EndCallBack ();

	public EndCallBack endCallBack = null;

	public FlashType flashType = FlashType.Tween;

	public UITweener.Method easeType = UITweener.Method.Linear;

	public bool flashEnabled = true;

	public float flashInterval = 0.5f;

	public int flashTimes = 2;

	public float InitialAlpha = 1;

	public float maxAlpha = 1f;

	public bool useRealTime = false;

	public EndAction endAction = EndAction.Replay;

	public TweenValue tweenValue = null;

	public bool flashDisabled = true;

	public float flashTimer = 0;

	public int flashTimesCounter = 0;

	public float currentAlpha = 0;

	public void Start ()
	{
		if (InitialAlpha > maxAlpha) {
			InitialAlpha = maxAlpha;
		}
		Initialize ();
	}

	public void Update ()
	{
		if (flashEnabled) {
			flashDisabled = false;
			float timeDelta;
			if (useRealTime) {
				timeDelta = RealTime.deltaTime;
				//Pause Game too long will cause real time delta very large, Reduce it!
				while (timeDelta >= flashInterval) {
					timeDelta -= flashInterval;
				}
			}
			else {
				timeDelta = Time.deltaTime;
			}
			flashTimer += timeDelta;
			if (flashTimer >= flashInterval) {
				flashTimesCounter++;
				if (flashTimesCounter > flashTimes) {
					Initialize ();
					if (endCallBack != null) {
						endCallBack ();
					}
					switch (endAction) {
					case EndAction.None:
						flashEnabled = false;
						break;
					case EndAction.Replay:
						break;
					case EndAction.Destroy:
						Destroy (this.gameObject);
						break;
					case EndAction.Hide:
						NGUITools.SetActive (this.gameObject, false);
						break;
					default:
						break;
					}
				}
				else {
					if (flashType == FlashType.Immediately) {
						flashTimer = 0;
						currentAlpha = maxAlpha - currentAlpha;
						SetAlphaRecursively (this.gameObject, currentAlpha);
					}
					else {
						flashTimer = 0;
						StartTweenWidgetFlash ();
					}
				}
			}
		}
		else {
			if (flashDisabled == false) {
				Initialize ();
				flashDisabled = true;
			}
		}
	}

	public void Initialize ()
	{
		currentAlpha = InitialAlpha;
		flashTimer = flashInterval;
		flashTimesCounter = 0;
		SetAlphaRecursively (this.gameObject, currentAlpha);
	}

	public void StartTweenWidgetFlash ()
	{
		tweenValue = TweenValue.Begin (this.gameObject, flashInterval, maxAlpha - currentAlpha);
		tweenValue.from = currentAlpha;
		tweenValue.method = easeType;
		tweenValue.AddOnUpdate (UIWidgetFlashTweenUpdate);
		currentAlpha = maxAlpha - currentAlpha;
	}

	public void SetAlphaRecursively (GameObject gameObject, float alpha)
	{
		UIWidget[] widgets = gameObject.GetComponentsInChildren<UIWidget> ();
		foreach (UIWidget widget in widgets) {
			widget.alpha = alpha;
		}
	}

	public void UIWidgetFlashTweenUpdate ()
	{
		SetAlphaRecursively (this.gameObject, tweenValue.value);
	}
}
