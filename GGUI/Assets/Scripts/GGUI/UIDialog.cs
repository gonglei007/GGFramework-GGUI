using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using LuaInterface;

namespace GGFramework.GGUI
{
    /**
     * 对话框类。通过 UI 实现层（IUIImplementation）与具体引擎解耦。
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

        // 按钮文本：由具体控件改为 GameObject 引用（可序列化、引擎无关），Awake 时经实现层包装。
        public GameObject yesButtonLabel = null;
        public GameObject noButtonLabel = null;
        private IUIText titleLabel = null;
        private IUIText messageLabel = null;
        private IUIText yesButtonLabelText = null;
        private IUIText noButtonLabelText = null;
        private IUIButton yesButton = null;
        private IUIButton noButton = null;
        private IUIButton cancelButton = null;
        private IUILayoutGroup buttonGrid = null;
        private GameObject mask;

        private float initCellWidth = 0.0f;
        private string initYesButtonText = "Yes";
        private string initNoButtonText = "No";

        public Action<Option> onCallback = null;
        //public IFunction onCallback = null;

        protected override void Awake()
        {
            base.Awake();
            IUIImplementation impl = UISystem.Instance.Implementation;
            titleLabel = impl.FindText(this.gameObject, "Content/ContainerTitle/Label");
            cancelButton = impl.FindButton(this.gameObject, "Content/ContainerTitle/ButtonCancel");
            messageLabel = impl.FindText(this.gameObject, "Content/ContainerContent/Label");
            yesButton = impl.FindButton(this.gameObject, "Content/ContainerButtons/ButtonYes");
            noButton = impl.FindButton(this.gameObject, "Content/ContainerButtons/ButtonNo");
            buttonGrid = impl.FindLayoutGroup(this.gameObject, "Content/ContainerButtons");
            mask = impl.FindGameObject(this.gameObject, "Mask");

            yesButtonLabelText = impl.WrapText(yesButtonLabel);
            noButtonLabelText = impl.WrapText(noButtonLabel);

            if (yesButton != null)
            {
                yesButton.AddClickListener(OnButtonClick);
            }
            if (noButton != null)
            {
                noButton.AddClickListener(OnButtonClick);
            }
            if (cancelButton != null)
            {
                cancelButton.AddClickListener(OnButtonClick);
            }
            if (mask != null)
            {
                impl.AddClickHandler(mask, OnButtonClick);
            }

            if (buttonGrid != null)
            {
                initCellWidth = buttonGrid.cellWidth;
            }
            if (yesButtonLabelText != null)
            {
                initYesButtonText = yesButtonLabelText.text;
            }
            if (noButtonLabelText != null)
            {
                initNoButtonText = noButtonLabelText.text;
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
        public void Show(string title, string message, int options = (int)Option.None, Action<Option> onCallback = null)
        {
            this.options = options;
            this.onCallback = onCallback;
            //this.onLuaCallback = null;
            UpdateVisible();
            if (this.titleLabel != null)
            {
                this.titleLabel.text = title;
            }
            if (this.messageLabel != null)
            {
                this.messageLabel.text = message;
            }
            base.Show();
        }

        public void ShowX(string title, string message, int options = (int)Option.None, IFunction onCallback = null)
        {
            this.Show(title, message, options, (Option option) => {
            });
            this.onCallback = null;
            //this.onLuaCallback = onLuaCallback;
        }

        public void SetButtonText(string yesButtonText, string noButtonText)
        {
            if (yesButtonLabelText != null)
            {
                yesButtonLabelText.text = yesButtonText;
            }
            if (noButtonLabelText != null)
            {
                noButtonLabelText.text = noButtonText;
            }
        }

        void OnButtonClick(GameObject sender)
        {
            Option option = Option.None;
            if (yesButton != null && sender == yesButton.gameObject)
            {
                option = Option.Yes;
            }
            if ((noButton != null && sender == noButton.gameObject) || sender == mask)
            {
                option = Option.No;
            }
            if (cancelButton != null && sender == cancelButton.gameObject)
            {
                option = Option.Cancel;
            }

            if (this.onCallback != null)
            {
                this.onCallback(option);
            }
            //if (this.onLuaCallback != null)
            //{
            //    this.onLuaCallback.Call(option);
            //}
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
            if (yesButton != null)
            {
                yesButton.SetActive(IsOptionOn(Option.Yes));
            }
            if (noButton != null)
            {
                noButton.SetActive(IsOptionOn(Option.No));
            }
            if (cancelButton != null)
            {
                cancelButton.SetActive(IsOptionOn(Option.Cancel));
            }
            SetButtonText(initYesButtonText, initNoButtonText);
            UpdateLayout();
        }

        void UpdateLayout()
        {
            int buttonNumber = 0;
            if (IsOptionOn(Option.Yes))
            {
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
            else
            {
                if (buttonGrid != null)
                {
                    buttonGrid.cellWidth = 0.0f;
                }
            }
            if (buttonGrid != null)
            {
                buttonGrid.Reposition();
            }
        }
    }
}

