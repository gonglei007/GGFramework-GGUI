//----------------------------------------------
//   NGUI: Next-Gen UI kit Addon
// Copyright © 2011-2015 Ren Yunhao
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Tween the value between from and to, trigger update to notify other to do update
/// </summary>
public class TweenValue : UITweener
{
	[Range (0f, 1f)]
	public float from = 1f;

	[Range (0f, 1f)]
	public float to = 1f;

	/// <summary>
	/// Event delegates called when the animation finishes.
	/// </summary>
	[HideInInspector]
	public List<EventDelegate> onUpdate = new List<EventDelegate> ();

	public float currentValue;

	public float value {
		get {
			return currentValue;
		}
		set {
			currentValue = value;
		}
	}

	/// <summary>
	/// Tween the value.
	/// </summary>
	protected override void OnUpdate(float factor, bool isFinished)
	{
		value = from + (to - from) * factor;
		current = this;
		if (onUpdate != null) {
			// Notify the listener delegates
			EventDelegate.Execute (onUpdate);
		}
		current = null;
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>
	static public TweenValue Begin (GameObject go, float duration, float to)
	{
		TweenValue comp = UITweener.Begin<TweenValue> (go, duration);
		comp.from = comp.value;
		comp.to = to;
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

	/// <summary>
	/// Convenience function -- add a new OnUpdate event delegate (here for to be consistent with RemoveOnUpdate).
	/// </summary>
	public void AddOnUpdate (EventDelegate del)
	{
		EventDelegate.Add (onUpdate, del);
	}

	/// <summary>
	/// Convenience function -- add a new OnUpdate event delegate (here for to be consistent with RemoveOnUpdate).
	/// </summary>
	public void AddOnUpdate (EventDelegate.Callback del)
	{
		EventDelegate.Add (onUpdate, del);
	}

	/// <summary>
	/// Remove an OnUpdate delegate. Will work even while iterating through the list when the tweener has finished its operation.
	/// </summary>
	public void RemoveOnUpdate (EventDelegate del)
	{
		if (onUpdate != null)
			onUpdate.Remove (del);
	}
}
