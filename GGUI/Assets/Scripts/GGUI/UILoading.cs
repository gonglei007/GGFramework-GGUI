using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 加载页面。显示加载页面的基本信息：背景、进度条、文本信息。
 * 如需扩展，到Lua里面做一个View来扩展。
 * 已加扩展：支持转场动画
 */
public class UILoading : UIBase
{
    public UILabel labelInfo = null;
    public UILabel labelTips = null;
    public UIProgressBar pbLoading = null;
    public float hintSwitchTime = 3.0f;
    public Transform fxRoot;
    public GameObject baseLoadingRoot;
    public Dictionary<string, Animation> fxDic = new Dictionary<string, Animation>();

    private string hint = null;
    private float hintSwitchTimer = 0.0f;
    //private float percent = 0.0f;
    //private string info = "";

    private void Update()
    {
        if (hint == null) {
            hintSwitchTimer += Time.deltaTime;
            if (hintSwitchTimer > hintSwitchTime) {
                UpdateInnerTips();
                hintSwitchTimer = 0.0f;
            }
        }
    }

    public override void OnLoad() {
        //TODO: 获取页面上的对象。
    }

    public void SetPercent(float percent) {
        if (pbLoading != null) {
            pbLoading.value = percent;
        }
    }

    public void SetInfo(string info){
        if (labelInfo != null)
        {
            labelInfo.text = info;
        }
    }

    public void SetTips(string tips) {
        hint = tips;
        if (labelTips != null)
        {
            labelTips.text = hint == null? "" : hint;
        }
    }

    public void UpdateInnerTips() {
        try
        {
            string strHintCount = TextSystem.GetText("hint_count");

            int hintCount = 0;
            bool result = int.TryParse(TextSystem.GetText("hint_count"), out hintCount);
            if (result) {
                if (hintCount > 0)
                {
                    int hintIndex = UnityEngine.Random.Range(1, hintCount);
                    string strHint = TextSystem.GetText("hint_text_"+hintIndex.ToString());
                    labelTips.text = strHint == null? "" : strHint;
                }
            }
        }
        catch (Exception e) {
            Debug.LogError(e);
        }
    }

    /// <summary>
    /// 加载动画资源，一次只加载一个，方便以后扩展
    /// </summary>
    /// <param name="fxName">动画名字</param>
    public void LoadFX(string fxName)
    {
        //TODO: 加载动画,完成后调用回调函数
        ResourceObject temp = ResourceSystem.Instance.GetObject(ResourceSystem.Tag.UI, fxName);
        if (temp.assetObject == null) {
            Debug.LogError("此特效资源没有加载！-" + fxName);
            return;
        }
        GameObject fx = GameObject.Instantiate(temp.assetObject as GameObject);
        if (fx != null)
        {
            Animation anim = fx.GetComponent<Animation>();
            if (fx.GetComponent<UIAnimationClipEvent>() == null)
            {
                fx.AddComponent<UIAnimationClipEvent>();
            }
            fx.transform.parent = fxRoot;
            fx.transform.localScale = Vector3.one;
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localRotation =  Quaternion.Euler(Vector3.zero);
            fx.gameObject.SetActive(false);
            if (fxDic.ContainsKey(fxName))
            {
                GameObject.Destroy(fxDic[fxName]);
                fxDic[fxName] = anim;
            }
            else
            {
                fxDic.Add(fxName, anim);
            }
        }
        else
        {
            Debug.LogWarning("动画 " + fxName + " 不存在！");
        }
    }

    /// <summary>
    /// 播放入场动画
    /// </summary>
    public void PlayFX()
    {
        //调用动画,关闭普通加载页面
        if (uifx != null && fxDic.ContainsKey(uifx))
        {
            ShowOneFX(uifx);
            fxDic[uifx].gameObject.SetActive(true);
            fxDic[uifx].Play();
        }
    }
    /// <summary>
    /// 播放关闭动画，要等关闭动画播放完毕才开始回调
    /// </summary>
    /// <param name="onFinished">结束回调</param>
    public void PlayHideFX(Action onFinished = null)
    {
        //调用关闭动画,结束时调用回调
        if (hideuifx != null && fxDic.ContainsKey(hideuifx))
        {
            ShowOneFX(hideuifx);
            fxDic[hideuifx].gameObject.SetActive(true);
            fxDic[hideuifx].Play();
            //取得动画GameObject上的Event脚本
            UIAnimationClipEvent clipEvent = fxDic[hideuifx].gameObject.GetComponent<UIAnimationClipEvent>();
            if (clipEvent != null)
            {
                //注册回调
                clipEvent.endEvent = () => {
                    onFinished?.Invoke();
                };
            }
            else
            {
                onFinished?.Invoke();
            }
        }
        else
        {
            HideAllFX();
            onFinished?.Invoke();
        }
    }
    /// <summary>
    /// 播放一个完整的过场效果，要等关闭动画播放完毕才开始回调
    /// </summary>
    /// <param name="fxName">动画名字</param>
    /// <param name="onFinished">结束回调</param>
    public void PlayCompleteFX(string fxName, Action onFinished = null)
    {
        baseLoadingRoot.SetActive(false);
        //调用关闭动画,结束时调用回调
        if (fxName != null && fxDic.ContainsKey(fxName))
        {
            ShowOneFX(fxName);
            fxDic[fxName].gameObject.SetActive(true);
            fxDic[fxName].Play();
            //取得动画GameObject上的Event脚本
            UIAnimationClipEvent clipEvent = fxDic[fxName].gameObject.GetComponent<UIAnimationClipEvent>();
            if (clipEvent != null)
            {
                //注册回调
                clipEvent.endEvent = () => {
                    base.Hide();
                    onFinished?.Invoke();
                };
            }
            else
            {
                base.Hide();
                onFinished?.Invoke();
            }
        }
        else
        {
            HideAllFX();
            base.Hide();
            onFinished?.Invoke();
        }
    }
    /// <summary>
    /// 显示单个动画GameObject
    /// </summary>
    /// <param name="uifxName"></param>
    public void ShowOneFX(string uifxName = null)
    {
        foreach (var item in fxDic)
        {
            if (uifxName != null && item.Key == uifxName)
            {
                item.Value.gameObject.SetActive(true);
            }
            else
            {
                item.Value.gameObject.SetActive(false);
            }
        }
    }
    /// <summary>
    /// 关闭所有动画GameObject
    /// </summary>
    public void HideAllFX()
    {
        foreach (var item in fxDic)
        {
            item.Value.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// 重新父类方法，主要为了播放动画，播放动画同时调用回调，方便同步动画加载资源
    /// </summary>
    /// <param name="onFinished"></param>
    public override void OnShowed(Action onFinished)
    {
        UpdateInnerTips();
        if (uifx != null)
        {
            baseLoadingRoot.SetActive(false);
            PlayFX();
        }
        else
        {
            //打开普通加载页面
            HideAllFX();
            baseLoadingRoot.SetActive(true);
        }
        base.OnShowed(onFinished);
    }

    public override void OnPreHide(Action onFinished)
    {
        PlayHideFX(()=> {
            base.OnPreHide(onFinished);
        });
    }
}
