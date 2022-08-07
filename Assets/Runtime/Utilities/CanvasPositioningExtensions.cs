using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFoundation.Utilities
{
    public static class CanvasPositioningExtensions
    {
        public static Vector2 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null)
        {
            camera = camera ?? Camera.main;
            var canvasRect = canvas.GetComponent<RectTransform>();
            var viewPosition = camera.WorldToViewportPoint(worldPosition);
            return new Vector2(
                viewPosition.x * canvasRect.sizeDelta.x - (canvasRect.sizeDelta.x * 0.5f),
                viewPosition.y * canvasRect.sizeDelta.y - (canvasRect.sizeDelta.y * 0.5f)
            );
        }
    }
}