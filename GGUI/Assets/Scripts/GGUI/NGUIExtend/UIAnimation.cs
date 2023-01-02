//----------------------------------------------
//   NGUI: Next-Gen UI kit Addon
// Copyright © 2011-2015 Ren Yunhao
//----------------------------------------------
using UnityEngine;
using System;

/// <summary>
/// Help class to quickly set window animation
/// </summary>
public class UIAnimation : MonoBehaviour
{
	public enum ShowAnimationType
	{
		None,
		FadeIn,
		ScaleIn,
		FlyIn
	}

	public enum CloseAnimationType
	{
		None,
		FadeOut,
		ScaleOut,
		FlyOut
	}

	public enum FlyDirection
	{
		Left,
		Right,
		Top,
		Bottom
	}

	public ShowAnimationType showAnimationType = ShowAnimationType.None;

	public CloseAnimationType closeAnimationType = CloseAnimationType.None;

	public FlyDirection flyInDirection = FlyDirection.Left;

	public FlyDirection flyOutDirection = FlyDirection.Right;

	public Vector3 startScale = new Vector3 (0, 0, 0);

	public UIClose closeScript;

	public Action Event_ShowBegin;

	public Action Event_CloseBegin;

	public Action Event_ShowEnd;

	public Action Event_CloseEnd;

	public Action<GameObject> cachedWindowCloseCallBack;

	public TweenScale tweenScale;

	public TweenPosition tweenPosition;

	public TweenAlpha tweenAlpha;

	public float animationTime = 0.3f;

	public Vector3 cachedPosition;

	public bool positionCached = false;

	public bool fadeOutWorking = false;

	public bool scaleOutWorking = false;

	public bool flyOutWorking = false;

	public bool FadeOutWorking {
		get {
			return fadeOutWorking;
		}
		set {
			fadeOutWorking = value;
		}
	}

	public bool ScaleOutWorking {
		get {
			return scaleOutWorking;
		}
		set {
			scaleOutWorking = value;
		}
	}

	public bool FlyOutWorking {
		get {
			return flyOutWorking;
		}
		set {
			flyOutWorking = value;
		}
	}

	public void OnEnable ()
	{
		if (Event_ShowBegin != null) {
			Event_ShowBegin ();
		}
		switch (showAnimationType) {
		case ShowAnimationType.FadeIn:
			CheckTweenAlpha ();
			SetAlphaRecursively (0f);
			TweenAlpha.Begin (gameObject, animationTime, 1);
			tweenAlpha.eventReceiver = this.gameObject;
			tweenAlpha.callWhenFinished = "FadeInEnd";
			tweenAlpha.method = UITweener.Method.EaseInOut;
			break;
		case ShowAnimationType.FlyIn:
			CheckTweenPosition ();
			Vector3 startPosition = gameObject.transform.localPosition;
			Vector3 endPosition = positionCached ? cachedPosition : gameObject.transform.localPosition;
			switch (flyInDirection) {
			case FlyDirection.Left:
				startPosition.x = -Screen.width;
				break;
			case FlyDirection.Right:
				startPosition.x = Screen.width;
				break;
			case FlyDirection.Top:
				startPosition.y = Screen.height;
				break;
			case FlyDirection.Bottom:
				startPosition.y = -Screen.height;
				break;
			}
			gameObject.transform.localPosition = startPosition;
			TweenPosition.Begin (gameObject, animationTime, endPosition);
			tweenPosition.method = UITweener.Method.EaseInOut;
			tweenPosition.eventReceiver = this.gameObject;
			tweenPosition.callWhenFinished = "FlyInEnd";
			break;
		case ShowAnimationType.ScaleIn:
			CheckTweenScale ();
			Vector3 endScale = new Vector3 (1, 1, 1);
			gameObject.transform.localScale = startScale;
			TweenScale.Begin (gameObject, animationTime, endScale);
			tweenScale.method = UITweener.Method.EaseOut;
			tweenScale.eventReceiver = this.gameObject;
			tweenScale.callWhenFinished = "ScaleInEnd";
			break;
		default:
			break;
		}
		if (closeAnimationType != CloseAnimationType.None) {
			closeScript = gameObject.GetComponent<UIClose> ();
			if (closeScript != null) {
				closeScript.willStartCallBack += HanleWindowWillClose;
				cachedWindowCloseCallBack = closeScript.endCallBack;
			}
		}
	}

	public void OnDisable ()
	{
		switch (showAnimationType) {
		case ShowAnimationType.FlyIn:
			gameObject.transform.localPosition = Vector3.zero;
			break;
		default:
			break;
		}
	}

	public void OnDestroy ()
	{
		if (closeAnimationType != CloseAnimationType.None) {
			closeScript = gameObject.GetComponent<UIClose> ();
			if (closeScript != null) {
				closeScript.willStartCallBack += HanleWindowWillClose;
			}
		}
	}

	public void Update ()
	{
		if (closeScript != null && closeScript.endCallBack != null) {
			cachedWindowCloseCallBack = closeScript.endCallBack;
		}
	}

