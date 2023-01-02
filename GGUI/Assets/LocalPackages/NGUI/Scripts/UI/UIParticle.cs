using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystemRenderer))]
public class UIParticle : MonoBehaviour {

	private UIPanel panel;
	private ParticleSystemRenderer pRenderer;
	private Material dyMaterial;
	private UIWidget coverWidget;

	// Use this for initialization
	void Start () {


		//找到这个粒子系统最近的父节点的UIPanel
		panel = GetComponentInParent<UIPanel> ();
		//找到这个粒子系统的Renderer
		pRenderer = GetComponent<ParticleSystemRenderer> ();

		dyMaterial = new Material (Shader.Find ("Hidden/Unlit/Transparent Colored 1")) 
		{
			//提升其渲染队列至4000
			renderQueue = 4000,
			//使用原始material的texture
			mainTexture = pRenderer.sharedMaterial.mainTexture
		};

		pRenderer.material = dyMaterial;

		/* 为什么不采用这种做法？
		 * pRenderer.material.renderQueue = 4000;
		 * 这样的话，Unity会帮助我们自动创建动态材质
		 * 如此，该动态引用无法得到，很难判断
		 * Destroy时也不好操作
		 */ 


	}

	void OnWillRenderObject()
	{
		if (panel.hasClipping)
		{
			//裁剪区域
			Vector4 cr = panel.drawCallClipRange;
			//裁剪边儿的柔和度
			Vector2 soft = panel.clipSoftness;

			Vector2 sharpenss = new Vector2 (1000.0f, 1000.0f);

			if (soft.x > 0f)
				sharpenss.x = cr.z / soft.x;
			if (soft.y > 0f)
				sharpenss.y = cr.w / soft.y;
			
			//经过测试粒子系统产生的Mesh是不受UIPanel缩放比影响的
			//所以要将其缩放比记录下来
			float scale = panel.transform.lossyScale.x;
			//粒子系统的顶点坐标系相对于panel会有一定的偏移，所以要将其position记录下来
			Vector3 position = panel.transform.position;
			
			Debug.Assert (dyMaterial != null, "dyMaterial 创建失败！！！！！！");

			//坐标变化的顺序：缩放、旋转、平移，这里不考虑粒子系统的旋转
			dyMaterial.SetVector 
			(
				Shader.PropertyToID ("_ClipRange0"),
				new Vector4 (
							 -cr.x / cr.z - position.x/scale / cr.z,
					         -cr.y / cr.w - position.y/scale / cr.w,
							 1f / cr.z / scale,
							 1f / cr.w / scale
							)
			);

			dyMaterial.SetVector(Shader.PropertyToID("_ClipArgs0"),new Vector4(sharpenss.x,sharpenss.y,0,1));
		}
	}


	void OnDestroy()
	{
		DestroyImmediate (dyMaterial);
		dyMaterial = null;
	}
}
