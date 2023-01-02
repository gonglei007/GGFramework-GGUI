using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGFramework.GGUI;

public class Demo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
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
