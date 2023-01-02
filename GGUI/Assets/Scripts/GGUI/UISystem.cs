using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

/**
 * UI系统。分层管理，不同层的内容有不同的表现和从属关系。
 * TODO: GL - 预定义各个层次的数值。
 * TODO: GL - 各层次挂在不同的gameobject上面（pages, windows, dialogs...）
 * 游戏进入一个状态后，先加载一个初始化的Page。然后后台根据配置，自动加载其它Page。
 */
public class UISystem : Singleton<UISystem>
{
    public enum UILayer {
        Root = 0,
        Page,
        Window,
        UIFX,
        Loading,
        Dialog,
        Message,
        Waiting,
        Count,
    };

    private UITweenerFactory uiTweenerFactory = new UITweenerFactory();
    public GameObject uiAnimationRoot = null;
    //public GameObject popTweener;

    private static readonly Dictionary<UILayer, int> UILayerDepth = new Dictionary<UILayer, int>{
        { UILayer.Root, 1},
        { UILayer.Page, 100},
        { UILayer.Window, 200},
        { UILayer.UIFX, 300},
        { UILayer.Loading, 400},
        { UILayer.Dialog, 500},
        { UILayer.Message, 600},
        { UILayer.Waiting, 700},
    };

    private Dictionary<UILayer, Dictionary<string, UIBase>> mUITable = new Dictionary<UILayer, Dictionary<string, UIBase>>((int)UILayer.Count); // 存储所有的UI对象。

    private UIBase mRoot = null;// new UIBase();        // UI的根
    //private Dictionary<string, UIPage> mPages = new Dictionary<string, UIPage>(10);         // 记录系统中的所有Page
    private Stack<UIPage> mPageStack = new Stack<UIPage>(10);                               // 记录当前的页面堆栈。
    private Dictionary<string, UIWindow> mWindows = new Dictionary<string, UIWindow>();     // 记录打开的Window。
    private UIDialog mDialog = null;
    private UILoading mLoading = null;
    private UIWaiting mWaiting = null;
    private Dictionary<string, Action> uiActionTable = new Dictionary<string, Action>();    // ui异步操作表。

    public UITweenerFactory UITweenerFactory {
        get {
            return uiTweenerFactory;
        }
    }

    protected UISystem() {
    }

