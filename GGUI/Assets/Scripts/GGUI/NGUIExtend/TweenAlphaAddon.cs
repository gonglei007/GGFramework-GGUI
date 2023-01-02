//----------------------------------------------
//   NGUI: Next-Gen UI kit Addon
// Copyright © 2011-2015 Ren Yunhao
//----------------------------------------------
using UnityEngine;

/// <summary>
/// Tween the object and children's alpha. Works with both UI widgets as well as renderers.
/// </summary>
[AddComponentMenu ("NGUI/Addon/Tween/Tween Alpha Addon")]
public class TweenAlphaAddon : UITweener
{
	[Range (0f, 1f)]
	public float from = 1f;

	[Range (0f, 1f)]
	public float to = 1f;

	public bool mCached = false;

	public UIRect[] mRects;

	public Material[] mMats;

	public SpriteRenderer[] mSrs;

	public UISprite[] mSprites;

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
		Renderer[] renderers = GetComponentsInChildren<Renderer> (true);
		mMats = new Material[renderers.Length];
		int index = 0;
		foreach (Renderer renderer in renderers) {
			mMats [index] = renderer.material;
			index++;
		}
		mSrs = GetComponentsInChildren<SpriteRenderer> (true);
		mSprites = GetComponentsInChildren<UISprite> (true);
	}

	/// <summary>
	/// Tween's current value.
	/// </summary>
	public float value {
		get {
			if (!mCached)
				Cache ();
			if (mRects != null && mRects.Length > 0)
				return mRects [0].alpha;
			if (mSrs != null && mSrs.Length > 0)
				return mSrs [0].color.a;
			return mMats != null && mMats.Length > 0 ? mMats [0].color.a : 1f;
		}
		set {
			if (!mCached)
				Cache ();
			if (mRects != null && mRects.Length > 0) {
				foreach (UIRect rect in mRects) {
					rect.alpha = value;
				}
			}
			else if (mSrs != null && mSrs.Length > 0) {
				foreach (SpriteRenderer sr in mSrs) {
					Color c = sr.color;
					c.a = value;
					sr.color = c;
				}
			}
			else if (mMats != null && mMats.Length > 0) {
				foreach (Material mat in mMats) {
					Color c = mat.color;
					c.a = value;
					mat.color = c;
				}
			}
			else if (mSprites != null && mSprites.Length > 0) {
				foreach (UISprite item in mSprites) {
					item.alpha = value;
				}
			}
		}
	}

	/// <summary>
	/// Tween the value.
	/// </summary>
	protected override void OnUpdate (float factor, bool isFinished)
	{
		value = Mathf.Lerp (from, to, factor);
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>
	static public TweenAlphaAddon Begin (GameObject go, float duration, float alpha)
	{
		TweenAlphaAddon comp = UITweener.Begin<TweenAlphaAddon> (go, duration);
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

	[ContextMenu ("Set Alpha To 0")]
	public void SetAphlaToZero ()
	{
		if (!mCached)
			Cache ();
		if (mRects != null && mRects.Length > 0) {
			foreach (UIRect rect in mRects) {
				rect.alpha = 0;
			}
		}
		else if (mSrs != null && mSrs.Length > 0) {
			foreach (SpriteRenderer sr in mSrs) {
				Color c = sr.color;
				c.a = 0;
				sr.color = c;
			}
		}
		else if (mMats != null && mMats.Length > 0) {
			foreach (Material mat in mMats) {
				Color c = mat.color;
				c.a = 0;
				mat.color = c;
			}
		}
	}

	[ContextMenu ("Set Alpha To one")]
	public void SetAphlaToOne ()
	{
		if (!mCached)
			Cache ();
		if (mRects != null && mRects.Length > 0) {
			foreach (UIRect rect in mRects) {
				rect.alpha = 1;
			}
		}
		else if (mSrs != null && mSrs.Length > 0) {
			foreach (SpriteRenderer sr in mSrs) {
				Color c = sr.color;
				c.a = 1;
				sr.color = c;
			}
		}
		else if (mMats != null && mMats.Length > 0) {
			foreach (Material mat in mMats) {
				Color c = mat.color;
				c.a = 1;
				mat.color = c;
			}
		}
	}
}
