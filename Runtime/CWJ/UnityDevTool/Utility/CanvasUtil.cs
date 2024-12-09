using UnityEngine;

namespace CWJ
{
    public static class CanvasUtil
    {
        /// <summary>
        /// Canvas(RenderMode.ScreenSpaceCamera) to world position
        /// </summary>
        /// <param name="screenPosition">mouse position</param>
        /// <param name="camera"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public static Vector3 CanvasToWorldPos_ScreenSpaceRenderMode(this Vector3 screenPosition, Camera camera, Canvas canvas)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceCamera) throw new UnityException(nameof(CanvasToWorldPos_ScreenSpaceRenderMode) + "() is only works on ScreenSpaceCamera canvas");
            screenPosition.z = canvas.planeDistance;
            return camera.ScreenToWorldPoint(screenPosition);
        }

        /// <summary>
        /// Canvas(RenderMode.WorldSpace) to world position
        /// </summary>
        /// <param name="screenPosition">mouse position</param>
        /// <param name="camera"></param>
        /// <param name="canvas"></param>
        /// <returns></returns>
        public static Vector3 CanvasToWorldPos_WorldSpaceRenderMode(this Vector3 screenPosition, Camera camera, Canvas canvas)
        {
            if (canvas.renderMode != RenderMode.WorldSpace) throw new UnityException(nameof(CanvasToWorldPos_WorldSpaceRenderMode) + "() is only works on WorldSpace canvas");
            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(canvas.transform.forward, canvas.transform.position);
            float distance;
            plane.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }

        public static Vector2 WorldToCanvasPosition(this Vector3 worldPosition, Camera camera, RectTransform canvasRect)
        {
            Vector2 canvasPos = camera.WorldToViewportPoint(worldPosition);

            canvasPos.x *= canvasRect.sizeDelta.x;
            canvasPos.y *= canvasRect.sizeDelta.y;

            canvasPos.x -= canvasRect.sizeDelta.x * canvasRect.pivot.x;
            canvasPos.y -= canvasRect.sizeDelta.y * canvasRect.pivot.y;

            return canvasPos;
        }

        public static Vector3 WorldToCanvasPosition(this Canvas canvas, Camera camera, Vector3 worldPosition)
        {
            var viewportPosition = camera.WorldToViewportPoint(worldPosition);
            return canvas.ViewportToCanvasPosition(viewportPosition);
        }

        public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition)
        {
            var viewportPosition = new Vector3(screenPosition.x / Screen.width,
                                               screenPosition.y / Screen.height,
                                               0);
            return canvas.ViewportToCanvasPosition(viewportPosition);
        }

        public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition)
        {
            var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
            var canvasRect = canvas.GetComponent<RectTransform>();
            var scale = canvasRect.sizeDelta;
            return Vector3.Scale(centerBasedViewPortPosition, scale);
        }
    }

}