using System;
using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>
    /// 引擎无关的按钮控件抽象。
    /// NGUI 实现对应 UIButton + UIEventListener.onClick，UGUI 实现对应 UnityEngine.UI.Button。
    /// </summary>
    public interface IUIButton
    {
        /// <summary>
        /// 承载该控件的 GameObject。
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// 设置按钮 GameObject 的激活状态。
        /// </summary>
        void SetActive(bool active);

        /// <summary>
        /// 注册点击回调，回调参数为触发点击的 GameObject。
        /// </summary>
        void AddClickListener(Action<GameObject> handler);

        /// <summary>
        /// 移除已注册的点击回调。
        /// </summary>
        void RemoveClickListener(Action<GameObject> handler);
    }
}
