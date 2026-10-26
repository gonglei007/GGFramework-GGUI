using UnityEngine;

namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// 基于 NGUI UILabel 的文本控件实现。
    /// </summary>
    public class NGUIText : IUIText
    {
        private readonly UILabel label;

        public NGUIText(UILabel label)
        {
            this.label = label;
        }

        public string text
        {
            get { return label != null ? label.text : null; }
            set { if (label != null) { label.text = value; } }
        }

        public GameObject gameObject
        {
            get { return label != null ? label.gameObject : null; }
        }
    }
}
