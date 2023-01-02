using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIButtonStateEx : MonoBehaviour
{
	public List<GameObject> pressObj = new List<GameObject> ();

	public List<GameObject> releaseObj = new List<GameObject> ();

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
		}
	}

    [ContextMenu("SwitchButtonState")]
    public void SwitchButtonState()
    {
        Press = !Press;
    }
/*
#if UNITY_EDITOR
    [ContextMenu("Switch Button State")]
    public void SwitchButtonState()
    {
        GameObject firstObj = null;
        bool isFirstObjBelongToPressObj = true;

//        for (int i = 0; i < pressObj.Count; i++)
//        {
//            if (pressObj[i] != null)
//            {
//                firstObj = pressObj[i];
//            }
//        }

//        if (firstObj == null)
//        {
//            isFirstObjBelongToPressObj = false;
//            for (int i = 0; i < releaseObj.Count; i++)
//            {
//                if (releaseObj[i] != null)
//                {
//                    firstObj = releaseObj[i];
//                }
//            }
//        }

        if (firstObj != null)
        {
            this.Press = isFirstObjBelongToPressObj? (!firstObj.activeSelf) : (firstObj.activeSelf) ;
        }
    }
#endif
*/}
