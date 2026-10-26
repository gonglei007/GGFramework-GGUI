using UnityEngine;
using System.Collections;

namespace GGFramework.GGUI.NGUI
{
	public class UILabelLocalization : MonoBehaviour
	{
		public static bool loaded = false;

		public string localizeKey;

		public string keyParam;

		public string textPrefix;

		public string textSuffix;

		// Use this for initialization
		public void Start()
		{
			try
			{
				//if (UISystem.Instance != null && UISystem.Instance.IsLocalLizationInited) {
				BindText();
				//}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("UILabelLocalization error : " + e);
			}
		}

		public void BindText()
		{
			this.GetComponent<UILabel>().text = string.Format("{0}{1}{2}", textPrefix, UISystem.Instance.TextAdaptor.GetText(string.Format("{0}{1}", localizeKey, keyParam)), textSuffix);
		}

#if UNITY_EDITOR
		[ContextMenu("Show Key")]
		public void ShowKey()
		{
			Debug.LogWarning(localizeKey);
		}
#endif
		public void Awake()
		{
			if (!loaded)
			{
				//TextSystem.LoadLanguage("Localization/zh-Hans/loc-kit");
				loaded = true;
			}
			//this.GetComponent<UILabel>().text = TextSystem.GetText(localizeKey);
		}

		public static void OnInit()
		{
			if (!loaded)
			{
				//TextSystem.LoadLanguage("Localization/zh-Hans/loc-kit");
				loaded = true;
			}
		}

		static public void Init()
		{
			//TextSystem.OnFirstUsed += OnInit;
		}
	}
}
