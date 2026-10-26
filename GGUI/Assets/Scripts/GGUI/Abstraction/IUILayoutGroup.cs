using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>
    /// 引擎无关的布局组抽象。
    /// NGUI 实现对应 UIGrid，UGUI 实现对应 HorizontalLayoutGroup 等。
    /// </summary>
    public interface IUILayoutGroup
    {
        /// <summary>
        /// 单元格宽度。
        /// </summary>
        float cellWidth { get; set; }

        /// <summary>
        /// 触发重新排布。
        /// </summary>
        void Reposition();

        /// <summary>
        /// 承载该控件的 GameObject。
        /// </summary>
        GameObject gameObject { get; }
    }
}
