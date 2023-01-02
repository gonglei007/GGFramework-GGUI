using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此脚本用于解决UI动画回调的问题，回调默认加到主Clip上
/// </summary>
public class UIAnimationClipEvent : MonoBehaviour
{
    public Action startEvent;//动画开始播放回调，需使用请注册在这个Action上
    public Action endEvent;//动画播放结束回调，需使用请注册在这个Action上

    public enum EvnetPlace
    {
        Start,
        End,
    }
    // Start is called before the first frame update
    void Start()
    {
        //开始时将Action注册到动画切片上，由于此次使用只有一个动画切片，所以注册到主动画切片上
        Animation animation = this.gameObject.GetComponent<Animation>();
        string clipName = animation.clip.name;
        AddAnimationEvent(animation, clipName, EvnetPlace.Start, "OnStart");
        AddAnimationEvent(animation, clipName, EvnetPlace.End, "OnEnd");
    }

    /// <summary>
    /// 添加事件回调
    /// </summary>
    /// <param name="animation">动画脚本</param>
    /// <param name="clipName">动画切片名字</param>
    /// <param name="evnetPlace">设置回调在开头还是结尾</param>
    /// <param name="evnetFunction">Function名字</param>
    private void AddAnimationEvent(Animation animation, string clipName, EvnetPlace evnetPlace, string evnetFunction)
    {
        //clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        AnimationClip clip = animation.GetClip(clipName);
        //创建动画事件
        AnimationEvent animationEvent = new AnimationEvent();
        //设置事件回掉函数名字
        animationEvent.functionName = evnetFunction;
        //传入参数
        //animationEvent.objectReferenceParameter = target;
        //设置触发帧
        if (evnetPlace == EvnetPlace.Start)
        {
            animationEvent.time = 0f;
        }
        else if (evnetPlace == EvnetPlace.End)
        {
            animationEvent.time = clip.length;
        }
        //注册事件
        clip.AddEvent(animationEvent);
    }

    void OnStart()
    {
        startEvent?.Invoke();
    }
    void OnEnd()
    {
        endEvent?.Invoke();
    }

}
