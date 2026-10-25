using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGFramework.GGUI
{
    /// <summary>
    /// 等待UI
    /// </summary>
    public class UIWaiting : UIBase
    {
        public GameObject view = null;
        public float delay = 2.0f;

        private float timer = 0.0f;
        private bool showing = false;

        public override void Show()
        {
            this.gameObject.SetActive(true);
            view.SetActive(false);
            showing = true;
        }

        public override void Hide()
        {
            showing = false;
            timer = 0.0f;
            this.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (showing)
            {
                timer += Time.deltaTime;
                if (timer >= delay)
                {
                    view.SetActive(true);
                    showing = false;
                    timer = 0;
                }
            }
        }
    }
}
