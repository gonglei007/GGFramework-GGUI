using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace GGFramework.GGUI.UGUI
{
    public class UGUIHoldGesture:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler,IBeginDragHandler,IEndDragHandler,ICancelHandler,IUpdateSelectedHandler,IDeselectHandler
    {
        public Action tap,begin,cancel;public Action<bool> release,arm;public Func<double> clock;public Func<float> pixelsToUnits;public Func<bool> ignoreFocusLoss;
        public double HoldSeconds { get; set; } = .55;
        public float MoveLimit { get; set; } = 14;
        public KeyCode HoldKey { get; set; } = KeyCode.Space;
        public Func<Vector2,bool> ArmWhen { get; set; }
        bool pointer,keyboard,longPress,ignoreClick,moved,armed;int pointerId,consumeFrame=-1;double started;Vector2 origin;
        double Now=>clock?.Invoke()??Time.realtimeSinceStartupAsDouble;public bool Held=>pointer||keyboard;public bool LongPress=>longPress;
        public bool ConsumeClick(){bool value=ignoreClick||keyboard||consumeFrame==Time.frameCount;ignoreClick=false;return value;}
        public void OnPointerDown(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||Held||e.pointerId>=0&&Input.touchCount>0&&e.pointerId!=Input.GetTouch(0).fingerId)return;
            pointer=true;pointerId=e.pointerId;origin=e.position;started=Now;longPress=ignoreClick=moved=armed=false;
        }
        public void OnDrag(PointerEventData e)
        {
            if(!pointer||e.pointerId!=pointerId)return;Vector2 delta=(e.position-origin)*(pixelsToUnits?.Invoke()??1);
            if(!longPress){if(delta.magnitude>MoveLimit){moved=true;ignoreClick=true;}return;}armed=ArmWhen?.Invoke(delta)==true;arm?.Invoke(armed);
        }
        public void OnPointerUp(PointerEventData e){if(!pointer||e.pointerId!=pointerId)return;pointer=false;if(longPress){ignoreClick=true;release?.Invoke(armed);}longPress=false;}
        public void Tick(){if(Held&&!longPress&&!moved&&Now-started>=HoldSeconds){longPress=true;ignoreClick=true;begin?.Invoke();}}
        public void KeyDown(){if(Held)return;keyboard=true;longPress=ignoreClick=moved=armed=false;started=Now;}
        public void KeyUp(){if(!keyboard)return;keyboard=false;consumeFrame=Time.frameCount;if(longPress)release?.Invoke(false);else tap?.Invoke();longPress=false;ignoreClick=false;}
        public void CancelHold(){bool active=Held;pointer=keyboard=false;longPress=false;if(active){ignoreClick=true;cancel?.Invoke();}}
        protected virtual void Update()=>Tick();
        public void OnUpdateSelected(BaseEventData data){if(Input.GetKeyDown(HoldKey)){KeyDown();data.Use();}if(Input.GetKeyUp(HoldKey)){KeyUp();data.Use();}}
        public void OnDeselect(BaseEventData data){if(keyboard)CancelHold();}
        public void OnCancel(BaseEventData data){CancelHold();data.Use();}
        public void OnBeginDrag(PointerEventData data){}public void OnEndDrag(PointerEventData data){}
        bool ignoredFocusLoss;
        protected virtual void OnDisable()=>CancelHold();protected virtual void OnApplicationPause(bool paused){if(paused)CancelHold();}
        protected virtual void OnApplicationFocus(bool focused){if(!focused){ignoredFocusLoss=ignoreFocusLoss?.Invoke()==true;if(!ignoredFocusLoss)CancelHold();}else if(ignoredFocusLoss){ignoredFocusLoss=false;if(Held&&Input.touchCount==0&&!Input.GetMouseButton(0)&&!Input.GetKey(HoldKey))CancelHold();}}
    }
}
