using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGFramework.GGUI
{
    public class ComponentUtil
    {
        /// <summary>
        /// 从指定的对象复制Component，挂到当前的对象中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originals"></param>
        /// <returns></returns>
        public static void CopyComponent<T>(T[] originals, GameObject targetObject) where T : Component
        {
            foreach (T o in originals)
            {
                System.Type type = o.GetType();
                Component copy = targetObject.AddComponent(type);
                System.Reflection.FieldInfo[] fields = type.GetFields();
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    field.SetValue(copy, field.GetValue(o));
                }
            }
        }

        /**
         * 查找并绑定控件。
         */
        public static void BindComponent<T>(ref T item, string name, GameObject targetObject)
        {
            if (targetObject == null)
            {
                Debug.LogWarning("targetObject不能为null!");
                return;
            }
            Transform transform = targetObject.transform.Find(name);
            if (transform == null || transform.gameObject == null)
            {
                Debug.LogWarning("无法找到所需的控件!-" + name);
                return;
            }

            item = transform.gameObject.GetComponent<T>();
            if (item == null)
            {
                Debug.LogError(name + "控件沒有綁定脚本！-" + typeof(T).FullName);
            }
        }
    }
}
