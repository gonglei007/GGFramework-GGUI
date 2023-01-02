using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// UI Controller script. place on root.
/// </summary>
public class InputController : MonoBehaviour
{
	public readonly List<InputChild> Children = new List<InputChild> ();

	public bool DisableOnPause = true;

	public bool SelectOnAwake = true;

	public void Start ()
	{
	}

	public void OnEnable ()
	{
		Invoke ("SelectDefault", .25f);
	}

	public void Add (InputChild child)
	{
		Children.Add (child);
	}

	public void Remove (InputChild child)
	{
		Children.Remove (child);
	}

	public void GoInvoke (InputChild child)
	{
		child.gameObject.BroadcastMessage ("OnPress", true);
	}

	public void GoNext (InputChild child)
	{
		var ordered = Children.OrderBy (o => o.TabIndex);
		var next = ordered.FirstOrDefault (o => o.TabIndex > child.TabIndex) ?? ordered.FirstOrDefault ();
		UICamera.selectedObject = next == null ? null : next.gameObject;
	}

	public void GoBack (InputChild child)
	{
		var ordered = Children.OrderBy (o => o.TabIndex);
		var next = ordered.LastOrDefault (o => o.TabIndex < child.TabIndex) ?? ordered.LastOrDefault ();
		UICamera.selectedObject = next == null ? null : next.gameObject;
	}

	public void GoLeft (InputChild child)
	{
		var ordered = Children.Where (o => o.Row == child.Row).OrderBy (o => o.Col);
		var next = ordered.LastOrDefault (o => o.Col < child.Col) ?? ordered.LastOrDefault ();
		UICamera.selectedObject = next == null ? null : next.gameObject;
	}

	public void GoRight (InputChild child)
	{
		var ordered = Children.Where (o => o.Row == child.Row).OrderBy (o => o.Col);
		var next = ordered.FirstOrDefault (o => o.Col > child.Col) ?? ordered.FirstOrDefault ();
		UICamera.selectedObject = next == null ? null : next.gameObject;
	}

	public void GoUp (InputChild child)
	{
		var ordered = Children.Where (o => o.Col == child.Col).OrderBy (o => o.Row);
		var next = ordered.LastOrDefault (o => o.Row < child.Row) ?? ordered.LastOrDefault ();
		UICamera.selectedObject = next == null ? null : next.gameObject;
	}

	public void GoDown (InputChild child)
	{
		var ordered = Children.Where (o => o.Col == child.Col).OrderBy (o => o.Row);
		var next = ordered.FirstOrDefault (o => o.Row > child.Row) ?? ordered.FirstOrDefault ();
		UICamera.selectedObject = next == null ? null : next.gameObject;
	}

	public void Update ()
	{
		if (UICamera.selectedObject == null || !NGUITools.GetActive (UICamera.selectedObject)) {
			if (Input.GetKeyDown (KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow) || Input.GetKeyDown (KeyCode.UpArrow) || Input.GetKeyDown (KeyCode.DownArrow) || Input.GetKeyDown (KeyCode.Tab)) {
				UICamera.selectedObject = Children.OrderBy (o => o.TabIndex).First ().gameObject;
			}
		}
	}

	public void SelectDefault ()
	{
		if (SelectOnAwake) {
			if (UICamera.selectedObject == null || !NGUITools.GetActive (UICamera.selectedObject)) {
				var inp = Children.OrderBy (o => o.TabIndex).FirstOrDefault ();
				if (inp != null) {
					UICamera.selectedObject = inp.gameObject;
				}
			}
		}
	}
}
