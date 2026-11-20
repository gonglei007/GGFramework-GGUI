using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace GGFramework.GGUI
{
    public class UIBase : MonoBehaviour
    {
        private new string name = null;
        private string path = null;
        private UIBase mParent = null;
        protected string showAnimationName = "showAnimClip";
        protected string hideAnimationName = "hideAnimClip";
        protected string uifx = null;
        protected string hideuifx = null;

        public GameObject tweenObject = null;
        public string tweenAnimationName = null;
        private IUITweener tweener = null;
        IUIImplementation implementation;
        UIHost host;
        bool loaded;
        int visibilityRevision;
        public void BindImplementation(IUIImplementation value)
        {
            if(value==null)throw new ArgumentNullException(nameof(value));
            implementation=value;host=GetComponentInParent<UIHost>(true);
        }
        internal void LoadOnce(){if(loaded)return;loaded=true;OnLoad();}
        protected virtual void OnDisable(){visibilityRevision++;}
        protected virtual void OnDestroy(){visibilityRevision++;if(host)host.Forget(this);}

        protected virtual void Awake()
        {
            if (tweenObject != null)
            {
                // 通过实现层创建补间控制器，核心层不再直接依赖 NGUI 的 UIPlayTween/UITweener。
                tweener = (implementation ?? UIImplementationResolver.Resolve(this)).CreateTweener(tweenObject, tweenAnimationName);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public string Name
        {
            set
            {
                name = value;
            }
            get
            {
                return name;
            }
        }

        // 资源路径
        public string Path
        {
            set
            {
                path = value;
            }
            get
            {
                return path;
            }
        }

        public UIBase Parent
        {
            get
            {
                return mParent;
            }
            set
            {
                mParent = value;
            }
        }

        public bool IsShowing()
        {
            return this.gameObject.activeSelf;
        }

        public virtual void Show()
        {
            Show(null);
        }

        public virtual void Show(string uifx)
        {
            this.uifx = uifx;
            int revision=++visibilityRevision;
            //this.showAnimationName = animationName;
            OnPreShow(() => {
                if(!this || revision!=visibilityRevision)return;
                this.gameObject.SetActive(true);
                if(revision!=visibilityRevision || !isActiveAndEnabled)return;
                OnShowed(() => { });
            });
        }

        public virtual void Hide()
        {
            Hide(null);
        }

        public virtual void Hide(string hideuifx, Action onFinish = null)
        {
            this.hideuifx = hideuifx;
            int revision=++visibilityRevision;
            OnPreHide(() => {
                if(!this || revision!=visibilityRevision)return;
                this.gameObject.SetActive(false);
                if (onFinish != null)
                {
                    onFinish();
                }
            });
        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnPreShow(Action onFinished)
        {
            onFinished?.Invoke();
        }

        public virtual void OnShowed(Action onFinished)
        {
            if (tweener != null && tweenObject != null)
            {
                tweener.Play(true);
            }
            onFinished?.Invoke();
        }

        public virtual void OnPreHide(Action onFinished)
        {
            if (tweener != null && tweenObject != null)
            {
                tweener.Play(false);
            }
            onFinished?.Invoke();
        }
    }
}

