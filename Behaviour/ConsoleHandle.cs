using BepInEx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BepinExUtils.Console.Behaviour;

// Refs: https://github.com/Rellac-Rellac/unity-gui-windows/blob/main/Assets/GUIWindows/Scripts/GUIWindowHandle.cs
public class ConsoleHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Axis
    {
        Horizontal,
        Vertical,
        Diagonal
    }

    private const float MinWidth = 100f;
    private const float MinHeight = 100f;
    private Direction _direction;

    private Vector2 _initialMousePos;
    private Vector2 _initialPivot;
    private Vector2 _initialSize;
    private bool _isDragging;

    private RectTransform? _parentWindow;

    private void Awake()
    {
        var panel = transform.parent;
        if (!panel || !panel.TryGetComponent<RectTransform>(out var rectTransform)) return;
        _parentWindow = rectTransform;
    }

    private void Update()
    {
        if (!_parentWindow || !_isDragging)
            return;

        if (UnityInput.Current.GetMouseButtonUp(0))
        {
            _isDragging = false;
            _parentWindow.SetPivot(_initialPivot);
            return;
        }

        var scaleOffset = Vector2.one - (Vector2)transform.lossyScale + Vector2.one;
        var mouseDelta = Vector2.Scale((Vector2)UnityInput.Current.mousePosition - _initialMousePos, scaleOffset);
        var size = _initialSize;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_direction)
        {
            case Direction.Up:
                size += new Vector2(0, mouseDelta.y);
                break;
            case Direction.Down:
                size -= new Vector2(0, mouseDelta.y);
                break;
            case Direction.Left:
                size -= new Vector2(mouseDelta.x, 0);
                break;
            case Direction.Right:
                size += new Vector2(mouseDelta.x, 0);
                break;
            case Direction.UpRight:
                size += new Vector2(mouseDelta.x, mouseDelta.y);
                break;
            case Direction.UpLeft:
                size += new Vector2(-mouseDelta.x, mouseDelta.y);
                break;
            case Direction.DownRight:
                size += new Vector2(mouseDelta.x, -mouseDelta.y);
                break;
            case Direction.DownLeft:
                size += new Vector2(-mouseDelta.x, -mouseDelta.y);
                break;
        }

        if (size.x < MinWidth || size.y < MinHeight)
        {
            var newSize = size;
            if (size.x < MinWidth)
                newSize.x = MinWidth;
            if (size.y < MinHeight)
                newSize.y = MinHeight;
            _parentWindow.sizeDelta = newSize;
            return;
        }

        _parentWindow.sizeDelta = size;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Utils.Logger.Debug($"OnPointerDown: {_parentWindow} {eventData.button}");
        if (eventData.button != PointerEventData.InputButton.Left || !_parentWindow) return;
        eventData.dragging = true;
        _parentWindow.SetAsLastSibling();
        _isDragging = true;

        _initialMousePos = UnityInput.Current.mousePosition;
        _initialSize = _parentWindow.sizeDelta;
        _initialPivot = _parentWindow.pivot;

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_direction)
        {
            case Direction.Up:
                _parentWindow.SetPivot(new(0.5f, 0f));
                break;
            case Direction.Down:
                _parentWindow.SetPivot(new(0.5f, 1f));
                break;
            case Direction.Left:
                _parentWindow.SetPivot(new(1f, 0.5f));
                break;
            case Direction.Right:
                _parentWindow.SetPivot(new(0f, 0.5f));
                break;
            case Direction.UpRight:
                _parentWindow.SetPivot(new(0f, 0f));
                break;
            case Direction.UpLeft:
                _parentWindow.SetPivot(new(1f, 0f));
                break;
            case Direction.DownRight:
                _parentWindow.SetPivot(new(0f, 1f));
                break;
            case Direction.DownLeft:
                _parentWindow.SetPivot(new(1f, 1f));
                break;
        }

        _parentWindow.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        eventData.dragging = false;
        _isDragging = false;
    }

    internal void SetAxis(Axis axis)
    {
        if (!_parentWindow) return;
        _direction = axis switch
        {
            Axis.Horizontal => transform.position.x > _parentWindow.position.x ? Direction.Right : Direction.Left,
            Axis.Vertical => transform.position.y > _parentWindow.position.y ? Direction.Up : Direction.Down,
            Axis.Diagonal => transform.position.y > _parentWindow.position.y
                ? transform.position.x > _parentWindow.position.x ? Direction.UpRight : Direction.UpLeft
                : transform.position.x > _parentWindow.position.x
                    ? Direction.DownRight
                    : Direction.DownLeft,
            _ => _direction
        };
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }
}