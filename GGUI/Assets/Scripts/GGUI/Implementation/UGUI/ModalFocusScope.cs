using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace GGFramework.GGUI.UGUI
{
    public class ModalFocusScope:MonoBehaviour
    {
        static readonly List<ModalFocusScope> scopes=new List<ModalFocusScope>();static int handledFrame=-1;
        public Action escape;GameObject previous,preferred,last;
        public static bool OwnsKeyboard=>handledFrame==Time.frameCount||Top!=null;
        public static bool Handle(KeyCode key,bool reverse=false)
        {
            var top=Top;if(!top)return false;if(key==KeyCode.Tab){top.Cycle(reverse?-1:1);return true;}
            if(key==KeyCode.Escape){handledFrame=Time.frameCount;top.escape?.Invoke();return true;}return false;
        }
        static ModalFocusScope Top=>scopes.Where(s=>s&&s.gameObject.activeInHierarchy).OrderBy(s=>s,Comparer<ModalFocusScope>.Create((a,b)=>Compare(a.transform,b.transform))).LastOrDefault();
        static int Compare(Transform a,Transform b){var x=new List<int>();var y=new List<int>();for(var t=a;t;t=t.parent)x.Add(t.GetSiblingIndex());for(var t=b;t;t=t.parent)y.Add(t.GetSiblingIndex());x.Reverse();y.Reverse();for(int i=0;i<Math.Min(x.Count,y.Count);i++)if(x[i]!=y[i])return x[i].CompareTo(y[i]);return x.Count.CompareTo(y.Count);}
        protected virtual void OnEnable(){scopes.Add(this);previous=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;}
        protected virtual void OnDisable(){scopes.Remove(this);if(EventSystem.current&&EventSystem.current.currentSelectedGameObject&&EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform)){if(previous&&previous.activeInHierarchy)EventSystem.current.SetSelectedGameObject(previous);else EventSystem.current.SetSelectedGameObject(null);}}
        public void Activate(GameObject target=null){preferred=target;Ensure();}
        Selectable[] Controls()=>GetComponentsInChildren<Selectable>().Where(s=>s.IsActive()&&s.IsInteractable()).ToArray();
        void Ensure(){if(Top!=this||!EventSystem.current)return;var selected=EventSystem.current.currentSelectedGameObject;if(selected&&selected.activeInHierarchy&&selected.transform.IsChildOf(transform))return;var target=preferred&&preferred.activeInHierarchy?preferred:Controls().FirstOrDefault()?.gameObject;EventSystem.current.SetSelectedGameObject(target);}
        public void Cycle(int step)
        {
            if(Top!=this||!EventSystem.current)return;handledFrame=Time.frameCount;var controls=Controls();if(controls.Length==0)return;int index=Array.FindIndex(controls,s=>s.gameObject==EventSystem.current.currentSelectedGameObject);index=(index+step+controls.Length)%controls.Length;EventSystem.current.SetSelectedGameObject(controls[index].gameObject);Reveal(controls[index].transform);
        }
        static void Reveal(Transform target)
        {
            var scroll=target.GetComponentInParent<ScrollRect>();if(!scroll||!scroll.vertical||!scroll.content||!target.IsChildOf(scroll.content))return;var viewport=scroll.viewport?scroll.viewport:(RectTransform)scroll.transform;var corners=new Vector3[4];((RectTransform)target).GetWorldCorners(corners);float top=corners.Max(p=>viewport.InverseTransformPoint(p).y),bottom=corners.Min(p=>viewport.InverseTransformPoint(p).y);float shift=top>viewport.rect.yMax?viewport.rect.yMax-top:bottom<viewport.rect.yMin?viewport.rect.yMin-bottom:0;scroll.StopMovement();scroll.content.anchoredPosition+=new Vector2(0,shift);
        }
        protected virtual void Update(){if(Top!=this)return;Ensure();if(Input.GetKeyDown(KeyCode.Tab))Handle(KeyCode.Tab,Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift));if(Input.GetKeyDown(KeyCode.Escape))Handle(KeyCode.Escape);}
        protected virtual void LateUpdate(){Ensure();if(Top!=this||!EventSystem.current)return;var selected=EventSystem.current.currentSelectedGameObject;if(selected!=last&&(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.RightArrow))&&selected)Reveal(selected.transform);last=selected;}
    }
}
