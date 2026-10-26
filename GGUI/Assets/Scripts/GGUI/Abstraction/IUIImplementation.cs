using System;
using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>
    /// UI 实现层总入口。核心 UI 系统通过此接口调用具体引擎（NGUI / UGUI）的能力，
    /// 从而与底层 UI 引擎解耦，并支持运行期切换实现。
    /// </summary>
    public interface IUIImplementation
    {
        /// <summary>
        /// 实现名称，例如 "NGUI" / "UGUI"。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 初始化实现层。
        /// </summary>
        /// <param name="uiRoot">UI 根对象。</param>
        /// <param name="animationRoot">补间动画模板根对象（可为空）。</param>
        void Init(GameObject uiRoot, GameObject animationRoot);

        /// <summary>
        /// 创建一个层级分组对象（替代 UIPanel 相关逻辑）。
        /// </summary>
        /// <param name="layerName">层名。</param>
        /// <param name="depth">深度 / 渲染排序值。</param>
        /// <param name="parent">父级 Transform。</param>
        GameObject CreateLayerGroup(string layerName, int depth, Transform parent);

        /// <summary>
        /// 为目标对象创建补间控制器（替代 UIPlayTween + CopyComponent 逻辑）。
        /// </summary>
        /// <param name="target">补间目标对象。</param>
        /// <param name="animName">补间模板名（可为空）。</param>
        IUITweener CreateTweener(GameObject target, string animName);

        /// <summary>按路径查找文本控件。</summary>
        IUIText FindText(GameObject root, string path);

        /// <summary>按路径查找按钮控件。</summary>
        IUIButton FindButton(GameObject root, string path);

        /// <summary>按路径查找进度条控件。</summary>
        IUIProgressBar FindProgressBar(GameObject root, string path);

        /// <summary>按路径查找布局组控件。</summary>
        IUILayoutGroup FindLayoutGroup(GameObject root, string path);

        /// <summary>按路径查找子 GameObject。</summary>
        GameObject FindGameObject(GameObject root, string path);

        /// <summary>为任意 GameObject 注册点击回调（用于遮罩等非按钮对象）。</summary>
        void AddClickHandler(GameObject target, Action<GameObject> handler);

        /// <summary>移除任意 GameObject 上已注册的点击回调。</summary>
        void RemoveClickHandler(GameObject target, Action<GameObject> handler);

        /// <summary>按 GameObject 引用包装文本控件。</summary>
        IUIText WrapText(GameObject go);

        /// <summary>按 GameObject 引用包装进度条控件。</summary>
        IUIProgressBar WrapProgressBar(GameObject go);
    }
}
