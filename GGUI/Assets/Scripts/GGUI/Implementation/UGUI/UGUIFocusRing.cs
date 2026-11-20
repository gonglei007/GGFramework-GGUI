using UnityEngine;
using UnityEngine.UI;

namespace GGFramework.GGUI.UGUI
{
    // A focus decoration has its own small mesh; it never duplicates a complex contour.
    public class UGUIFocusRing : MaskableGraphic
    {
        public float WidthPixels { get; set; } = 2;
        float previousScale=-1;
        float ScreenScale()
        {
            float relative=canvas?rectTransform.lossyScale.x/canvas.transform.lossyScale.x:1;
            return Mathf.Max(.001f,Mathf.Abs(relative)*(canvas?canvas.scaleFactor:1));
        }
        protected override void OnEnable(){base.OnEnable();Canvas.preWillRenderCanvases+=RefreshWidth;}
        protected override void OnDisable(){Canvas.preWillRenderCanvases-=RefreshWidth;base.OnDisable();}
        void RefreshWidth(){if(!Mathf.Approximately(previousScale,ScreenScale()))SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();previousScale=ScreenScale();var r=rectTransform.rect;
            float line=Mathf.Min(WidthPixels/previousScale,Mathf.Min(r.width,r.height)*.5f);
            Quad(mesh,new Rect(r.xMin,r.yMin,r.width,line));Quad(mesh,new Rect(r.xMin,r.yMax-line,r.width,line));
            Quad(mesh,new Rect(r.xMin,r.yMin+line,line,Mathf.Max(0,r.height-line*2)));Quad(mesh,new Rect(r.xMax-line,r.yMin+line,line,Mathf.Max(0,r.height-line*2)));
        }
        void Quad(VertexHelper mesh,Rect r)
        {
            int start=mesh.currentVertCount;Color32 tint=color;
            mesh.AddVert(new Vector3(r.xMin,r.yMin),tint,Vector2.zero);mesh.AddVert(new Vector3(r.xMin,r.yMax),tint,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax,r.yMax),tint,Vector2.zero);mesh.AddVert(new Vector3(r.xMax,r.yMin),tint,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
    }
}
