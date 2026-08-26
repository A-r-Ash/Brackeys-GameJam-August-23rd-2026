using UnityEngine;
using UnityEngine.EventSystems;

// On-screen joystick. Drag the handle; read Direction (-1..1 on each axis) from PlayerController.
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;   // the joystick base
    [SerializeField] private RectTransform handle;       // the draggable knob
    [SerializeField] private float range = 60f;          // max handle travel in pixels

    public Vector2 Direction { get; private set; }

    public void OnPointerDown(PointerEventData e) => OnDrag(e);

    public void OnDrag(PointerEventData e)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, e.position, e.pressEventCamera, out pos);

        Vector2 clamped = Vector2.ClampMagnitude(pos, range);
        handle.anchoredPosition = clamped;
        Direction = clamped / range;   // normalized -1..1
    }

    public void OnPointerUp(PointerEventData e)
    {
        Direction = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
