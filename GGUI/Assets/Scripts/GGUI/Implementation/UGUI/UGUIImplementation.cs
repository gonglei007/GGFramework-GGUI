using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace GGFramework.GGUI.UGUI
{
    /// <summary>UGUI/TMP backend. Existing canvases and EventSystems are reused unchanged.</summary>
    public class UGUIImplementation : IUIImplementation, IDisposable
    {
        sealed class ClickBinding { public GameObject Target; public Button Button; public UGUIClickRelay Relay; public Action<GameObject> Handler; public UnityAction Wrapped; }
        readonly List<ClickBinding> clicks=new List<ClickBinding>();
        public string Name=>"UGUI";
        public void Init(GameObject uiRoot,GameObject animationRoot)
        {
            if(!uiRoot)throw new ArgumentNullException(nameof(uiRoot));
            if(!uiRoot.GetComponentInParent<Canvas>(true))
            {
                var canvas=uiRoot.GetComponent<Canvas>()??uiRoot.AddComponent<Canvas>();
                canvas.renderMode=RenderMode.ScreenSpaceOverlay;
                if(!uiRoot.GetComponent<GraphicRaycaster>())uiRoot.AddComponent<GraphicRaycaster>();
            }
            if(!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
                new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        }
        public GameObject CreateLayerGroup(string layerName,int depth,Transform parent)
        {
            if(!parent)throw new ArgumentNullException(nameof(parent));
            var go=new GameObject(layerName,typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));
            var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var canvas=go.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=depth;
            return go;
        }
        public IUITweener CreateTweener(GameObject target,string animName)=>new UGUITweener(target,animName);
        public GameObject FindGameObject(GameObject root,string path)
        {if(!root)return null;return string.IsNullOrEmpty(path)?root:root.transform.Find(path)?.gameObject;}
        public IUIText FindText(GameObject root,string path)=>WrapText(FindGameObject(root,path));
        public IUIButton FindButton(GameObject root,string path)
        {var button=FindGameObject(root,path)?.GetComponent<Button>();return button?new UGUIButton(button):null;}
        public IUIProgressBar FindProgressBar(GameObject root,string path)=>WrapProgressBar(FindGameObject(root,path));
        public IUILayoutGroup FindLayoutGroup(GameObject root,string path)
        {var go=FindGameObject(root,path);return go&&go.GetComponent<LayoutGroup>()?new UGUILayoutGroup(go):null;}
        public IUIText WrapText(GameObject go)
        {
            if(!go)return null;
            var tmp=go.GetComponent<TMP_Text>();if(tmp)return new UGUITMPText(tmp);
            var text=go.GetComponent<Text>();return text?new UGUIText(text):null;
        }
        public IUIProgressBar WrapProgressBar(GameObject go)
        {var slider=go?go.GetComponent<Slider>():null;return slider?new UGUIProgressBar(slider):null;}
        public void AddClickHandler(GameObject target,Action<GameObject> handler)
        {
            if(!target||handler==null)return;
            clicks.RemoveAll(b=>!b.Target);
            if(clicks.Any(b=>b.Target==target&&b.Handler==handler))return;
            var binding=new ClickBinding{Target=target,Handler=handler,Button=target.GetComponent<Button>()};
            if(binding.Button){binding.Wrapped=()=>handler(target);binding.Button.onClick.AddListener(binding.Wrapped);}
            else{binding.Relay=target.GetComponent<UGUIClickRelay>()??target.AddComponent<UGUIClickRelay>();binding.Relay.Add(handler);}
            clicks.Add(binding);
        }
        public void RemoveClickHandler(GameObject target,Action<GameObject> handler)
        {
            foreach(var binding in clicks.Where(b=>ReferenceEquals(b.Target,target)&&b.Handler==handler).ToArray())
            {Unbind(binding);clicks.Remove(binding);}
        }
        static void Unbind(ClickBinding binding)
        {if(binding.Button)binding.Button.onClick.RemoveListener(binding.Wrapped);if(binding.Relay)binding.Relay.Remove(binding.Handler);}
        public void Dispose(){foreach(var binding in clicks)Unbind(binding);clicks.Clear();}
    }
}
