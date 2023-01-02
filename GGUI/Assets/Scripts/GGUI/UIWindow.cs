using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 跟Page类似，不同的地方是：
///     - Window不是用来管理UI流程的。在每个页面都可能弹出。
///     - 加载先考虑使用运行时动态加载。
///     - Window可以有N个实例。
/// 在切Page的时候，要关闭打开的Window
/// </summary>
public class UIWindow : UIBase
{

    public override void OnShowed(Action onFinished)
    {
        base.OnShowed(onFinished);
    }

    public override void OnPreHide(Action onFinished)
    {
        base.OnPreHide(onFinished);
    }
}
