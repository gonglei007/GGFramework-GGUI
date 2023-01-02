using UnityEngine;

/// <summary>
/// UI Controller script
/// </summary>
public class InputChild : MonoBehaviour
{
	/// <summary>
	/// determines l/r selection order
	/// </summary>
	public int Col;

	/// <summary>
	/// determines up/down selection order
	/// </summary>
	public int Row;

	/// <summary>
	/// Determines Tab order
	/// </summary>
	public int TabIndex = 0;

	public bool enableArraowKey = true;

	public InputController controller;

	public void Awake ()
	{
		controller = FindObjectOfType<InputController> ();
	}

	public void OnEnable ()
	{
		//   if(controller)
		controller.Add (this);
	}

	public void OnDisable ()
	{
		// if (controller)
		controller.Remove (this);
		if (UICamera.selectedObject == gameObject) {
			UICamera.selectedObject = null;
		}
	}

	public void OnHover (bool isHover)
	{
		if (enabled && isHover) {
			UICamera.selectedObject = gameObject;
		}
		else if (UICamera.selectedObject == gameObject) {
			UICamera.selectedObject = null;
		}
	}

	public void OnKey (KeyCode key)
	{
		if (enabled && NGUITools.GetActive (gameObject)) {
			switch (key) {
			case KeyCode.LeftArrow:
				if (enableArraowKey)
					controller.GoLeft (this);
				break;
			case KeyCode.RightArrow:
				if (enableArraowKey)
					controller.GoRight (this);
				break;
			case KeyCode.UpArrow:
				if (enableArraowKey)
					controller.GoUp (this);
				break;
			case KeyCode.DownArrow:
				if (enableArraowKey)
					controller.GoDown (this);
				break;
			case KeyCode.Space:
			case KeyCode.Return:
				controller.GoInvoke (this);
				break;
			case KeyCode.Tab:
				if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) {
					controller.GoBack (this);
				}
				else {
					controller.GoNext (this);
				}
				break;
			}
		}
	}
}