    private void Awake()
    {
        GameObject root = new GameObject();
        root.AddComponent<UIBase>();
        root.name = "uiRoot";

        for (int i = (int)UILayer.Root; i < (int)UILayer.Count; ++i)
        {
            mUITable[(UILayer)i] = new Dictionary<string, UIBase>();
            GetCreateGroup(((UILayer)i));
        }
        mUITable[UILayer.Root]["Root"] = mRoot;
        uiTweenerFactory.Init(uiAnimationRoot);
        ResourceSystem.Instance.onResourceLoaded += OnUIResourceLoaded;
        //TODO: GL - 构建存储UI对象的分组GameObject
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnUIResourceLoaded(ResourceItem resourceItem) {
        Debug.LogWarning("--->资源加载完成:" + resourceItem.name + "|" + resourceItem.path);
        if (resourceItem == null) {
            return;
        }
        if (!uiActionTable.ContainsKey(resourceItem.name))
        {
            return;
        }
        Action doPresent = uiActionTable[resourceItem.name];
        if (doPresent != null) {
            doPresent();
        }
    }

    /**
     * 获取分层组。如果不存在，则创建。
     */
    private GameObject GetCreateGroup(UILayer layer) {
        GameObject uiRoot = GameObject.FindWithTag("UIRoot");
        if (uiRoot == null)
        {
            throw new Exception("没有找到Tag为UIRoot的对象！");
            return null;
        }
        string layerName = layer.ToString();
        Transform transform = uiRoot.transform.Find(layerName);
        if (transform != null) {
            return transform.gameObject;
        }
        GameObject groupObject = new GameObject(layerName);
        groupObject.layer = LayerMask.NameToLayer("UI");
        groupObject.transform.parent = uiRoot.transform;
        groupObject.transform.localPosition = Vector3.zero;
        groupObject.transform.localRotation = Quaternion.identity;
        groupObject.transform.localScale = Vector3.one;
        UIPanel panel = groupObject.AddComponent<UIPanel>();
        if (UILayerDepth.ContainsKey(layer)) {
            panel.useSortingOrder = true;
            panel.depth = UILayerDepth[layer];
            panel.sortingOrder = UILayerDepth[layer];
        }
        return groupObject;
    }

    public void SetUIBase(UISystem.UILayer layer, string name, UIBase uiBase) {
        if (!mUITable.ContainsKey(layer))
        {
            throw new Exception("UISystem尚未初始化-" + layer.ToString());
            return;
        }
        if (mUITable[layer].ContainsKey(name))
        {
            throw new Exception(string.Format("{0}资源被重复加载！-{1}", layer.ToString(), name));
            return;
        }
        mUITable[layer][name] = uiBase;
    }

    public UIBase GetUIBase(UISystem.UILayer layer, string name) {
        if (!mUITable.ContainsKey(layer)) {
            //throw new Exception("UISystem尚未初始化-"+layer.ToString());
            Debug.LogWarning("UISystem尚未初始化-" + layer.ToString());
            return null;
        }
        if (!mUITable[layer].ContainsKey(name)) {
            //throw new Exception(string.Format("没有找到此UI-{0} | {1}", layer.ToString(), name));
            Debug.LogWarningFormat("没有找到此UI-{0} | {1}。请确认好此UI是没有参与加载还是尚未完成加载！", layer.ToString(), name);
            return null;
        }
        return mUITable[layer][name];
    }

    public void RemoveUIBase(UISystem.UILayer layer, string name) {
        if (!mUITable.ContainsKey(layer))
        {
            return;
        }
        if (!mUITable[layer].ContainsKey(name))
        {
            return;
        }
        mUITable[layer].Remove(name);
    }

    public UIPage GetPage(string name)
    {
        return GetUIBase(UILayer.Page, name) as UIPage;
    }

    public UIWindow GetWindow(string name) {
        return GetUIBase(UILayer.Window, name) as UIWindow;
    }

    /**
     * 获取当前（栈顶的）页面。
     */
    public UIPage GetCurrentPage(){
        if (mPageStack.Count == 0) {
            return null;
        }
        UIPage currentPage = mPageStack.Peek();
        return currentPage;
    }

    public bool LoadFrameworkUIs(Action<Exception> callback) {
        //TODO: 从ResourceSystem获取通用UI的对象。
        //TODO: 异常检查，如果发生异常，弹出原生异常弹框，抛出日志。
        //ResourceSystem.Instance.Load(GameSystem.ResourceGroup_Framework, ResourceSystem.Tag.UI, "UILoading", "Framework/UI/UILoading");
        //ResourceSystem.Instance.Load(GameSystem.ResourceGroup_Framework, ResourceSystem.Tag.UI, "UIDialog", "Framework/UI/UIDialog");
        //ResourceSystem.Instance.Load(GameSystem.ResourceGroup_Framework, ResourceSystem.Tag.UI, "UIWaiting", "Framework/UI/UIWaiting");
        ResourceSystem.Instance.Reset();
        ResourceSystem.Instance.AddResourceItem(new ResourceItem(null, ResourceSystem.Tag.UI, "uiframework", 0));
        // 调用资源加载
        ResourceSystem.Instance.StartLoading(
            (Exception e) => {
                if (callback != null) {
                    callback(e);
                }
            },
            (Exception e) => {
            });

        return true;
    }

    public bool CreateFrameworkUIs() {
        GameDebugger.sPushLog("Create Framework UIs");
        CreateDialog("UIDialog");
        CreateLoading("UILoading");
        CreateWaiting("UIWaiting");
        return true;
    }

    public void DestroyFrameworkUIs() {
        DestroyDialog("UIDialog");
        DestroyLoading("UILoading");
        DestroyWaiting("UIWaiting");
    }

    /**
     * （通过加载的Object）创建通用UI。供整个游戏使用。
     */
    public UIBase CreateCommonUI(UILayer layer, string name) {
        ResourceObject resObject = ResourceSystem.Instance.GetObject(ResourceSystem.Tag.UI, name);
        UIBase uiBase = Create(layer, name, resObject.assetObject);
        return uiBase;
    }

    public UIBase Create(UILayer layer, string name, UnityEngine.Object uiObject) {
        if (uiObject == null) {
            throw new Exception("prefab不能为null！"+name);
            return null;
        }
        GameObject uiRoot = GetCreateGroup(layer);
        GameObject uiObjectInstance = UnityEngine.Object.Instantiate(uiObject) as GameObject;
        UIBase baseUI = uiObjectInstance.GetComponent<UIBase>();
        if (baseUI == null) {
            Destroy(uiObjectInstance);
            throw new Exception("需要挂载UI脚本，例如：UIPage, UIWindow...！" + name);
            return null;
        }
        baseUI.Name = name;
        uiObjectInstance.layer = LayerMask.NameToLayer("UI");
        if (uiRoot != null)
        {
            uiObjectInstance.transform.parent = uiRoot.transform;
        }
        uiObjectInstance.transform.localPosition = Vector3.zero;
        uiObjectInstance.transform.localScale = Vector3.one;
        uiObjectInstance.SetActive(false);

        //baseUI.Path = path;
        //if (popTweener != null && baseUI.tweenObject != null)
        //{
        //    UITweener[] tweeners = popTweener.GetComponentsInChildren<UITweener>(true);
        //    ComponentUtil.CopyComponent<UITweener>(tweeners, baseUI.tweenObject);
        //}
        return baseUI;
    }

    public UIDialog CreateDialog(string name) {
        UIBase uiBase = GetUIBase(UILayer.Dialog, name);
        if (uiBase != null)
        {
            Debug.LogWarningFormat("Dialog {0} already exist!", name);
            return uiBase as UIDialog;
        }
        UIDialog dialog = CreateCommonUI(UILayer.Dialog, name) as UIDialog;
        this.mDialog = dialog;
        SetUIBase(UILayer.Dialog, name, dialog as UIBase);
        this.mDialog.OnLoad();
        return dialog;
    }

    public void DestroyDialog(string name) {
        RemoveUIBase(UILayer.Dialog, name);
        if (this.mDialog != null) {
            Destroy(this.mDialog.gameObject);
            //this.mDialog.transform.parent = null;
            this.mDialog = null;
        }
    }

    public UILoading CreateLoading(string name)
    {
        UIBase uiBase = GetUIBase(UILayer.Loading, name);
        if (uiBase != null)
        {
            Debug.LogWarningFormat("Loading {0} already exist!", name);
            return uiBase as UILoading;
        }
        UILoading loading = CreateCommonUI(UILayer.Loading, name) as UILoading;
        this.mLoading = loading;
        SetUIBase(UILayer.Loading, name, loading as UIBase);
        this.mLoading.OnLoad();
        return loading;
    }

    public void DestroyLoading(string name)
    {
        RemoveUIBase(UILayer.Loading, name);
        if (this.mLoading != null) {
            Destroy(this.mLoading.gameObject);
            //this.mLoading.transform.parent = null;
            this.mLoading = null;
        }
    }

    public UIWaiting CreateWaiting(string name)
    {
        UIBase uiBase = GetUIBase(UILayer.Waiting, name);
        if (uiBase != null)
        {
            Debug.LogWarningFormat("Waiting {0} already exist!", name);
            return uiBase as UIWaiting;
        }
        UIWaiting waiting = CreateCommonUI(UILayer.Waiting, name) as UIWaiting;
        this.mWaiting = waiting;
        SetUIBase(UILayer.Waiting, name, waiting as UIBase);
        this.mWaiting.OnLoad();
        return waiting;
    }

    public void DestroyWaiting(string name)
    {
        RemoveUIBase(UILayer.Waiting, name);
        if (this.mWaiting != null)
        {
            Destroy(this.mWaiting.gameObject);
            //this.mLoading.transform.parent = null;
            this.mWaiting = null;
        }
    }

    public void CreatePage(string name, Action<UIPage, bool> callback)
    {
        UIPage page = null;
        try
        {
            UIBase uiBase = GetUIBase(UILayer.Page, name);
            if (uiBase != null)
            {
                Debug.LogWarningFormat("Page {0} already exist!", name);
                if (callback != null)
                {
                    callback(uiBase as UIPage, true);
                }
                //return uiBase as UIPage;
            }
            //ResourceObject resObject = ResourceSystem.Instance.GetObject(ResourceSystem.Tag.UI, name);
            ResourceSystem.Instance.ListenObject(name, (ResourceObject resObject) => {
                page = Create(UILayer.Page, name, resObject.assetObject) as UIPage;
                page.Parent = mRoot;
                SetUIBase(UILayer.Page, name, page as UIBase);

                ResourceObject[] componentList = ResourceSystem.Instance.GetAssets<UIComponent>(ResourceSystem.Tag.UI, resObject.bundleName);
                foreach (ResourceObject component in componentList)
                {
                    page.SetComponent(component.assetName, component.assetObject as UIComponent);
                }

                page.OnLoad();
                if (callback != null)
                {
                    callback(page, false);
                }
            });
        }
        catch (Exception e) {
            Debug.LogError("CreatePage failed!-"+ e.ToString());
            throw e;
        }
        //return page;
    }

    public void CreatePageX(string name, LuaFunction callback) {
        CreatePage(name, (UIPage page, bool exist) => {
            if (callback != null)
            {
                callback.Call(page, exist);
            }
        });
    }

    public void CreateWindow(string name, Action<UIWindow, bool> callback)
    {
        try {
            UIBase uiBase = GetUIBase(UILayer.Window, name);
            if (uiBase != null)
            {
                Debug.LogWarningFormat("Window {0} already exist!", name);
                if (callback != null)
                {
                    callback(uiBase as UIWindow, true);
                }
                //return uiBase as UIWindow;
            }
            //ResourceObject resObject = ResourceSystem.Instance.GetObject(ResourceSystem.Tag.UI, name);
            ResourceSystem.Instance.ListenObject(name, (ResourceObject resObject) =>
            {
                UIWindow window = Create(UILayer.Window, name, resObject.assetObject) as UIWindow;
                //window.Parent = mRoot;
                SetUIBase(UILayer.Window, name, window as UIBase);
                window.OnLoad();
                if (callback != null)
                {
                    callback(window, false);
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError("CreateWindow failed!-" + e.ToString());
            throw e;
        }
        //return window;
    }

    public void CreateWindowX(string name, LuaFunction callback)
    {
        CreateWindow(name, (UIWindow window, bool exist) => {
            if (callback != null)
            {
                callback.Call(window, exist);
            }
        });
    }

    /**
     * 显示指定页面
     */
    public bool PresentPage(string name, bool hideCurrentPage = true, Action onShow = null) {
        Debug.Log("CS PresentPage->" + name);
        Action doPresent = () => {
            UIPage currentPage = null;
            if (mPageStack.Count > 0)
            {
                if (hideCurrentPage)
                {
                    currentPage = mPageStack.Peek();
                }
            }

            UIPage newPage = GetPage(name);
            mPageStack.Push(newPage);
            newPage.Show();
            if (currentPage != null)
            {
                currentPage.Hide();
                //currentPage.gameObject.SetActive(false);
            }
            if (onShow != null) {
                onShow();
            }
        };
        if (GetUIBase(UILayer.Page, name) == null) {
            if (!uiActionTable.ContainsKey(name))
            {
                uiActionTable[name] = doPresent;
            }
            ResourceSystem.Instance.ReorderDownloadListByAssetName(name);
            return false;
        }
        doPresent();
        //newPage.gameObject.SetActive(true);
        return true;
    }

    public bool PresentPageX(string name, bool hideCurrentPage = true, LuaFunction onShow = null) {
        return PresentPage(name, hideCurrentPage, () =>
        {
            if (onShow != null) {
                onShow.Call();
            }
        });
    }

    private void HidePage(UIPage page) {
        if (page != null)
        {
            // 如果当前对话框的父是此Page，则隐藏对话框。
            //TODO: GL - 如果对话框是必须要响应的，则要阻止页面弹出。对话框也要做一下遮挡。
            if (mDialog.Parent == page)
            {
                if (mDialog.IsShowing())
                {
                    mDialog.Hide();
                }
            }
            HideWindowsByPage(page);
            page.Hide();
        }
    }

    /// <summary>
    /// 弹出页面到指定Page（此Page不弹）。
    /// </summary>
    /// <param name="name"></param>
    public void DimissToPage(string name) {
        DismissPage(name, false);
    }

    /// <summary>
    /// 弹出页面。
    /// 如果没有指定名字，则隐藏当前最顶层页面。
    /// </summary>
    /// <param name="dismissSelf">true-弹出自身。false-不弹自身。</param>
    public void DismissPage(string name=null, bool dismissSelf=true) {
        if (mPageStack.Count == 0) {
            return;
        }
        UIPage page = null;
        if (name != null)
        {
            page = GetPage(name);
            if (page != null && mPageStack.Contains(page))
            {
                UIPage hidePage = null;
                do {
                    hidePage = mPageStack.Peek();
                    if (hidePage == null) {
                        break;
                    }
                    // 当找到指定Page时，把它显示出来，结束。
                    if (!dismissSelf && hidePage.Name == name)
                    {
                        hidePage.Show();
                        break;
                    }
                    hidePage = mPageStack.Pop();
                    HidePage(hidePage);
                    if (dismissSelf && hidePage.Name == name)
                    {
                        if (mPageStack.Count != 0)
                        {
                            hidePage = mPageStack.Peek();
                            if (hidePage != null) {
                                hidePage.Show();
                            }
                        }
                        break;
                    }
                } while (mPageStack.Count > 0);
            }
            else
            {
                return;
            }
        }
        else {
            page = mPageStack.Pop();
            HidePage(page);
            if (mPageStack.Count != 0)
            {
                page = mPageStack.Peek();
                page.Show();
            }
        }
    }

    public void DismissAllPage()
    {
        while (mPageStack.Count > 0)
        {
            DismissPage();
        }
    }

    public bool IsPageInStack(string name) {
        UIPage page = GetPage(name);
        if (page == null)
        {
            return false;
        }
        if (!mPageStack.Contains(page)) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 显示窗体。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="global">是否为全局窗体，如果不是全局窗体，则附属于当前Page，当Page关闭的时候，附属的Windows都会被关闭。</param>
    public void ShowWindow(string name, bool global=false) {
        UIWindow window = GetWindow(name);
        if (window == null || window.IsShowing()) {
            return;
        }
        if (global)
        {
            window.Parent = null;
        }
        else {
            window.Parent = this.GetCurrentPage();
        }
        window.Show();
        if (!mWindows.ContainsKey(name)) {
            mWindows[name] = window;
        }
    }

    public void HideWindow(string name)
    {
        UIWindow window = GetWindow(name);
        if (window == null || !window.IsShowing())
        {
            return;
        }
        window.Parent = null;
        window.Hide();
        if (mWindows.ContainsKey(name))
        {
            mWindows.Remove(name);
        }
    }

    public void HideAllWindows() {
        foreach (string key in mWindows.Keys) {
            HideWindow(key);
        }
    }

    public void HideWindowsByPage(UIPage page) {
        foreach (UIWindow window in mUITable[UILayer.Window].Values) {
            if (window != null && window.Parent == page && window.IsShowing()) {
                window.Hide();
            }
        }
    }

    public UIDialog PopDialog(string title, string message, int options = (int)UIDialog.Option.Yes, Action<UIDialog.Option> onCallback = null) {
        Debug.Assert(this.mDialog != null);
        try
        {
            // 给此对话框设置当前页面为父级页面。
            if (this.mDialog.IsShowing())
            {
                this.mDialog.Hide();
            }
            this.mDialog.Show(title, message, options, onCallback);
            mDialog.Parent = this.GetCurrentPage();
        }
        catch (Exception e) {
            Debug.LogError(e);
            if (onCallback != null) {
                onCallback(UIDialog.Option.Yes);
            }
        }
        return mDialog;
    }

    public UIDialog PopDialogX(string title, string message, int options = (int)UIDialog.Option.Yes, LuaFunction onLuaCallback = null)
    {
        Debug.Assert(this.mDialog != null);
        try
        {
            if (this.mDialog.IsShowing())
            {
                this.mDialog.Hide();
            }
            this.mDialog.ShowX(title, message, options, onLuaCallback);
            mDialog.Parent = this.GetCurrentPage();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        return mDialog;
    }

    public void PopDialogX(string message, int options = (int)UIDialog.Option.Yes, LuaFunction onLuaCallback = null)
    {
        PopDialogX("", message, options, onLuaCallback);
    }

    public void HideDialog() {
        Debug.Assert(this.mDialog != null);
        if (this.mDialog.IsShowing())
        {
            this.mDialog.Hide();
            mDialog.Parent = null;
        }
    }

    public UILoading GetLoadingUI(string name=null) {
        if (string.IsNullOrEmpty(name))
        {
            return mLoading;
        }
        else {
            UILoading loading = GetUIBase(UILayer.Loading, name) as UILoading;
            return loading;
        }
    } 

    /// <summary>
    /// 展示转场动画
    /// </summary>
    /// <param name="name">Loading页面名字，有疑问找公雷</param>
    /// <param name="fxName">动画名</param>
    /// <param name="onFinished">动画播放结束的回调</param>
    public void ShowLoadingSwitch(string name, string fxName, Action onFinished = null)
    {
        //if (this.mLoading != null)
        //{
        //    HideLoading();
        //}
        UILoading loading = GetUIBase(UILayer.Loading, name) as UILoading;

        if (loading != null)
        {
            if (!loading.IsShowing())
            {
                loading.Show();
                loading.PlayCompleteFX(fxName,onFinished);
            }
            this.mLoading = loading;
        }
    }

    /// <summary>
    /// 显示加载页面
    /// </summary>
    /// <param name="name">加载页面的名称</param>
    /// <param name="fxName">特效名称</param>
    /// <param name="hint">提示文本。如果是null，则定时滚动随机播放hint_text_文本。如果不是null，则显示hint参数内容</param>
    /// <returns></returns>
    public UILoading ShowLoading(string name, string fxName = null, string hint = null) {
        //if (this.mLoading != null) {
        //    HideLoading();
        //}
        UILoading loading = GetUIBase(UILayer.Loading, name) as UILoading;
        loading.SetTips(hint);

        if (loading != null) {
            if (!loading.IsShowing())
            {
                loading.Show(fxName);
            }
            this.mLoading = loading;
        }
        return loading;
    }

    /// <summary>
    /// 延迟显示加载页面
    /// </summary>
    /// <param name="name"></param>
    /// <param name="delay"></param>
    public void ShowLoading(string name, float delay) {
        IEnumerator DelayShowLoading(string iName, float iDelay)
        {
            yield return new WaitForSeconds(iDelay);
            ShowLoading(iName);
        }

        CoroutineUtil.DoCoroutine(DelayShowLoading(name, delay));
    }

    public void HideLoading(string hideFXName,Action onFinish = null)
    {
        if (this.mLoading != null && this.mLoading.IsShowing())
        {
            this.mLoading.Hide(hideFXName, onFinish);
        }
        this.mLoading = null;
    }

    public void HideLoading() {
        if (this.mLoading != null && this.mLoading.IsShowing()) {
            this.mLoading.Hide();
        }
        this.mLoading = null;
    }

    public void ShowWaiting(bool show) {
        if (this.mWaiting == null) {
            return;
        }
        if (show)
        {
            this.mWaiting.Show();
        }
        else {
            this.mWaiting.Hide();
        }
    }
}
