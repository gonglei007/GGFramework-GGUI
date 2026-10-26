using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGFramework.GGUI;

public class Demo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 切换 UI 实现：默认使用 NGUI，无需任何调用。
        // 如需切换到 UGUI，请在首次访问 UISystem.Instance（触发其 Awake）之前注入实现：
        // UISystem.SetImplementation(new GGFramework.GGUI.UGUI.UGUIImplementation());

        ResourceSystem.Instance.Init("Game");
        UISystem.Instance.TextAdaptor.onGetText = (string text) =>
        {
            return text;
        };

        UISystem.Instance.PopDialog(
            "title",
            "message",
            (int)UIDialog.Option.Yes,
            (UIDialog.Option obj) => {
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
