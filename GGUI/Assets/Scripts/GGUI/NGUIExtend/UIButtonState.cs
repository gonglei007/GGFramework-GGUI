using UnityEngine;
using System.Collections;

public class UIButtonState : MonoBehaviour
{
	public UISprite pressSprite;

	public UISprite releaseSprite;

	public bool press = false;

	public bool Press {
		get {
			return press;
		}
		set {
			press = value;
			if (pressSprite != null) {
				pressSprite.gameObject.SetActive (press);
			}
			if (releaseSprite != null) {
				releaseSprite.gameObject.SetActive (!press);
			}
		}
	}
}
