using UnityEngine;
using UnityEngine.UI;

namespace GGFramework.GGUI.UGUI
{
    // Scale vertices in button space, leaving layout, clipping and raycast geometry intact.
    public class UGUIButtonVisual : BaseMeshEffect
    {
        UGUIButtonMotion owner;
        public void Bind(UGUIButtonMotion feedback) { owner = feedback; RefreshMesh(); }
        public void RefreshMesh() { if (graphic) graphic.SetVerticesDirty(); }

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || !owner || Mathf.Approximately(owner.VisualScale, 1)) return;
            var root = (RectTransform)owner.transform;
            var center = (Vector3)root.rect.center;
            var toRoot = root.worldToLocalMatrix * transform.localToWorldMatrix;
            var fromRoot = transform.worldToLocalMatrix * root.localToWorldMatrix;
            var vertex = new UIVertex();
            for (int i = 0; i < mesh.currentVertCount; i++)
            {
                mesh.PopulateUIVertex(ref vertex, i);
                var point = toRoot.MultiplyPoint3x4(vertex.position);
                vertex.position = fromRoot.MultiplyPoint3x4(center + (point - center) * owner.VisualScale);
                mesh.SetUIVertex(vertex, i);
            }
        }
    }
}
