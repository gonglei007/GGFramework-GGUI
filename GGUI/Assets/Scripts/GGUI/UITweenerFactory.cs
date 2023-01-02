using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITweenerFactory
{
    public enum UIAnimationType {
        None,
        Pop,
        SlideUp,
    };

    private Dictionary<string, UITweener[]> uiAnmationTables = new Dictionary<string, UITweener[]>();

    public void Init(GameObject root)
    {
        if (root == null) {
            return;
        }
        for (int i = 0; i < root.transform.childCount; ++i) {
            GameObject animationObject = root.transform.GetChild(i).gameObject;
            UITweener[] uiTweeners = animationObject.GetComponentsInChildren<UITweener>(true);
            if (uiTweeners.Length > 0) {
                uiAnmationTables[animationObject.name] = uiTweeners;
            }
        }
    }

    public UITweener[] GetUITweeners(string name) {
        if (!uiAnmationTables.ContainsKey(name)) {
            return null;
        }
        return uiAnmationTables[name];
    }
}
