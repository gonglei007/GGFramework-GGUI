using System.Collections.Generic;
using UnityEngine;

namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// NGUI 补间模板工厂。由核心层的 UITweenerFactory 下沉而来，
    /// 负责缓存动画模板根对象下的 UITweener 组件，供 CreateTweener 复制使用。
    /// </summary>
    public class NGUITweenerFactory
    {
        private readonly Dictionary<string, UITweener[]> uiAnmationTables = new Dictionary<string, UITweener[]>();

        public void Init(GameObject root)
        {
            if (root == null)
            {
                return;
            }
            for (int i = 0; i < root.transform.childCount; ++i)
            {
                GameObject animationObject = root.transform.GetChild(i).gameObject;
                UITweener[] uiTweeners = animationObject.GetComponentsInChildren<UITweener>(true);
                if (uiTweeners.Length > 0)
                {
                    uiAnmationTables[animationObject.name] = uiTweeners;
                }
            }
        }

        public UITweener[] GetUITweeners(string name)
        {
            if (string.IsNullOrEmpty(name) || !uiAnmationTables.ContainsKey(name))
            {
                return null;
            }
            return uiAnmationTables[name];
        }
    }
}
