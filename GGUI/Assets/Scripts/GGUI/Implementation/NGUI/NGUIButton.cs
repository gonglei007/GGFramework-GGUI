using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// 基于 NGUI UIButton + UIEventListener 的按钮实现。
    /// </summary>
    public class NGUIButton : IUIButton
    {
        private readonly UIButton button;
        // 记录 Action 与包装后的 UIEventListener.VoidDelegate 的映射，保证可正确移除。
        private readonly Dictionary<Action<GameObject>, UIEventListener.VoidDelegate> handlers =
            new Dictionary<Action<GameObject>, UIEventListener.VoidDelegate>();

        public NGUIButton(UIButton button)
        {
            this.button = button;
        }

        public GameObject gameObject
        {
            get { return button != null ? button.gameObject : null; }
        }

        public void SetActive(bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        public void AddClickListener(Action<GameObject> handler)
        {
            if (button == null || handler == null)
            {
                return;
            }
            UIEventListener.VoidDelegate wrapped = new UIEventListener.VoidDelegate(handler);
            handlers[handler] = wrapped;
            UIEventListener.Get(button.gameObject).onClick += wrapped;
        }

        public void RemoveClickListener(Action<GameObject> handler)
        {
            if (button == null || handler == null)
            {
                return;
            }
            UIEventListener.VoidDelegate wrapped;
            if (handlers.TryGetValue(handler, out wrapped))
            {
                UIEventListener.Get(button.gameObject).onClick -= wrapped;
                handlers.Remove(handler);
            }
        }
    }
}
