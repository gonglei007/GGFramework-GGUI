using UnityEngine;

namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// 基于 NGUI UIProgressBar 的进度条实现。
    /// </summary>
    public class NGUIProgressBar : IUIProgressBar
    {
        private readonly UIProgressBar bar;

        public NGUIProgressBar(UIProgressBar bar)
        {
            this.bar = bar;
        }

        public float value
        {
            get { return bar != null ? bar.value : 0f; }
            set { if (bar != null) { bar.value = value; } }
        }

        public GameObject gameObject
        {
            get { return bar != null ? bar.gameObject : null; }
        }
    }
}
