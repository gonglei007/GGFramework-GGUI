using UnityEngine;
using System.Collections;

public class UIGraySprite : UISprite
{
	public bool useGray;

	public Shader originShader;

	public Shader currentShader;

	protected override void OnStart()
	{
		originShader = (material != null) ? material.shader : null;
		currentShader = originShader;
		useGray = true;
	}

	public override Shader shader {
		get {
			return currentShader;
		}
		set {
			throw new System.NotImplementedException (GetType () + " has no shader setter");
		}
	}

	public void UseGray (bool use)
	{
		useGray = use;
		if (panel != null) {
			panel.RebuildAllDrawCalls ();
			if (use == true) {
				currentShader = Shader.Find ("Unlit/Transparent Colored Gray");
			}
			else {
				currentShader = originShader;
			}
		}
	}

	#region For Editor
	#if UNITY_EDITOR
			protected override void OnValidate()
	{
		UseGray(useGray);
		
		base.OnValidate();
	}
	#endif
	public override void MarkAsChanged ()
	{
		base.MarkAsChanged ();
		if (NGUITools.GetActive (this)) {
			if (originShader == null) {
				originShader = (material != null) ? material.shader : null;
				currentShader = originShader;
			}
		}
	}
#endregion
}
