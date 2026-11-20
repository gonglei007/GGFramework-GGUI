using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>Instance-owned GGUI host for existing scene UI. Does not recreate canvases, move
    /// layers or load prefabs. The host owns mounted windows and their replacement/teardown.</summary>
    public sealed class UIHost : MonoBehaviour
    {
        public sealed class Lease : IDisposable
        {
            internal UIHost Owner;
            internal string Layer;
            internal bool Retired;
            public UIBase View { get; internal set; }
            public bool IsCurrent => Owner && Owner.IsCurrent(this);
            public void Dispose() { if (Owner) Owner.Close(this); }
        }
        readonly Dictionary<string,Transform> layers = new Dictionary<string,Transform>(StringComparer.Ordinal);
        readonly Dictionary<string,Lease> current = new Dictionary<string,Lease>(StringComparer.Ordinal);
        public IUIImplementation Implementation { get; private set; }
        public int PresentedCount { get; private set; }
        public int ClosedCount { get; private set; }
        public int ActiveCount => current.Values.Count(x=>x.View && x.View.gameObject.activeInHierarchy);
        bool ownsImplementation, disposing, closingAll;
        public void Initialize(IUIImplementation implementation, bool owns = false)
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            if (Implementation != null) throw new InvalidOperationException("UI host is already initialized.");
            Implementation=implementation;ownsImplementation=owns;
            implementation.Init(gameObject,null);
        }
        public void RegisterLayer(string id, Transform layer)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Layer id required.",nameof(id));
            if (!layer || !layer.IsChildOf(transform)) throw new ArgumentException("Layer must belong to this UI host.",nameof(layer));
            if(layers.TryGetValue(id,out var old) && old!=layer)throw new InvalidOperationException("Layer already registered: "+id);
            layers[id]=layer;
        }
        public Lease Present(string layer, GameObject window)
        {
            if (disposing || closingAll || Implementation==null)throw new InvalidOperationException("UI host is unavailable.");
            if (!layers.TryGetValue(layer,out var parent) || !parent)throw new ArgumentException("Unknown UI layer: "+layer);
            if (!window || window.transform.parent!=parent)throw new ArgumentException("Window must already be a direct child of its registered layer.");
            if(current.TryGetValue(layer,out var previous) && previous.View && previous.View.gameObject==window)return previous;
            if(current.Values.Any(x=>x.View && x.View.gameObject==window))throw new InvalidOperationException("Window is already mounted.");
            var view=window.GetComponent<UIBase>() ?? window.AddComponent<UIWindow>();
            view.BindImplementation(Implementation);
            view.Name=window.name;
            var lease=new Lease{Owner=this,Layer=layer,View=view};
            current[layer]=lease;PresentedCount++;
            try{if(previous!=null)Retire(previous);if(IsCurrent(lease)){view.LoadOnce();if(IsCurrent(lease))view.Show();}}
            catch{if(IsCurrent(lease))Close(lease);throw;}
            return lease;
        }
        public bool IsCurrent(Lease lease) => lease!=null && lease.Owner==this && lease.View &&
            current.TryGetValue(lease.Layer,out var value) && ReferenceEquals(value,lease);
        public bool Close(Lease lease)
        {
            if(!IsCurrent(lease))return false;
            current.Remove(lease.Layer);Retire(lease);return true;
        }
        void Retire(Lease lease)
        {
            if(lease.Retired)return;
            lease.Retired=true;ClosedCount++;
            var view=lease.View;
            try { view.Hide(); }
            finally { if(view){view.gameObject.SetActive(false);Destroy(view.gameObject);} }
        }
        public void CloseLayer(string layer)
        {
            if(current.TryGetValue(layer,out var value))
            {
                if(!Close(value))current.Remove(layer);
            }
        }
        public void CloseAll()
        {
            if(closingAll)return;
            closingAll=true;List<Exception> errors=null;
            try
            {
                foreach(var lease in current.Values.ToArray())
                    try{Close(lease);}catch(Exception error){(errors??(errors=new List<Exception>())).Add(error);}
            }
            finally{current.Clear();closingAll=false;}
            if(errors!=null)throw new AggregateException("UI teardown failed after releasing all windows.",errors);
        }
        internal void Forget(UIBase view)
        {
            foreach(var item in current.Where(x=>ReferenceEquals(x.Value.View,view)).ToArray())
            {current.Remove(item.Key);ClosedCount++;}
        }
        void OnDestroy()
        {
            disposing=true;
            try{CloseAll();}
            finally{if(ownsImplementation)(Implementation as IDisposable)?.Dispose();}
        }
    }
}
