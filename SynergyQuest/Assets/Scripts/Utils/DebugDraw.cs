using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
    /**
     * Utilities for drawing basic shapes and text for debugging purposes
     */
    public static class DebugDraw
    {
        /**
         * The default material used in SpriteRenderers
         */
        private static Lazy<Material> DefaultSpriteMaterial = new Lazy<Material>(() =>
        {
            // This is a bit hacky, but seems to be the only way to ensure one gets the default sprite renderer material:
            // We create a sprite renderer object, extract the material from it and then destroy the object again.
            
            var spriteObj = new GameObject();
            var spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();

            var material = spriteRenderer.sharedMaterial;
            
            Object.Destroy(spriteObj);

            return material;
        });
        
        /**
         * Draws the outline of a rectangle
         */
        public static void DrawRect(Vector3 lowerLeftCorner, float Width, float Height, Color color, float lineWidth = 0.05f)
        {
            var upperLeft = lowerLeftCorner + Vector3.up * Height;
            var upperRight = upperLeft + Vector3.right * Width;
            var lowerRight = lowerLeftCorner + Vector3.right * Width;
            
            DrawLine(
                color,
                lineWidth,
                true,
                float.PositiveInfinity,
                lowerLeftCorner,
                lowerRight,
                upperRight,
                upperLeft
            );
        }

        /**
         * Draws the outline of a circle
         */
        public static void DrawCircle(
            Vector3 center,
            float radius,
            Color color,
            int numSegments,
            float lineWidth = 0.05f,
            float timeout = float.PositiveInfinity)
        {
            // Based on https://gamedev.stackexchange.com/a/126429
            var theta = 0.0f;
            var deltaTheta = 2.0f * Mathf.PI / numSegments;

            var positions = Enumerable
                .Range(0, numSegments)
                .Select(_ =>
                {
                    var position = center + radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0.0f);
                    theta += deltaTheta;

                    return position;
                })
                .ToArray();
            
            DrawLine(color, lineWidth, true, timeout, positions);
        }

        /**
         * <summary>
         * Draws a line
         * </summary>
         * <param name="loop">if true, connect the first and last position, closing the line to a shape</param>
         * <param name="lifetime">How long the line will be display. Use <c>float.PositiveInfinity</c> if the line should never be removed.</param>
         */
        public static void DrawLine(Color color, float width, bool loop, float lifetime, params Vector3[] positions)
        {
            var lineObj = new GameObject("Debug Line");
            var lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.material = DefaultSpriteMaterial.Value;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            lineRenderer.positionCount = positions.Length;

            for (int i = 0; i < positions.Length; ++i)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
            
            lineRenderer.loop = loop;

            lineRenderer.startWidth = 1.0f;
            lineRenderer.endWidth = 1.0f;

            lineRenderer.widthMultiplier = width;

            if (!float.IsPositiveInfinity(lifetime))
            {
                GameObject.Destroy(lineObj, lifetime);
            }
        }

        /**
         * Draws text centered at the given position.
         */
        public static void DrawText(Vector3 position, String textStr, float height)
        {
            var textObj = new GameObject(textStr);
            textObj.transform.position = position;

            var textComponent = textObj.AddComponent<TextMeshPro>();
            textComponent.text = textStr;
            textComponent.enableAutoSizing = true;
            textComponent.fontSizeMin = 0.0f;
            textComponent.fontSizeMax = float.MaxValue;
            textComponent.color = Color.red;
            
            var rectTransform = (RectTransform) textObj.transform;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            
            textComponent.ForceMeshUpdate();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textComponent.textBounds.size.x);
            
            textComponent.UpdateMeshPadding();
        }
    }
}