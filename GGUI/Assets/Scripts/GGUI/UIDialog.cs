using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

/**
 * 基于NGUI的对话框类。
 * 对话框上的部件要符合命名规则。
 */
public class UIDialog : UIBase
{
    public enum Option
    {
        None = 0x0000,
        Yes = 0x0001,
        No = 0x0002,
        Cancel = 0x0004,
    };
    int options = (int)Option.None;

    public UILabel yesButtonLabel = null;
    public UILabel noButtonLabel = null;
    private UILabel titleLabel = null;
    private UILabel messageLabel = null;
    private UIButton yesButton = null;
    private UIButton noButton = null;
    private UIButton cancelButton = null;
    private UIGrid buttonGrid = null;
    private GameObject mask;

    private float initCellWidth = 0.0f;
    private string initYesButtonText = "Yes";
    private string initNoButtonText = "No";

    public Action<Option> onCallback = null;
    public LuaFunction onLuaCallback = null;

    protected override void Awake()
    {
        base.Awake();
        ComponentUtil.BindComponent(ref titleLabel, "Content/ContainerTitle/Label", this.gameObject);
        ComponentUtil.BindComponent(ref cancelButton, "Content/ContainerTitle/ButtonCancel", this.gameObject);
        ComponentUtil.BindComponent(ref messageLabel, "Content/ContainerContent/Label", this.gameObject);
        ComponentUtil.BindComponent(ref yesButton, "Content/ContainerButtons/ButtonYes", this.gameObject);
        ComponentUtil.BindComponent(ref noButton, "Content/ContainerButtons/ButtonNo", this.gameObject);
        ComponentUtil.BindComponent(ref buttonGrid, "Content/ContainerButtons", this.gameObject);
        ComponentUtil.BindComponent(ref mask, "Mask", this.gameObject);

        if (yesButton != null)
        {
            UIEventListener.Get(yesButton.gameObject).onClick += OnButtonClick;
        }
        if (noButton != null)
        {
            UIEventListener.Get(noButton.gameObject).onClick += OnButtonClick;
        }
        if (cancelButton != null)
        {
            UIEventListener.Get(cancelButton.gameObject).onClick += OnButtonClick;
        }
        if (mask != null)
        {
            UIEventListener.Get(mask).onClick += OnButtonClick;
            //EventDelegate.Add(mask.GetComponent<UIEventTrigger>().onClick, OnMaskClick);
        }

        if (buttonGrid != null)
        {
            initCellWidth = buttonGrid.cellWidth;
        }
        if (yesButtonLabel != null)
        {
            initYesButtonText = yesButtonLabel.text;
        }
        if (noButtonLabel != null)
        {
            initNoButtonText = noButtonLabel.text;
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

    /**
     * 显示对话框。
     * @param options: option的逻辑与，确定显示哪些按钮。
     */
    public void Show(string title, string message, int options = (int)Option.None, Action<Option> onCallback=null)
    {
        this.options = options;
        this.onCallback = onCallback;
        this.onLuaCallback = null;
        UpdateVisible();
        this.titleLabel.text = title;
        this.messageLabel.text = message;
        base.Show();
    }

    public void ShowX(string title, string message, int options = (int)Option.None, LuaFunction onLuaCallback = null)
    {
        this.Show(title, message, options, (Option option)=> {
        });
        this.onCallback = null;
        this.onLuaCallback = onLuaCallback;
    }

    public void SetButtonText(string yesButtonText, string noButtonText) {
        if (yesButtonLabel != null) {
            yesButtonLabel.text = yesButtonText;
        }
        if (noButtonLabel != null)
        {
            noButtonLabel.text = noButtonText;
        }
    }

    void OnButtonClick(GameObject sender)
    {
        Option option = Option.None;
        if (sender == yesButton.gameObject)
        {
            option = Option.Yes;
        }
        if (sender == noButton.gameObject || sender == mask)
        {
            option = Option.No;
        }
        if (sender == cancelButton.gameObject)
        {
            option = Option.Cancel;
        }

        if (this.onCallback != null) {
            this.onCallback(option);
        }
        if (this.onLuaCallback != null)
        {
            this.onLuaCallback.Call(option);
        }
        this.Hide();
    }

    bool IsOptionOn(Option option)
    {
        if ((this.options & (int)option) != 0)
        {
            return true;
        }
        return false;
    }

    void UpdateVisible()
    {
        yesButton.gameObject.SetActive(IsOptionOn(Option.Yes));
        noButton.gameObject.SetActive(IsOptionOn(Option.No));
        cancelButton.gameObject.SetActive(IsOptionOn(Option.Cancel));
        SetButtonText(initYesButtonText, initNoButtonText);
        UpdateLayout();
    }

    void UpdateLayout() {
        int buttonNumber = 0;
        if (IsOptionOn(Option.Yes)) {
            buttonNumber++;
        }
        if (IsOptionOn(Option.No))
        {
            buttonNumber++;
        }
        if (buttonNumber >= 2)
        {
            if (buttonGrid != null)
            {
                buttonGrid.cellWidth = initCellWidth;
            }
        }
        else {
            if (buttonGrid != null)
            {
                buttonGrid.cellWidth = 0.0f;
            }
        }
        if (buttonGrid != null) {
            buttonGrid.repositionNow = true;
        }
    }
}
