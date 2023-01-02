using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIButtonStateEx2 : MonoBehaviour
{
	public List<GameObject> pressObj = new List<GameObject> ();

	public List<GameObject> releaseObj = new List<GameObject> ();

	public List<UITweener> tweens = new List<UITweener> ();

	public bool press = false;

	public bool Press {
		get {
			return press;
		}
		set {
			press = value;
			for (int i = 0; i < pressObj.Count; i++) {
				if (pressObj [i] != null) {
					pressObj [i].SetActive (press);
				}
			}
			for (int i = 0; i < releaseObj.Count; i++) {
				if (releaseObj [i] != null) {
					releaseObj [i].SetActive (!press);
				}
			}
			for (int i = 0; i < this.tweens.Count; i++) {
				if (press) {
					if (this.tweens [i] != null) {
						this.tweens [i].PlayForward ();
					}
				}
				else {
					if (this.tweens [i] != null) {
						this.tweens [i].PlayReverse ();
					}
				}
			}
		}
	}
}
