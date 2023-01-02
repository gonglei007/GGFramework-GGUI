//----------------------------------------------
//   NGUI: Next-Gen UI kit Addon
// Copyright © 2011-2017 ygp
//----------------------------------------------
using UnityEngine;

/// <summary>
/// Tween the object and children's alpha. Works with both UI widgets as well as renderers.
/// </summary>
[AddComponentMenu ("NGUI/Addon/Tween/Tween Alpha Addon")]
public class TweenAlphaRespective : UITweener
{
	[Range (0f, 1f)]
	public float from = 1f;

	[Range (0f, 1f)]
	public float to = 1f;

	public bool mCached = false;

	public UIRect[] mRects;

	public Material[] mMats;

	public SpriteRenderer[] mSrs;

	public float[] mRectAlphas;

	public float[] mMatAlphas;

	public float[] mSrAlphas;

	public float mFactor2 = 1f;

	[System.Obsolete ("Use 'value' instead")]
	public float alpha {
		get {
			return this.value;
		}
		set {
			this.value = value;
		}
	}

	public void Cache ()
	{
		mCached = true;
		mRects = GetComponentsInChildren<UIRect> (true);
		if (mRects != null) {
			mRectAlphas = new float[mRects.Length];
			for (int i = 0; i < mRects.Length; i++) {
				mRectAlphas [i] = mRects [i].alpha;
			}
		}
		Renderer[] renderers = GetComponentsInChildren<Renderer> (true);
		mMats = new Material[renderers.Length];
		int index = 0;
		foreach (Renderer renderer in renderers) {
			mMats [index] = renderer.material;
			index++;
		}
		mMatAlphas = new float[mMats.Length];
		for (int i = 0; i < mMats.Length; i++) {
			mMatAlphas [i] = mMats [i].color.a;
		}
		mSrs = GetComponentsInChildren<SpriteRenderer> (true);
		if (mSrs != null) {
			mSrAlphas = new float[mSrs.Length];
			for (int i = 0; i < mSrs.Length; i++) {
				mSrAlphas [i] = mSrs [i].color.a;
			}
		}
	}

	/// <summary>
	/// Tween's current value.
	/// </summary>
	public float value {
		get {
			if (!mCached)
				Cache ();
			return mFactor2;
		}
		set {
			if (!mCached)
				Cache ();
			mFactor2 = value;
			if (mRects != null && mRects.Length > 0) {
				for (int i = 0; i < mRects.Length; i++) {
					mRects [i].alpha = mFactor2 * mRectAlphas [i];
				}
			}
			else if (mSrs != null && mSrs.Length > 0) {
				for (int i = 0; i < mSrs.Length; i++) {
					Color c = mSrs [i].color;
					c.a = mFactor2 * mSrAlphas [i];
					mSrs [i].color = c;
				}
			}
			else if (mMats != null && mMats.Length > 0) {
				for (int i = 0; i < mMats.Length; i++) {
					Color c = mMats [i].color;
					c.a = mFactor2 * mMatAlphas [i];
					mMats [i].color = c;
				}
			}
		}
	}

	/// <summary>
	/// Tween the value.
	/// </summary>
	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = Mathf.Lerp (from, to, factor);
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>
	static public TweenAlphaRespective Begin (GameObject go, float duration, float alpha)
	{
		TweenAlphaRespective comp = UITweener.Begin<TweenAlphaRespective> (go, duration);
		comp.from = comp.value;
		comp.to = alpha;
		if (duration <= 0f) {
			comp.Sample (1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	public override void SetStartToCurrentValue ()
	{
		from = value;
	}

	public override void SetEndToCurrentValue ()
	{
		to = value;
	}
}
