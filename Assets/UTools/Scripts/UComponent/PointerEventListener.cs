using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UTools
{
    public enum PointerEventType
    {
        Click,
        Enter,
        Exit,
        Down,
        Up,
        BeginDrag,
        Drag,
        EndDrag
    }

    public readonly struct PointerEventMessage
    {
        public PointerEventMessage(PointerEventType eventType, GameObject target, PointerEventData eventData, bool is3DObject, Vector2 delta)
        {
            EventType = eventType;
            Target = target;
            EventData = eventData;
            Is3DObject = is3DObject;
            Delta = delta;
        }

        public PointerEventType EventType { get; }
        public GameObject Target { get; }
        public PointerEventData EventData { get; }
        public bool Is3DObject { get; }
        public Vector2 Delta { get; }
    }

    [Serializable]
    public sealed class PointerEventDataEvent : UnityEvent<PointerEventData>
    {
    }

    [Serializable]
    public sealed class PointerDragEvent : UnityEvent<PointerEventData, Vector2>
    {
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("UTools/Pointer Event Listener")]
    public sealed class PointerEventListener : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Global Publish")]
        public bool publishGlobally = true;

        [Header("Runtime Callbacks")]
        public UnityAction onClick;
        public UnityAction onDown;
        public UnityAction onEnter;
        public UnityAction onExit;
        public UnityAction onUp;
        public UnityAction onBeginDrag;
        public UnityAction onDrag;
        public UnityAction onEndDrag;

        public UnityAction<PointerEventData> onClickWithData;
        public UnityAction<PointerEventData> onDownWithData;
        public UnityAction<PointerEventData> onEnterWithData;
        public UnityAction<PointerEventData> onExitWithData;
        public UnityAction<PointerEventData> onUpWithData;
        public UnityAction<PointerEventData> onBeginDragWithData;
        public UnityAction<PointerEventData, Vector2> onDragWithData;
        public UnityAction<PointerEventData> onEndDragWithData;

        [Header("Inspector Events")]
        public UnityEvent onClickEvent = new UnityEvent();
        public UnityEvent onDownEvent = new UnityEvent();
        public UnityEvent onEnterEvent = new UnityEvent();
        public UnityEvent onExitEvent = new UnityEvent();
        public UnityEvent onUpEvent = new UnityEvent();
        public UnityEvent onBeginDragEvent = new UnityEvent();
        public UnityEvent onDragEvent = new UnityEvent();
        public UnityEvent onEndDragEvent = new UnityEvent();

        public PointerEventDataEvent onClickEventWithData = new PointerEventDataEvent();
        public PointerEventDataEvent onDownEventWithData = new PointerEventDataEvent();
        public PointerEventDataEvent onEnterEventWithData = new PointerEventDataEvent();
        public PointerEventDataEvent onExitEventWithData = new PointerEventDataEvent();
        public PointerEventDataEvent onUpEventWithData = new PointerEventDataEvent();
        public PointerEventDataEvent onBeginDragEventWithData = new PointerEventDataEvent();
        public PointerDragEvent onDragEventWithData = new PointerDragEvent();
        public PointerEventDataEvent onEndDragEventWithData = new PointerEventDataEvent();

        public bool Is3DObject => !(transform is RectTransform);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.dragging)
            {
                return;
            }

            Dispatch(PointerEventType.Click, eventData, Vector2.zero, onClickWithData, onClick, onClickEventWithData, onClickEvent);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Dispatch(PointerEventType.Down, eventData, Vector2.zero, onDownWithData, onDown, onDownEventWithData, onDownEvent);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Dispatch(PointerEventType.Enter, eventData, Vector2.zero, onEnterWithData, onEnter, onEnterEventWithData, onEnterEvent);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Dispatch(PointerEventType.Exit, eventData, Vector2.zero, onExitWithData, onExit, onExitEventWithData, onExitEvent);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Dispatch(PointerEventType.Up, eventData, Vector2.zero, onUpWithData, onUp, onUpEventWithData, onUpEvent);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Dispatch(PointerEventType.BeginDrag, eventData, ReadDelta(eventData), onBeginDragWithData, onBeginDrag, onBeginDragEventWithData, onBeginDragEvent);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = ReadDelta(eventData);
            Publish(PointerEventType.Drag, eventData, delta);
            onDragWithData?.Invoke(eventData, delta);
            onDrag?.Invoke();
            onDragEventWithData.Invoke(eventData, delta);
            onDragEvent.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Dispatch(PointerEventType.EndDrag, eventData, ReadDelta(eventData), onEndDragWithData, onEndDrag, onEndDragEventWithData, onEndDragEvent);
        }

        private void Dispatch(
            PointerEventType eventType,
            PointerEventData eventData,
            Vector2 delta,
            UnityAction<PointerEventData> callbackWithData,
            UnityAction callback,
            PointerEventDataEvent unityEventWithData,
            UnityEvent unityEvent)
        {
            Publish(eventType, eventData, delta);
            callbackWithData?.Invoke(eventData);
            callback?.Invoke();
            unityEventWithData.Invoke(eventData);
            unityEvent.Invoke();
        }

        private void Publish(PointerEventType eventType, PointerEventData eventData, Vector2 delta)
        {
            if (!publishGlobally)
            {
                return;
            }

            UMessageCenter.Instance.Publish(
                new PointerEventMessage(eventType, gameObject, eventData, Is3DObject, delta),
                cacheIfNoSubscribers: false);
        }

        private static Vector2 ReadDelta(PointerEventData eventData)
        {
            return eventData == null ? Vector2.zero : eventData.delta;
        }
    }
}
