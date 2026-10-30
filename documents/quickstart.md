# GGUI 快速接入指南（Quick Start）

> GGUI 是一个游戏客户端 UI 框架，命名空间 `GGFramework.GGUI`；控件能力抽象为 UI 接口层（`Abstraction/`），**默认由 NGUI 实现**（`Implementation/NGUI/`），另含 UGUI 占位实现（`Implementation/UGUI/`）。
> 本文档带领你从零开始，把 GGUI 接入自己的 Unity 工程，并跑起第一个页面 / 窗口 / 对话框。
> 引擎版本：Unity 6000.3.13f1。

---

## 0. 目录

- [1. 拷贝代码到工程](#1-拷贝代码到工程)
- [2. 解决依赖](#2-解决依赖)
- [3. 搭建场景与 UIRoot](#3-搭建场景与-uiroot)
- [4. 初始化 UISystem](#4-初始化-uisystem)
- [5. 最小全流程示例（页面 / 窗口 / 对话框）](#5-最小全流程示例页面--窗口--对话框)
- [6. 常见问题](#6-常见问题)

---

## 1. 拷贝代码到工程

框架代码位于仓库的 `GGUI/Assets/Scripts/GGUI/` 目录，整体拷贝到你的 Unity 工程并保持目录结构即可：

```
你的工程/Assets/Scripts/
    └── GGUI/                      ← 从本仓库整体拷贝
        ├── Systems/UISystem.cs
        ├── UIBase.cs / UIPage.cs / UIWindow.cs
        ├── UIDialog.cs / UILoading.cs / UIWaiting.cs / UIComponent.cs
        ├── UITweenerFactory.cs
        ├── Adaptors/TextAdaptor.cs
        ├── Utils/ComponentUtil.cs / UILabelLocalization.cs / UIAnimationClipEvent.cs
        ├── NGUIExtend/            ← 约 20 个 NGUI 扩展控件（可选）
        ├── Components/ · FX/ · Bridge/
        ├── Abstraction/           ← UI 接口层（IUIImplementation / IUIButton / IUIText / IUITweener / IUIProgressBar / IUILayoutGroup）
        ├── Implementation/NGUI/   ← 默认底层实现（NGUI）
        └── Implementation/UGUI/   ← UGUI 占位实现（骨架，待补全）
```

> GGUI 框架自身（`GGUI/Assets/Scripts/GGUI/`）依赖一套 `Dependency` 基础库
> （`ResourceSystem`、`ResourceItem`、`ResourceObject`、`GameDebugger` 等，位于仓库 `GGUI/Assets/Scripts/Dependency/`），请一并拷入工程；它们与实际业务代码（`Demo.cs` 等）位于工程根命名空间，**不带** `GGFramework.*` 前缀。
> 而 `Singleton` / `CoroutineUtil` 已收敛到 **GGFramework.Core** 单一源（工程内本地副本已删除），需在 `Packages/manifest.json` 添加：`"com.gonglei.ggframework.core": "https://github.com/gonglei007/GGFramework-Core.git#v0.1.1"`（`Dependency.asmdef` 已引用 `GGFramework.Core.Runtime`）。

---

## 2. 解决依赖

GGUI 依赖以下第三方库，需一并放入工程（本仓库放置于 `Assets/LocalPackages`）：

| 依赖 | 用途 |
| --- | --- |
| **GGFramework.Core** | `Singleton` / `CoroutineUtil` 基础类单一源（UPM git URL 引用 `#v0.1.1`） |
| **NGUI** | 默认 UI 底层实现依赖的控件系统（`UILabel` / `UISprite` / `UIButton` / `UITweener` / `UIPlayTween` / `UIPanel`…）；经 `Abstraction/` 接口层解耦 |
| **Loxodon Framework** | AssetBundle 资源异步加载底层 |
| **SimpleJson** | 轻量 JSON 序列化辅助 |

> 命名空间约定：GGUI 框架代码使用 `GGFramework.GGUI`；`Dependency` / `NGUIExtend` /
> `UIAnimationClipEvent` / `CountDown` 等属于应用层辅助代码，使用全局命名空间。

---

## 3. 搭建场景与 UIRoot

UISystem 在初始化时会到场景中查找 **Tag 为 `UIRoot`** 的游戏对象，找不到会直接抛异常：

```csharp
// 源码：UISystem.GetCreateGroup()
GameObject uiRoot = GameObject.FindWithTag("UIRoot");
if (uiRoot == null)
    throw new Exception("没有找到Tag为UIRoot的对象！");
```

UIRoot 创建方式（在场景中新建一个空物体）：

| 步骤 | 操作 |
| --- | --- |
| 1 | `GameObject → Create Empty`，命名为 `UIRoot` |
| 2 | 在 Inspector 顶部把 Tag 设置为 `UIRoot`（若标签不存在需先添加） |
| 3 | 推荐勾选 `DontDestroyOnLoad` 归属，使 UIRoot 跨场景常驻 |
| 4 | （可选）挂 `UIPanel` / `UIRoot`（NGUI 的根缩放脚本）适配不同分辨率 |

> `UIRoot` 建好后不需要手动创建各 UI 层分组。UISystem 初始化时会在 UIRoot 下按
> `UILayer` 枚举自动创建 `Root / Page / Window / UIFX / Loading / Dialog / Message / Waiting` 8 个分组
> GameObjects，并各自挂载 `UIPanel`、按预设深度排序。

---

## 4. 初始化 UISystem

在场景里放一个管理初始化的 GameObject 和脚本（例如 `Demo.cs`），在 `Start()` 中：

```csharp
using UnityEngine;
using GGFramework.GGUI;

public class Demo : MonoBehaviour
{
    void Start()
    {
        // 1) 初始化资源系统（框架 Bundle 名）
        ResourceSystem.Instance.Init("Game");

        // 2) （可选）注册本地化文本回调；不注册则 GetText 原样返回 key
        UISystem.Instance.TextAdaptor.onGetText = (string text) => text;

        // 3) 加载框架通用 UI（UIDialog / UILoading / UIWaiting 所在的 uiframework 资源）
        // 先 Reset 资源清单、添加资源项并异步加载
        UISystem.Instance.LoadFrameworkUIs((Exception e) =>
        {
            if (e == null)
            {
                // 4) 资源就绪后创建框架通用 UI
                UISystem.Instance.CreateFrameworkUIs();
            }
        });
    }
}
```

> 说明：
> - `UISystem` 与 `ResourceSystem` 都是 `Singleton<T>` 单例（继承 `MonoBehaviour`、`DontDestroyOnLoad`），直接访问 `.Instance` 即可。
> - `ResourceSystem.Instance.Init("...")` 必须在首次访问资源前调用一次，用于构建 Loxodon 资源管线。
> - 使用对话框 / 加载页 / 等待 UI 之前，务必先调用 `CreateFrameworkUIs()` 创建出 `UIDialog`、`UILoading`、`UIWaiting` 实例。

---

## 5. 最小全流程示例（页面 / 窗口 / 对话框）

下面演示三种最常见的 UI 形态。**页面 / 窗口由 Prefab + 脚本组成**，资源名与脚本上的 `Name` 由 UISystem 统一写入。

### 5.1 页面（UIPage，全屏，栈式管理）

先准备一个 Prefab，根节点挂 `UIPage` 脚本，并命名为你希望资源索引的名字（比如 `SamplePage`）。

```csharp
// 展示并压栈一个全屏页面
UISystem.Instance.CreatePage("SamplePage", (UIPage page, bool alreadyExist) =>
{
    UISystem.Instance.PresentPage("SamplePage");   // 入栈显示，自动隐藏当前栈顶页面
});

// 返回上一页 / 弹出到指定页 / 清空栈
UISystem.Instance.DismissPage();                    // 弹出栈顶，显示新栈顶
UISystem.Instance.DimissToPage("MainPage");         // 弹到"MainPage"，MainPage 自身不弹出
UISystem.Instance.DismissAllPage();                 // 清空页面栈
```

### 5.2 窗口（UIWindow，弹出窗，可多实例 / 全局）

```csharp
// 异步加载并创建窗口
UISystem.Instance.CreateWindow("SampleWindow", (UIWindow window, bool alreadyExist) =>
{
    // 显示窗口：false=附属当前 Page（切页自动回收）；true=全局窗口（不随页关闭）
    UISystem.Instance.ShowWindow("SampleWindow", false);
});

// 隐藏 / 全部隐藏
UISystem.Instance.HideWindow("SampleWindow");
UISystem.Instance.HideAllWindows();
```

### 5.3 模态对话框（UIDialog）

在 `CreateFrameworkUIs()` 之后可直接弹出：

```csharp
UISystem.Instance.PopDialog(
    "提示",                      // 标题
    "确定要删除吗？",            // 内容
    (int)UIDialog.Option.Yes | (int)UIDialog.Option.No,   // 位掩码配置按钮
    (UIDialog.Option option) =>
    {
        Debug.Log("用户选择: " + option);   // Option.Yes / No / Cancel
    });
```

> 遮罩（`Mask`）点击会被视为 **`Option.No`**；回调触发后对话框自动 `Hide()`。

---

## 6. 常见问题

| 现象 | 原因 / 解决办法 |
| --- | --- |
| 抛 `没有找到Tag为UIRoot的对象！` | 场景里没有 Tag=`UIRoot` 的对象，见 [§3](#3-搭建场景与-uiroot) |
| 对话框中 `ButtonYes` / `Label` 等控件没有被自动绑定 | `UIDialog` 依赖 `ComponentUtil.BindComponent` 按**固定路径**查子物体，请保持命名：`Content/ContainerTitle/Label`、`Content/ContainerTitle/ButtonCancel`、`Content/ContainerContent/Label`、`Content/ContainerButtons/ButtonYes`、`Content/ContainerButtons/ButtonNo`、`Mask` |
| 页面 / 窗口一直创建不起来 | 对应 Prefab 尚未加载进 `ResourceSystem`，或未通过 `ResourceSystem.AddResourceItem` / `ListenObject` 纳入加载队列；页面通常由 `PresentPage` 在资源就绪后自动补建 |
| 对话框 / 加载页弹不出 | 漏掉了 `CreateFrameworkUIs()` |
| 文本没有本地化 | 未给 `UISystem.Instance.TextAdaptor.onGetText` 赋值回调 |

---

> 更深入的分层、页面栈、生命周期、动画、本地化、异步资源联动等内容，见 **[使用手册](manual.md)**。
