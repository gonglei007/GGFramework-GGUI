using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>
    /// 引擎无关的进度条控件抽象。
    /// NGUI 实现对应 UIProgressBar，UGUI 实现对应 UnityEngine.UI.Slider。
    /// </summary>
    public interface IUIProgressBar
    {
        /// <summary>
        /// 进度值，取值范围 0..1。
        /// </summary>
        float value { get; set; }

        /// <summary>
        /// 承载该控件的 GameObject。
        /// </summary>
        GameObject gameObject { get; }
    }
}
