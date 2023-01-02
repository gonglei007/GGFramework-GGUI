using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class UIEmojiManager : MonoBehaviour
{
	public static UIEmojiManager instance = null;

	public static bool hasDestroyed = false;

	//当有实例触发OnDestroy的时候变更为true，这个时候就不创建GameObject了
	public static UIAtlas mAtlas = null;

	//private List<UISprite> mUsedSpriteList = new List<UISprite>();
	public List<UISprite> mRecycledSpriteList = new List<UISprite> ();

	public static UIEmojiManager Instance {
		get {
			if (instance == null) {
				instance = GameObject.FindObjectOfType (typeof(UIEmojiManager)) as UIEmojiManager;
				if (instance == null && !hasDestroyed) {
					instance = new GameObject ("UIEmojiManager").AddComponent<UIEmojiManager> ();
				}
			}
			return instance;
		}
	}

	// Use this for initialization
	public void Start ()
	{
		this.gameObject.SetActive (false);
		if (!hasDestroyed) {
			if (mRecycledSpriteList.Count < 5) {
				for (int i = mRecycledSpriteList.Count - 1; i < 10; i++) {
					UISprite sp = NGUITools.AddWidget<UISprite> (this.gameObject);
					sp.name = "_emoji";
					mRecycledSpriteList.Add (sp);
				}
			}
		}
	}

	public void Init ()
	{
	}

	public void OnDestroy ()
	{
		hasDestroyed = true;
	}

	public static UISprite RequestSprite ()
	{
		return Instance.GetAvailableSprite ();
	}

	public UISprite GetAvailableSprite ()
	{
		UISprite sp = null;
		if (mRecycledSpriteList.Count > 0) {
			sp = mRecycledSpriteList [mRecycledSpriteList.Count - 1];
			mRecycledSpriteList.RemoveAt (mRecycledSpriteList.Count - 1);
		}
		else {
			if (!hasDestroyed) {
				sp = NGUITools.AddWidget<UISprite> (this.gameObject);
				sp.name = "_emoji";
			}
		}
		return sp;
	}

	public static void RecycleSprite (UISprite sp)
	{
		Instance.RecycledUnusedSprite (sp);
	}

	public void RecycledUnusedSprite (UISprite sp)
	{
		if (sp != null) {
			sp.gameObject.SetActive (false);
			sp.transform.parent = this.transform;
			mRecycledSpriteList.Add (sp);
		}
	}

	/// <summary>
	/// 在这里定制emojiCode对应的表情sprite
	/// Customize your own emoji code here
	/// </summary>
	public static void TranslateEmojiCode (int emojiCode, int emojiLevel, out string spriteName, out UIAtlas atlas)
	{
		if (emojiLevel >= 0) {
			#if UNITY_EDITOR
						            if (mAtlas == null && !hasDestroyed || !Application.isPlaying)
#else
			if (mAtlas == null && !hasDestroyed)
			#endif
			 {
				mAtlas = Resources.Load<UIAtlas> ("Atlas/Resource");
			}
			spriteName = string.Format ("resource_{0:#0000}", emojiCode);
			atlas = mAtlas;
		}
		else {
			#if UNITY_EDITOR
						            if (mAtlas == null && !hasDestroyed || !Application.isPlaying)
#else
			if (mAtlas == null && !hasDestroyed)
			#endif
			 {
				mAtlas = Resources.Load<UIAtlas> ("Atlas/Resource");
			}
			spriteName = string.Format ("resource_{0}", 1000);
			atlas = mAtlas;
		}
	}
}
