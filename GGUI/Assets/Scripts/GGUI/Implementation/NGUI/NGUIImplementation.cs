using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGFramework.GGUI.NGUI
{
    /// <summary>
    /// 基于 NGUI 的 UI 实现层。精确包装原有核心逻辑（UIPanel 分层、UIPlayTween 补间、
    /// UILabel/UIButton/UIGrid/UIProgressBar 控件查找与包装）。
    /// </summary>
    public class NGUIImplementation : IUIImplementation
    {
        private readonly NGUITweenerFactory tweenerFactory = new NGUITweenerFactory();
        private GameObject uiRoot;

        // 记录任意 GameObject 上的点击回调包装，保证可正确移除。
        private readonly Dictionary<GameObject, Dictionary<Action<GameObject>, UIEventListener.VoidDelegate>> clickHandlers =
            new Dictionary<GameObject, Dictionary<Action<GameObject>, UIEventListener.VoidDelegate>>();

        public string Name
        {
            get { return "NGUI"; }
        }

        public void Init(GameObject uiRoot, GameObject animationRoot)
        {
            this.uiRoot = uiRoot;
            tweenerFactory.Init(animationRoot);
        }

        public GameObject CreateLayerGroup(string layerName, int depth, Transform parent)
        {
            GameObject groupObject = new GameObject(layerName);
            groupObject.layer = LayerMask.NameToLayer("UI");
            groupObject.transform.parent = parent;
            groupObject.transform.localPosition = Vector3.zero;
            groupObject.transform.localRotation = Quaternion.identity;
            groupObject.transform.localScale = Vector3.one;
            UIPanel panel = groupObject.AddComponent<UIPanel>();
            panel.useSortingOrder = true;
            panel.depth = depth;
            panel.sortingOrder = depth;
            return groupObject;
        }

        public IUITweener CreateTweener(GameObject target, string animName)
        {
            if (target == null)
            {
                return null;
            }
            UIPlayTween playTween = target.AddComponent<UIPlayTween>();
            playTween.tweenTarget = target;
            playTween.resetOnPlay = true;
            playTween.resetIfDisabled = true;
            if (!string.IsNullOrEmpty(animName))
            {
                UITweener[] tweeners = tweenerFactory.GetUITweeners(animName);
                if (tweeners != null)
                {
                    ComponentUtil.CopyComponent<UITweener>(tweeners, target);
                }
            }
            return new NGUITweener(playTween);
        }

        public IUIText FindText(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            UILabel label = go.GetComponent<UILabel>();
            if (label == null)
            {
                Debug.LogError("控件未绑定 UILabel！-" + path);
                return null;
            }
            return new NGUIText(label);
        }

        public IUIButton FindButton(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            UIButton button = go.GetComponent<UIButton>();
            if (button == null)
            {
                Debug.LogError("控件未绑定 UIButton！-" + path);
                return null;
            }
            return new NGUIButton(button);
        }

        public IUIProgressBar FindProgressBar(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            UIProgressBar bar = go.GetComponent<UIProgressBar>();
            if (bar == null)
            {
                Debug.LogError("控件未绑定 UIProgressBar！-" + path);
                return null;
            }
            return new NGUIProgressBar(bar);
        }

        public IUILayoutGroup FindLayoutGroup(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            UIGrid grid = go.GetComponent<UIGrid>();
            if (grid == null)
            {
                Debug.LogError("控件未绑定 UIGrid！-" + path);
                return null;
            }
            return new NGUIGrid(grid);
        }

        public GameObject FindGameObject(GameObject root, string path)
        {
            return FindChild(root, path);
        }

        public void AddClickHandler(GameObject target, Action<GameObject> handler)
        {
            if (target == null || handler == null)
            {
                return;
            }
            Dictionary<Action<GameObject>, UIEventListener.VoidDelegate> map;
            if (!clickHandlers.TryGetValue(target, out map))
            {
                map = new Dictionary<Action<GameObject>, UIEventListener.VoidDelegate>();
                clickHandlers[target] = map;
            }
            UIEventListener.VoidDelegate wrapped = new UIEventListener.VoidDelegate(handler);
            map[handler] = wrapped;
            UIEventListener.Get(target).onClick += wrapped;
        }

        public void RemoveClickHandler(GameObject target, Action<GameObject> handler)
        {
            if (target == null || handler == null)
            {
                return;
            }
            Dictionary<Action<GameObject>, UIEventListener.VoidDelegate> map;
            UIEventListener.VoidDelegate wrapped;
            if (clickHandlers.TryGetValue(target, out map) && map.TryGetValue(handler, out wrapped))
            {
                UIEventListener.Get(target).onClick -= wrapped;
                map.Remove(handler);
            }
        }

        public IUIText WrapText(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            UILabel label = go.GetComponent<UILabel>();
            return label != null ? new NGUIText(label) : null;
        }

        public IUIProgressBar WrapProgressBar(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            UIProgressBar bar = go.GetComponent<UIProgressBar>();
            return bar != null ? new NGUIProgressBar(bar) : null;
        }

        private GameObject FindChild(GameObject root, string path)
        {
            if (root == null)
            {
                Debug.LogWarning("root 不能为 null!");
                return null;
            }
            Transform transform = root.transform.Find(path);
            if (transform == null || transform.gameObject == null)
            {
                Debug.LogWarning("无法找到所需的控件!-" + path);
                return null;
            }
            return transform.gameObject;
        }
    }
}
