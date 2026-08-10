# GGUI 使用手册

> GGUI 是一个**基于 NGUI** 的游戏客户端 UI 框架，命名空间 `GGFramework.GGUI`。
> 它提供分层 UI 管理、栈式页面导航、通用对话框/加载/等待组件、动画工厂与丰富的 NGUI 扩展控件，帮助团队快速搭建游戏界面层。
> 引擎版本：Unity 2021.3.4f1c1。

---

## 目录

- [1. 架构与分层](#1-架构与分层)
- [2. UISystem：核心总控](#2-uisystem核心总控)
- [3. UI 生命周期](#3-ui-生命周期)
- [4. 页面栈导航](#4-页面栈导航)
- [5. 窗口系统](#5-窗口系统)
- [6. 模态对话框](#6-模态对话框)
- [7. 加载页与等待 UI](#7-加载页与等待-ui)
- [8. 动画系统](#8-动画系统)
- [9. 文本本地化](#9-文本本地化)
- [10. NGUI 控件扩展库](#10-ngui-控件扩展库)
- [11. 与 ResourceSystem 异步联动](#11-与-resourcesystem-异步联动)
- [12. API 参考](#12-api-参考)

---

## 1. 架构与分层

### 1.1 整体结构

```
GGFramework.GGUI
├── UIBase.cs           所有 UI 的基类（生命周期 + 显隐流程）
├── UIPage.cs           全屏页面（栈式导航单元）
├── UIWindow.cs         弹出窗口（可多实例）
├── UIDialog.cs         模态对话框
├── UILoading.cs        加载页（进度条 + 提示 + 转场特效）
├── UIWaiting.cs        等待遮罩
├── UIComponent.cs      页面附属组件基类
├── UITweenerFactory.cs NGUI 补间动画工厂
├── Systems/UISystem.cs 核心总控（单例）
├── Adaptors/TextAdaptor.cs  文本本地化适配
├── Utils/ComponentUtil.cs  组件自动绑定 / 复制
├── Utils/UILabelLocalization.cs 本地化标签组件
└── NGUIExtend/         约 20 个 NGUI 扩展控件（动画 / 按钮 / 输入 / 灰化…）
```

### 1.2 八层 UI 分层

UISystem 内置 **8 个 UI 层**（`UISystem.UILayer` 枚举），每层有固定的深度（`UIPanel.depth` / `sortingOrder`），运行时在 `UIRoot` 下按层自动创建分组并挂载 `UIPanel`：

| 层 | 说明 |
| --- | --- |
| `Root` | 根节点 |
| `Page` | 全屏页面层 |
| `Window` | 窗口层 |
| `UIFX` | UI 特效层 |
| `Loading` | 加载层 |
| `Dialog` | 对话框层 |
| `Message` | 消息层 |
| `Waiting` | 等待遮罩层 |

> 上层 UI 基于 NGUI 的 `UIPanel` 排序控制，不是 UGUI 的 Canvas 层级。

---

## 2. UISystem：核心总控

`UISystem` 是继承 `Singleton<UISystem>` 的单例，负责所有 UI 的注册、创建与生命周期管理。

### 2.1 初始化

启动时在场景中放置一个 **Tag 为 `UIRoot`** 的 GameObject（详见快速接入指南），然后：

```csharp
UISystem.Instance.CreateFrameworkUIs();   // 创建框架固有 UI（对话框 / 加载 / 等待的容器）
```

### 2.2 UI 注册表

所有已创建的 UI 按 `(layer, name)` 登记到注册表：

```csharp
UISystem.Instance.SetUIBase(UISystem.UILayer.Page, "MainMenu", page);
UISystem.Instance.GetUIBase(UISystem.UILayer.Page, "MainMenu"); // 取回
UISystem.Instance.RemoveUIBase(UISystem.UILayer.Page, "MainMenu");
```

---

## 3. UI 生命周期

所有 UI 继承 `UIBase`，提供统一的生命周期回调。子类通过覆写 `On*` 方法在显隐前后插入动画与逻辑。

| 回调 | 时机 |
| --- | --- |
| `OnLoad()` | UI 创建（Prefab 实例化）完成时 |
| `OnPreShow(Action onFinished)` | 准备显示前（常用于播放入场动画，动画结束调用 `onFinished`） |
| `OnShowed(Action onFinished)` | 完全显示后 |
| `OnPreHide(Action onFinished)` | 准备隐藏前（常用于播放退场动画） |

```csharp
public class MyPage : UIPage
{
    public override void OnPreShow(Action onFinished)
    {
        // 播放入场动画，结束后回调，框架才会标记为"已显示"
        PlayTween("showAnim", onFinished);
    }

    public override void OnPreHide(Action onFinished)
    {
        PlayTween("hideAnim", onFinished);
    }
}
```

显隐入口由基类提供：

```csharp
ui.Show();                 // 显示（默认无特效名）
ui.Show(string uifx);      // 指定显示特效
ui.Hide();                 // 隐藏
ui.Hide(string hideuifx, Action onFinish = null); // 指定退场特效 + 结束回调
```

---

## 4. 页面栈导航

页面（`UIPage`）通过**栈式导航**管理，适合"全屏页面逐层进入 / 返回"的游戏流程。

### 4.1 创建并展示页面

```csharp
// 创建页面（异步，依据 ResourceSystem 加载 Prefab）
UISystem.Instance.CreatePage("MainMenu", (UIPage page, bool isReady) => {
    if (isReady) {
        // 创建成功
    }
});

// 入栈并显示（隐藏当前页）
UISystem.Instance.PresentPage("MainMenu", hideCurrentPage: true, onShow: null);

// 取当前页
UIPage current = UISystem.Instance.GetCurrentPage();
```

### 4.2 页面出栈与回退

```csharp
UISystem.Instance.DismissPage();          // 弹出当前页（默认 dismiss 自身）
UISystem.Instance.DismissPage("MainMenu");// 弹出到指定页（回退到目标页并移除其上的页面）
UISystem.Instance.DismissAllPage();       // 清空全部页面栈
bool inStack = UISystem.Instance.IsPageInStack("MainMenu"); // 是否在栈中
```

### 4.3 页面附属组件

页面可从同一 Bundle 加载附属 `UIComponent`：

```csharp
// 挂载组件到页面
UISystem.Instance.GetPage("MainMenu").SetComponent("Bag", new BagComponent());
// 按名取回
UIComponent c = UISystem.Instance.GetPage("MainMenu").GetComponent("Bag");
```

---

## 5. 窗口系统

窗口（`UIWindow`）是较轻量的弹出 UI，不管理流程，可同时存在多个实例。

```csharp
// 创建窗口
UISystem.Instance.CreateWindow("Shop", (UIWindow win, bool isReady) => {
    if (isReady) { win.Show(); }
});

// 显示窗口。global=true 为全局窗口：页面切换时不随页面隐藏；false 为附属窗口，随页面关闭
UISystem.Instance.ShowWindow("Shop", global: false);
UISystem.Instance.HideWindow("Shop");       // 隐藏指定窗口
UISystem.Instance.HideAllWindows();         // 隐藏所有窗口
UISystem.Instance.HideWindowsByPage(page);  // 隐藏某页面下的附属窗口
```

---

## 6. 模态对话框

对话框（`UIDialog`）基于 NGUI，通过**命名规定 + 自动绑定**（`ComponentUtil.BindComponent`）布局部件。

### 6.1 显示对话框

```csharp
UIDialog dlg = UISystem.Instance.CreateDialog("Confirm");
dlg.Show(
    title: "提示",
    message: "确定要删除该角色吗？",
    options: (int)(UIDialog.Option.Yes | UIDialog.Option.No),
    onCallback: (UIDialog.Option opt) => {
        if (opt == UIDialog.Option.Yes) {
            // 玩家点了"确定"
        } else {
            // 点了 No 或点击了遮罩
        }
    }
);
```

### 6.2 按钮位掩码

`UIDialog.Option` 是 `[Flags]` 位掩码，可组合按钮：

| 值 | 按钮 |
| --- | --- |
| `Option.Yes` | 确定 |
| `Option.No` | 取消 |
| `Option.Cancel` | 关闭 |

- 点击遮罩 = 触发 `No`
- 对话框自动按按钮数调整 `UIGrid` 布局

### 6.3 绑定规则

`UIDialog` 依赖 `ComponentUtil.BindComponent` 按固定子物体路径查找控件，请保持命名约定：

```
Content/ContainerTitle/Label
Content/ContainerContent/Label
Content/ContainerButtons/ButtonYes
Content/ContainerButtons/ButtonNo
Content/ContainerButtons/ButtonCancel
Mask
```

---

## 7. 加载页与等待 UI

### 7.1 加载页（UILoading）

```csharp
// 显示加载页（带进场特效 + 提示，返回 UILoading 实例）
UILoading loading = UISystem.Instance.ShowLoading("Loading", "loadin_fx", "正在加载...");

// 更新进度与提示
loading.SetPercent(0.5f);        // 进度条 (0~1)
loading.SetInfo("加载配置 50%");
loading.SetTips("小提示：合理分配体力");

// 切换加载提示文本
loading.UpdateInnerTips();

// 隐藏（带退场特效）
UISystem.Instance.HideLoading("loadin_hide_fx", onFinish: null);
UISystem.Instance.HideLoading(); // 直接隐藏
```

加载页的转场特效：

```csharp
loading.LoadFX("loadin_fx");           // 从资源加载进场特效
loading.PlayFX();                      // 播放
loading.PlayHideFX(() => { /* 完成回调 */ }); // 播放退场特效
loading.PlayCompleteFX("loadin_full"); // 播放完整过场
loading.ShowOneFX("uifx_name");
loading.HideAllFX();
```

### 7.2 等待 UI（UIWaiting）

用于短期遮罩等待，**默认延迟 2 秒**出现，避免一闪而过：

```csharp
UISystem.Instance.ShowWaiting(true);   // 显示等待遮罩（内部 2 秒延时）
UISystem.Instance.ShowWaiting(false);  // 关闭
```

### 7.3 加载切换

```csharp
// 显示一个加载页并在 onFinished 回调后切换到一个指定特效
UISystem.Instance.ShowLoadingSwitch("Loading", "switch_fx", () => {
    // 切换特效播放完成
});
UISystem.Instance.GetLoadingUI("Loading"); // 取当前加载页实例
```

---

## 8. 动画系统

### 8.1 基于 NGUI Tween 的动画工厂

`UITweenerFactory` 从动画根节点收集所有 `UITweener[]` 并以名字索引，供 `UIBase` 在 Awake 时复制到目标对象：

```csharp
var factory = new UITweenerFactory();
factory.Init(tweenRoot);                       // 收集 tweenRoot 下所有 UITweener
UITweener[] tweens = factory.GetUITweeners("showAnim"); // 按名取补间
```

### 8.2 UI 显隐动画

在 `OnPreShow` / `OnPreHide` 中播放基于 `UIPlayTween` 的补间，动画名通过 `showAnimClip` / `hideAnimClip` 配置。

### 8.3 页面级动画

`UIPage` 使用 Unity `Animation` 播放 `showAnimClip` / `hideAnimClip`，并通过 `AnimationEvent`（`ShowAnimFinish` / `HideAnimFinish`）回调同步状态。

---

## 9. 文本本地化

### 9.1 TextAdaptor 适配

框架不绑定具体本地化系统，通过 `TextAdaptor` 委托注入（`Func<string,string>`）：

```csharp
UISystem.Instance.TextAdaptor.onGetText = (string key) => {
    return LocalizationManager.Get(key);   // 注入你的本地化实现
};
```

### 9.2 UILabelLocalization 组件

给 `UILabel` 挂上 `UILabelLocalization` 组件并设置 key，即可自动绑定本地化文本：

```csharp
// 标签文本 key 自动从 "txt_xxx" 读取本地化后的文本
labelLocalization.key = "btn_confirm";
labelLocalization.BindText();
```

---

## 10. NGUI 控件扩展库

`NGUIExtend/` 提供约 20 个基于 NGUI 的扩展控件，直接拖拽到 Prefab 即可使用：

| 类别 | 控件 | 说明 |
| --- | --- | --- |
| 动画 | `UIAnimation` | 窗口淡入淡出 / 缩放 / 飞入飞出 |
| | `UIScale` / `UIShake` / `UIRotate` / `UIWidgetFlash` | 缩放 / 抖动 / 旋转 / 闪烁特效 |
| | `TweenScaleAddon` / `TweenAlphaAddon` / `TweenAlphaRespective` / `TweenValue` | Tween 扩展 |
| 按钮 | `UIButtonState` / `UIButtonStateEx` / `UIButtonStateEx2` | 按下 / 弹起状态贴图 |
| | `UIButtonColorAddon` | 按钮颜色 | 
| 灰化 | `UIGraySprite` | 继承 `UISprite`，支持置灰显示 |
| 输入 | `InputController` / `InputChild` | 键盘 / 手柄方向导航 |
| 其他 | `UIEmojiManager` | 表情图集回收 |
| | `UIClose` | 关闭回调 |
| | `NGUIToolsAddon` | 递归 alpha / color |
| | `NGUISetRenderQueue` | 渲染队列设置 |

---

## 11. 与 ResourceSystem 异步联动

UI 的 Prefab 通过 `Dependency/ResourceSystem`（基于 Loxodon AssetBundle）异步加载。资源未加载时，`PresentPage` / `CreatePage` 会进入待加载队列，资源就绪后自动执行。

```csharp
// 资源加载完成后自动 present 该页面：把页面加入 ResourceSystem 监听即可
ResourceSystem.Instance.ListenObject("MainMenu", (ResourceObject res) => {
    if (res.assetObject != null) {
        UISystem.Instance.PresentPage("MainMenu");
    }
});

// 编辑器中可用模拟模式（Simulation），无需打 Bundle 即可调试 UI
```

---

## 12. API 参考

### 12.1 UISystem（单例 `UISystem.Instance`）

**通用 / 注册表**

| 方法 | 说明 |
| --- | --- |
| `SetUIBase(UILayer, name, UIBase)` | 注册 UI 到指定层 |
| `GetUIBase(UILayer, name)` | 取 UI |
| `RemoveUIBase(UILayer, name)` | 移除 UI |
| `GetPage(name)` / `GetWindow(name)` | 取页面 / 窗口 |
| `GetCurrentPage()` | 取当前页 |

**框架固有 UI**

| 方法 | 说明 |
| --- | --- |
| `LoadFrameworkUIs(Action<Exception>)` | 加载框架固有 UI（对话框 / 加载 / 等待） |
| `CreateFrameworkUIs()` | 创建框架固有 UI |
| `DestroyFrameworkUIs()` | 销毁框架固有 UI |

**页面栈**

| 方法 | 说明 |
| --- | --- |
| `CreatePage(name, Action<UIPage,bool>)` | 异步创建页面 |
| `PresentPage(name, hideCurrentPage=true, Action onShow)` | 入栈并显示 |
| `DismissPage(name=null, dismissSelf=true)` | 出栈 |
| `DimissToPage(name)` | 弹出到指定页 |
| `DismissAllPage()` | 清空页面栈 |
| `IsPageInStack(name)` | 页面是否在栈中 |

**窗口**

| 方法 | 说明 |
| --- | --- |
| `CreateWindow(name, Action<UIWindow,bool>)` | 创建窗口 |
| `ShowWindow(name, global=false)` | 显示（global=true 为全局窗口） |
| `HideWindow(name)` / `HideAllWindows()` | 隐藏指定 / 全部 |
| `HideWindowsByPage(UIPage)` | 隐藏某页面附属窗口 |

**对话框 / 加载 / 等待**

| 方法 | 说明 |
| --- | --- |
| `CreateDialog(name)` / `DestroyDialog(name)` | 创建 / 销毁对话框 |
| `CreateLoading(name)` / `DestroyLoading(name)` | 创建 / 销毁加载页 |
| `CreateWaiting(name)` / `DestroyWaiting(name)` | 创建 / 销毁等待 UI |
| `ShowLoading(name, fxName=null, hint=null)` | 显示加载页 |
| `ShowLoading(name, delay)` | 延迟显示加载页 |
| `ShowLoadingSwitch(name, fxName, Action onFinished)` | 带转场切换 |
| `HideLoading(hideFXName, Action onFinish)` / `HideLoading()` | 隐藏加载页 |
| `ShowWaiting(bool show)` | 显示 / 隐藏等待遮罩 |

**属性**

| 属性 | 说明 |
| --- | --- |
| `UILayer` 枚举 | 8 个 UI 层 |
| `TextAdaptor` | 文本本地化适配器 |

### 12.2 UIBase（基类）

| 成员 | 说明 |
| --- | --- |
| `Name` / `Path` / `Parent` | UI 身份与挂载信息 |
| `Show()` / `Show(string uifx)` | 显示 |
| `Hide()` / `Hide(string hideuifx, Action onFinish)` | 隐藏 |
| `IsShowing()` | 是否正在显示 |
| `OnLoad()` / `OnPreShow(Action)` / `OnShowed(Action)` / `OnPreHide(Action)` | 生命周期回调（可覆写） |

### 12.3 UIDialog

| 成员 | 说明 |
| --- | --- |
| `Option` 枚举（`[Flags]`） | `Yes` / `No` / `Cancel` |
| `Show(title, message, options, Action<Option> onCallback)` | 显示模态框 |
| `ShowX(title, message, options, IFunction onCallback)` | 显示（Lua/脚本回调版） |
| `SetButtonText(yesText, noText)` | 自定义按钮文字 |

### 12.4 UILoading

| 成员 | 说明 |
| --- | --- |
| `SetPercent(float)` | 设置进度 (0~1) |
| `SetInfo(string)` / `SetTips(string)` | 设置信息 / 提示文本 |
| `UpdateInnerTips()` | 轮播下一条提示：读取 `hint_count` / `hint_text_N` |
| `LoadFX(string)` / `PlayFX()` | 加载 / 播放进场特效 |
| `PlayHideFX(Action)` | 播放退场特效 |
| `PlayCompleteFX(string, Action)` | 播放完整过场特效 |
| `ShowOneFX(string)` / `HideAllFX()` | 单个 / 全部特效 |

### 12.5 UIWaiting

| 成员 | 说明 |
| --- | --- |
| `Show()` / `Hide()` | 显示（默认 2 秒延时出现）/ 隐藏 |

### 12.6 UITweenerFactory

| 成员 | 说明 |
| --- | --- |
| `Init(GameObject root)` | 收集根节点下所有 `UITweener` |
| `GetUITweeners(string name)` | 按名取补间数组 |

### 12.7 TextAdaptor / ComponentUtil

| 成员 | 说明 |
| --- | --- |
| `TextAdaptor.GetText(string)` | 文本本地化入口 |
| `ComponentUtil.CopyComponent<T>()` | 跨对象复制组件 |
| `ComponentUtil.BindComponent(...)` | 按固定路径查找绑定控件 |
