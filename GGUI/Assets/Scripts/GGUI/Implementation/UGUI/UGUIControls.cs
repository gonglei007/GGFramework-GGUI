using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace GGFramework.GGUI.UGUI
{
    public class UGUIText:IUIText
    {
        readonly Text target;
        public UGUIText(Text target){this.target=target;}
        public string text{get=>target?target.text:null;set{if(target)target.text=value;}}
        public GameObject gameObject=>target?target.gameObject:null;
    }
    public class UGUITMPText:IUIText
    {
        readonly TMP_Text target;
        public UGUITMPText(TMP_Text target){this.target=target;}
        public string text{get=>target?target.text:null;set{if(target)target.text=value;}}
        public GameObject gameObject=>target?target.gameObject:null;
    }
    public class UGUIButton:IUIButton
    {
        readonly Button button;
        readonly Dictionary<Action<GameObject>,UnityAction> handlers=new Dictionary<Action<GameObject>,UnityAction>();
        public UGUIButton(Button button){this.button=button;}
        public GameObject gameObject=>button?button.gameObject:null;
        public void SetActive(bool active){if(button)button.gameObject.SetActive(active);}
        public void AddClickListener(Action<GameObject> handler)
        {if(!button||handler==null||handlers.ContainsKey(handler))return;UnityAction wrapped=()=>handler(button.gameObject);handlers.Add(handler,wrapped);button.onClick.AddListener(wrapped);}
        public void RemoveClickListener(Action<GameObject> handler)
        {if(handler==null||!handlers.TryGetValue(handler,out var wrapped))return;if(button)button.onClick.RemoveListener(wrapped);handlers.Remove(handler);}
    }
    public class UGUIProgressBar:IUIProgressBar
    {
        readonly Slider slider;
        public UGUIProgressBar(Slider slider){this.slider=slider;}
        public float value{get=>slider?slider.normalizedValue:0;set{if(slider)slider.normalizedValue=value;}}
        public GameObject gameObject=>slider?slider.gameObject:null;
    }
    public class UGUILayoutGroup:IUILayoutGroup
    {
        readonly GameObject target;
        readonly GridLayoutGroup grid;
        readonly Dictionary<LayoutElement,float> original=new Dictionary<LayoutElement,float>();
        float width;
        public UGUILayoutGroup(GameObject target){this.target=target;grid=target.GetComponent<GridLayoutGroup>();width=grid?grid.cellSize.x:0;}
        public float cellWidth
        {
            get=>width;
            set
            {
                width=value;
                if(grid){var size=grid.cellSize;if(value>0)size.x=value;grid.cellSize=size;return;}
                if(!target.GetComponent<HorizontalLayoutGroup>())return;
                foreach(Transform child in target.transform)
                {
                    var element=child.GetComponent<LayoutElement>()??child.gameObject.AddComponent<LayoutElement>();
                    if(!original.ContainsKey(element))original[element]=element.preferredWidth;
                    element.preferredWidth=value>0?value:original[element];
                }
            }
        }
        public void Reposition(){if(target&&target.transform is RectTransform rect)LayoutRebuilder.ForceRebuildLayoutImmediate(rect);}
        public GameObject gameObject=>target;
    }
    public class UGUITweener:IUITweener
    {
        readonly GameObject target;readonly string clipName;
        public UGUITweener(GameObject target=null,string clipName=null){this.target=target;this.clipName=clipName;}
        public void Play(bool forward)
        {
            if(!target)return;
            var animation=target.GetComponent<Animation>();
            if(animation&&!string.IsNullOrEmpty(clipName)&&animation[clipName]!=null)
            {var state=animation[clipName];state.speed=forward?1:-1;state.time=forward?0:state.length;animation.Play(clipName);return;}
            var fade=target.GetComponent<UGUIFade>()??target.AddComponent<UGUIFade>();fade.Play(forward,.16f);
        }
    }
}
