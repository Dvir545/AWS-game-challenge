using UnityEngine.EventSystems;

public class FloatingJoystick : Joystick
{
    protected override void Start()
    {
        base.Start();
        background.gameObject.SetActive(false);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!MobileKeyboardManager.Instance.IsMobileDevice() && eventData.button == PointerEventData.InputButton.Left)
            return;
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (!MobileKeyboardManager.Instance.IsMobileDevice() && eventData.button == PointerEventData.InputButton.Left)
            return;
        background.gameObject.SetActive(false);
        base.OnPointerUp(eventData);
    }
}