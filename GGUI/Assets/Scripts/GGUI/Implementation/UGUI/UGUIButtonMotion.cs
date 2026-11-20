using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGFramework.GGUI.UGUI
{
    public class UGUIButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        readonly List<UGUIButtonVisual> visuals = new List<UGUIButtonVisual>();
        Button button;
        bool hovered, pressed, focused;
        float scale = 1, pressUntil;
        protected virtual bool MotionEnabled => true;
        public float PressedScale { get; set; } = .965f;
        public float HighlightedScale { get; set; } = 1.018f;
        public float PressHoldSeconds { get; set; } = .065f;
        public float Response { get; set; } = 28f;
        protected virtual UGUIButtonVisual AddVisual(GameObject target) => target.AddComponent<UGUIButtonVisual>();
        public float VisualScale => scale;

        public void Refresh(Button target)
        {
            button = target;
            visuals.Clear();
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                if (!(graphic is Image) && !(graphic is Text)) continue;
                if (graphic.GetComponentInParent<Button>() != button) continue;
                var visual = graphic.GetComponent<UGUIButtonVisual>() ?? AddVisual(graphic.gameObject);
                visual.Bind(this);
                visuals.Add(visual);
            }
        }

        protected virtual void Update()
        {
            bool interactive = button && button.IsActive() && button.IsInteractable();
            float target = !MotionEnabled || !interactive ? 1 : pressed || Time.unscaledTime < pressUntil ? PressedScale : hovered || focused ? HighlightedScale : 1;
            float next = MotionEnabled ? Mathf.Lerp(scale, target, 1 - Mathf.Exp(-Time.unscaledDeltaTime * Response)) : target;
            if (Mathf.Abs(next - target) < .0002f) next = target;
            if (Mathf.Abs(next - scale) < .00001f) return;
            scale = next;
            foreach (var visual in visuals) if (visual) visual.RefreshMesh();
        }

        protected virtual void OnDisable()
        {
            hovered = pressed = focused = false;
            scale = 1; pressUntil = 0;
            foreach (var visual in visuals) if (visual) visual.RefreshMesh();
        }

        public void OnPointerEnter(PointerEventData e) { hovered = true; }
        public void OnPointerExit(PointerEventData e) { hovered = false; pressed = false; }
        public void OnPointerDown(PointerEventData e) { if (e.button == PointerEventData.InputButton.Left) { pressed = true; pressUntil = Time.unscaledTime + PressHoldSeconds; } }
        public void OnPointerUp(PointerEventData e) { pressed = false; }
        public void OnSelect(BaseEventData e) { focused = true; }
        public void OnDeselect(BaseEventData e) { focused = pressed = false; }
    }
}
