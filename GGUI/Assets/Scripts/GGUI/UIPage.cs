using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaFramework;
using LuaInterface;

/// <summary>
/// UI页面类。一个全屏页面。每个时刻只显示一个Page，是堆栈化的管理。
/// 加载一个Page的时候，会自动的加载同bundle下的Component。释放一个Page的时候，释放掉此Page同bundle中的Component。
/// </summary>
public class UIPage : UIBase
{
    private Animation animationCtrl;
    private AnimationClip clip;
    private UITweener uiTweener;
    private Action onShowFinish;
    private Action onHideFinish;

    public LuaFunction luaOnPreShow;//Lua回调
    public LuaFunction luaOnShow;
    public LuaFunction luaOnShowed;

    private Dictionary<string, UIComponent> componentDict = new Dictionary<string, UIComponent>();

    public void SetComponent(string name, UIComponent component) {
        componentDict[name] = component;
    }

    public new UIComponent GetComponent(string name)
    {
        UIComponent component = null;
        componentDict.TryGetValue(name, out component);
        return component;
    }

    public override void OnLoad()
    {
        //获取动画结点（如果有）。
        //animation名称约定为"showAnimClip"与"hideAnimClip"。
        animationCtrl = gameObject.GetComponent<Animation>();
        base.OnLoad();
    }

    public override void Show(string uifx)
    {
        base.Show(uifx);
    }

    public override void OnPreShow(Action onFinished)
    {
        if (this.uifx != null) {
            //TODO: play uifx
        }
        //Action action = () =>
        //{
        //    base.OnPreShow(onFinished);
        //};
        //luaOnPreShow.Call(action);
        if (luaOnPreShow != null) {
            luaOnPreShow.Call();
        }
        base.OnPreShow(onFinished);
    }

    public override void OnShowed(Action onFinished)
    {
        if (this.uifx != null) {
            //TODO: stop uifx
            //TODO: on uifx finished callback.
        }
        if (animationCtrl != null)
        {
            clip = animationCtrl.GetClip(showAnimationName);
            if (clip != null)
            {
                //播放UI动画
                animationCtrl.Play(showAnimationName);
                // 以下:添加回调
                AnimationEvent animationEvent = new AnimationEvent();
                animationEvent.functionName = "ShowAnimFinish";
                animationEvent.time = clip.length;
                clip.AddEvent(animationEvent);
                //由于AnimationEvent不支持传Action类型的参数，所以将回调中转一下
                onShowFinish = onFinished;
            }
            else
            {
                //如果没有动画片段，同样调用父类方法
                //Action action = () =>
                //{
                //    base.OnShowed(onFinished);
                //};
                //luaOnShowed.Call(action);
                if (luaOnShowed != null)
                {
                    luaOnShowed.Call();
                }
                base.OnShowed(onFinished);
            }
        }
        else
        {
            //如果没有动画组件，直接调用父类方法
            //Action action = () =>
            //{
            //    base.OnShowed(onFinished);
            //};
            //luaOnShowed.Call(action);
            if (luaOnShowed != null) {
                luaOnShowed.Call();
            }
            base.OnShowed(onFinished);
        }
    }

    private void ShowAnimFinish()
    {
        base.OnShowed(onShowFinish);
    }

    public override void OnPreHide(Action onFinished)
    {
        if (animationCtrl != null)
        {
            clip = animationCtrl.GetClip(hideAnimationName);
            if (clip != null)
            {
                //播放UI动画
                animationCtrl.Play(hideAnimationName);
                // 以下:添加回调
                AnimationEvent animationEvent = new AnimationEvent();
                animationEvent.functionName = "HideAnimFinish";
                animationEvent.time = clip.length;
                clip.AddEvent(animationEvent);
                //由于AnimationEvent不支持传Action类型的参数，所以将回调中转一下
                onHideFinish = onFinished;
            }
            else
            {
                base.OnPreHide(onFinished);
            }
        }
        else
        {
            //如果没有动画组件，直接调用父类方法
            base.OnPreHide(onFinished);
        }
    }

    private void HideAnimFinish()
    {
        base.OnShowed(onHideFinish);
    }
}
