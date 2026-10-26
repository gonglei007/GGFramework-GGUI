using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGFramework.GGUI.UGUI
{
    /// <summary>
    /// 基于 UGUI Text 的文本控件实现。
    /// </summary>
    public class UGUIText : IUIText
    {
        private readonly Text textComponent;

        public UGUIText(Text textComponent)
        {
            this.textComponent = textComponent;
        }

        public string text
        {
            get { return textComponent != null ? textComponent.text : null; }
            set { if (textComponent != null) { textComponent.text = value; } }
        }

        public GameObject gameObject
        {
            get { return textComponent != null ? textComponent.gameObject : null; }
        }
    }

    /// <summary>
    /// 基于 UGUI Button 的按钮实现。点击回调统一转换为携带 GameObject 参数的形式，与 NGUI 行为对齐。
    /// </summary>
    public class UGUIButton : IUIButton
    {
        private readonly Button button;
        private readonly Dictionary<Action<GameObject>, UnityAction> handlers = new Dictionary<Action<GameObject>, UnityAction>();

        public UGUIButton(Button button)
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
            GameObject sender = button.gameObject;
            UnityAction wrapped = () => handler(sender);
            handlers[handler] = wrapped;
            button.onClick.AddListener(wrapped);
        }

        public void RemoveClickListener(Action<GameObject> handler)
        {
            if (button == null || handler == null)
            {
                return;
            }
            UnityAction wrapped;
            if (handlers.TryGetValue(handler, out wrapped))
            {
                button.onClick.RemoveListener(wrapped);
                handlers.Remove(handler);
            }
        }
    }

    /// <summary>
    /// 基于 UGUI Slider 的进度条实现。
    /// 注意：value 语义为 0..1，需保证 Slider 的 minValue=0、maxValue=1。
    /// </summary>
    public class UGUIProgressBar : IUIProgressBar
    {
        private readonly Slider slider;

        public UGUIProgressBar(Slider slider)
        {
            this.slider = slider;
        }

        public float value
        {
            get { return slider != null ? slider.value : 0f; }
            set { if (slider != null) { slider.value = value; } }
        }

        public GameObject gameObject
        {
            get { return slider != null ? slider.gameObject : null; }
        }
    }

    /// <summary>
    /// UGUI 布局组占位实现。UGUI 无与 NGUI UIGrid.cellWidth 直接对应的属性，暂以字段承载。
    /// </summary>
    public class UGUILayoutGroup : IUILayoutGroup
    {
        private readonly GameObject target;
        private float cellWidthValue;

        public UGUILayoutGroup(GameObject target)
        {
            this.target = target;
        }

        public float cellWidth
        {
            get { return cellWidthValue; }
            set { cellWidthValue = value; }
        }

        public void Reposition()
        {
            // TODO: 接入 HorizontalLayoutGroup / GridLayoutGroup 的强制重建布局。
            Debug.LogWarning("[UGUI] UGUILayoutGroup.Reposition 尚未实现。");
        }

        public GameObject gameObject
        {
            get { return target; }
        }
    }

    /// <summary>
    /// UGUI 补间占位实现。
    /// </summary>
    public class UGUITweener : IUITweener
    {
        public void Play(bool forward)
        {
            // TODO: 接入 Animator 或 DOTween 实现显示/隐藏补间。
            Debug.LogWarning("[UGUI] UGUITweener.Play 尚未实现。");
        }
    }
}
