using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGFramework.GGUI.UGUI
{
    /// <summary>
    /// 基于 UGUI 的 UI 实现层（占位骨架）。
    /// 已实现关键控件（Text/Button/Slider）的查找与包装；补间、布局、遮罩点击等留 TODO，
    /// 便于后续按需填充。层级创建以 Canvas + sortingOrder 给出基础实现。
    /// </summary>
    public class UGUIImplementation : IUIImplementation
    {
        private GameObject uiRoot;

        // 记录任意 GameObject 上通过 Button 注册的点击回调包装，保证可正确移除。
        private readonly Dictionary<GameObject, Dictionary<Action<GameObject>, UnityAction>> clickHandlers =
            new Dictionary<GameObject, Dictionary<Action<GameObject>, UnityAction>>();

        public string Name
        {
            get { return "UGUI"; }
        }

        public void Init(GameObject uiRoot, GameObject animationRoot)
        {
            this.uiRoot = uiRoot;
            // TODO: 初始化 UGUI 运行环境（EventSystem、根 Canvas、CanvasScaler 等）。
            Debug.LogWarning("[UGUI] Init 为占位实现，EventSystem/根 Canvas 需后续完善。");
        }

        public GameObject CreateLayerGroup(string layerName, int depth, Transform parent)
        {
            GameObject groupObject = new GameObject(layerName, typeof(Canvas), typeof(GraphicRaycaster));
            RectTransform rect = groupObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Canvas canvas = groupObject.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = depth;
            // TODO: 按需完善层叠/射线过滤等设置。
            return groupObject;
        }

        public IUITweener CreateTweener(GameObject target, string animName)
        {
            // TODO: 接入 Animator 或 DOTween。
            Debug.LogWarning("[UGUI] CreateTweener 尚未实现，返回空的补间占位对象。");
            return new UGUITweener();
        }

        public IUIText FindText(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            Text text = go.GetComponent<Text>();
            if (text == null)
            {
                Debug.LogError("控件未绑定 Text！-" + path);
                return null;
            }
            return new UGUIText(text);
        }

        public IUIButton FindButton(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            Button button = go.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("控件未绑定 Button！-" + path);
                return null;
            }
            return new UGUIButton(button);
        }

        public IUIProgressBar FindProgressBar(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            Slider slider = go.GetComponent<Slider>();
            if (slider == null)
            {
                Debug.LogError("控件未绑定 Slider！-" + path);
                return null;
            }
            return new UGUIProgressBar(slider);
        }

        public IUILayoutGroup FindLayoutGroup(GameObject root, string path)
        {
            GameObject go = FindChild(root, path);
            if (go == null)
            {
                return null;
            }
            // TODO: 依据 HorizontalLayoutGroup/GridLayoutGroup 完善布局能力。
            return new UGUILayoutGroup(go);
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
            Button button = target.GetComponent<Button>();
            if (button == null)
            {
                // TODO: 遮罩等非 Button 对象需接入 EventTrigger 才能响应点击。
                Debug.LogWarning("[UGUI] AddClickHandler: 目标无 Button 组件，遮罩点击需接入 EventTrigger（TODO）。-" + target.name);
                return;
            }
            Dictionary<Action<GameObject>, UnityAction> map;
            if (!clickHandlers.TryGetValue(target, out map))
            {
                map = new Dictionary<Action<GameObject>, UnityAction>();
                clickHandlers[target] = map;
            }
            GameObject sender = target;
            UnityAction wrapped = () => handler(sender);
            map[handler] = wrapped;
            button.onClick.AddListener(wrapped);
        }

        public void RemoveClickHandler(GameObject target, Action<GameObject> handler)
        {
            if (target == null || handler == null)
            {
                return;
            }
            Dictionary<Action<GameObject>, UnityAction> map;
            UnityAction wrapped;
            if (clickHandlers.TryGetValue(target, out map) && map.TryGetValue(handler, out wrapped))
            {
                Button button = target.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveListener(wrapped);
                }
                map.Remove(handler);
            }
        }

        public IUIText WrapText(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            Text text = go.GetComponent<Text>();
            return text != null ? new UGUIText(text) : null;
        }

        public IUIProgressBar WrapProgressBar(GameObject go)
        {
            if (go == null)
            {
                return null;
            }
            Slider slider = go.GetComponent<Slider>();
            return slider != null ? new UGUIProgressBar(slider) : null;
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