	public void ShowCloseAnimation ()
	{
		if (Event_CloseBegin != null) {
			Event_CloseBegin ();
		}
		switch (closeAnimationType) {
		case CloseAnimationType.FadeOut:
			MarkFadeOutWork (true);
			CheckTweenAlpha ();
			TweenAlpha.Begin (gameObject, animationTime, 0);
			tweenAlpha.eventReceiver = this.gameObject;
			tweenAlpha.callWhenFinished = "FadeOutEnd";
			tweenAlpha.method = UITweener.Method.EaseInOut;
			break;
		case CloseAnimationType.FlyOut:
			MarkFlyOutWork (true);
			CheckTweenPosition ();
			Vector3 endPosition = gameObject.transform.localPosition;
			cachedPosition = gameObject.transform.localPosition;
			positionCached = true;
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds (gameObject.transform);
			switch (flyOutDirection) {
			case FlyDirection.Left:
				endPosition.x = -Screen.width / 2 - bounds.size.x;
				break;
			case FlyDirection.Right:
				endPosition.x = Screen.width / 2 + bounds.size.x;
				break;
			case FlyDirection.Top:
				endPosition.y = Screen.height / 2 + bounds.size.y;
				break;
			case FlyDirection.Bottom:
				endPosition.y = -Screen.height / 2 - bounds.size.y;
				break;
			}
			TweenPosition.Begin (gameObject, animationTime, endPosition);
			tweenPosition.method = UITweener.Method.EaseInOut;
			tweenPosition.eventReceiver = this.gameObject;
			tweenPosition.callWhenFinished = "FlyOutEnd";
			break;
		case CloseAnimationType.ScaleOut:
			MarkScaleOutWork (true);
			CheckTweenScale ();
			TweenScale.Begin (gameObject, animationTime, startScale);
			tweenScale.method = UITweener.Method.EaseInOut;
			tweenScale.eventReceiver = this.gameObject;
			tweenScale.callWhenFinished = "ScaleOutEnd";
			break;
		default:
			break;
		}
	}

	public void CheckTweenAlpha ()
	{
		tweenAlpha = gameObject.GetComponent<TweenAlpha> ();
		if (tweenAlpha == null) {
			tweenAlpha = gameObject.AddComponent<TweenAlpha> ();
		}
	}

	public void CheckTweenPosition ()
	{
		tweenPosition = gameObject.GetComponent<TweenPosition> ();
		if (tweenPosition == null) {
			tweenPosition = gameObject.AddComponent<TweenPosition> ();
		}
	}

	public void CheckTweenScale ()
	{
		tweenScale = gameObject.GetComponent<TweenScale> ();
		if (tweenScale == null) {
			tweenScale = gameObject.AddComponent<TweenScale> ();
		}
	}

	public void FadeInEnd ()
	{
		ShowEndCallback ();
	}

	public void FlyInEnd ()
	{
		ShowEndCallback ();
	}

	public void ScaleInEnd ()
	{
		ShowEndCallback ();
	}

	public void FadeOutEnd ()
	{
		if (!fadeOutWorking) {
			return;
		}
		CloseEndCallback ();
		if (!(scaleOutWorking || flyOutWorking)) {
			NGUITools.SetActive (gameObject, false);
		}
		SetAlphaRecursively (1);
		MarkFadeOutWork (false);
	}

	public void FlyOutEnd (UITweener tweener)
	{
		if (!flyOutWorking) {
			return;
		}
		CloseEndCallback ();
		if (!(scaleOutWorking || fadeOutWorking)) {
			NGUITools.SetActive (gameObject, false);
		}
		MarkFlyOutWork (false);
	}

	public void ScaleOutEnd (UITweener tweener)
	{
		if (!scaleOutWorking) {
			return;
		}
		CloseEndCallback ();
		if (!(fadeOutWorking || fadeOutWorking)) {
			NGUITools.SetActive (gameObject, false);
		}
		MarkScaleOutWork (false);
	}

	public void ShowEndCallback ()
	{
		if (Event_ShowEnd != null) {
			Event_ShowEnd ();
		}
	}

	public void CloseEndCallback ()
	{
		if (Event_CloseEnd != null) {
			Event_CloseEnd ();
		}
		if (cachedWindowCloseCallBack != null) {
			cachedWindowCloseCallBack (gameObject);
		}
	}

	public bool HanleWindowWillClose (GameObject window)
	{
		ShowCloseAnimation ();
		return false;
	}

	public void SetAlphaRecursively (float alpha)
	{
		UIWidget[] widgets = gameObject.GetComponentsInChildren<UIWidget> ();
		foreach (UIWidget widget in widgets) {
			widget.alpha = alpha;
		}
	}

	public void MarkFadeOutWork (bool isWorking)
	{
		UIAnimation[] windowAnimations = gameObject.GetComponents<UIAnimation> ();
		foreach (UIAnimation windowAnimation in windowAnimations) {
			windowAnimation.FadeOutWorking = isWorking;
		}
	}

	public void MarkScaleOutWork (bool isWorking)
	{
		UIAnimation[] windowAnimations = gameObject.GetComponents<UIAnimation> ();
		foreach (UIAnimation windowAnimation in windowAnimations) {
			windowAnimation.ScaleOutWorking = isWorking;
		}
	}

	public void MarkFlyOutWork (bool isWorking)
	{
		UIAnimation[] windowAnimations = gameObject.GetComponents<UIAnimation> ();
		foreach (UIAnimation windowAnimation in windowAnimations) {
			windowAnimation.FlyOutWorking = isWorking;
		}
	}
}
