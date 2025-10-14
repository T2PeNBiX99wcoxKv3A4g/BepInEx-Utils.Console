using BepInEx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BepinExUtils.Console.Behaviour;

public class ConsoleTitleBox : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _isDragging;
    private Vector2 _mouseOffset;
    private RectTransform? _parentWindow;

    private void Awake()
    {
        var panel = transform.parent;
        if (!panel || !panel.TryGetComponent<RectTransform>(out var rectTransform)) return;
        _parentWindow = rectTransform;
    }

    private void Update()
    {
        if (!_isDragging || !_parentWindow) return;
        _parentWindow.position = (Vector2)UnityInput.Current.mousePosition + _mouseOffset;
        if (!UnityInput.Current.GetMouseButtonUp(0)) return;
        _isDragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !_parentWindow) return;
        eventData.dragging = true;
        _mouseOffset = _parentWindow.position - UnityInput.Current.mousePosition;
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        eventData.dragging = false;
        _isDragging = false;
    }
}