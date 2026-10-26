using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>
    /// 引擎无关的文本控件抽象。
    /// NGUI 实现对应 UILabel，UGUI 实现对应 UnityEngine.UI.Text。
    /// </summary>
    public interface IUIText
    {
        /// <summary>
        /// 文本内容。
        /// </summary>
        string text { get; set; }

        /// <summary>
        /// 承载该控件的 GameObject。
        /// </summary>
        GameObject gameObject { get; }
    }
}
