using UnityEngine;

namespace BepinExUtils.Console.Extensions;

public static class RectTransformExtensions
{
    extension(RectTransform rectTransform)
    {
        public void SetPivot(Vector2 pivot)
        {
            var size = rectTransform.rect.size;
            var delatPivot = rectTransform.pivot - pivot;
            var deltaPosition = new Vector3(delatPivot.x * size.x, delatPivot.y * size.y);
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
    }
}