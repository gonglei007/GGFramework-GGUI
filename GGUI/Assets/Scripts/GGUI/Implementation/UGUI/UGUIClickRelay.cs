using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGFramework.GGUI.UGUI
{
    public sealed class UGUIClickRelay : MonoBehaviour, IPointerClickHandler
    {
        readonly HashSet<Action<GameObject>> handlers=new HashSet<Action<GameObject>>();
        public void Add(Action<GameObject> handler){handlers.Add(handler);}
        public void Remove(Action<GameObject> handler){handlers.Remove(handler);}
        public void OnPointerClick(PointerEventData data)
        {
            if(data.button!=PointerEventData.InputButton.Left)return;
            foreach(var handler in handlers.ToArray())if(handlers.Contains(handler))handler(gameObject);
        }
    }
}
