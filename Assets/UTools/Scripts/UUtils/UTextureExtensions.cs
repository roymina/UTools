using System;
using UnityEngine;

namespace UTools
{
    public static class UTextureExtensions
    {
        public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit = 100f)
        {
            if (texture == null)
            {
                return null;
            }

            Vector2 pivot = new(0.5f, 0.5f);
            Rect rect = new(0f, 0f, texture.width, texture.height);
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }

        public static Texture2D ToTexture2D(this Sprite sprite)
        {
            if (sprite == null)
            {
                return null;
            }

            Texture2D readableTexture = sprite.texture.ToTexture2D();
            Rect spriteRect = sprite.textureRect;
            Texture2D targetTexture = new(
                Mathf.RoundToInt(spriteRect.width),
                Mathf.RoundToInt(spriteRect.height),
                TextureFormat.RGBA32,
                false);

            Color[] pixels = readableTexture.GetPixels(
                Mathf.RoundToInt(spriteRect.x),
                Mathf.RoundToInt(spriteRect.y),
                Mathf.RoundToInt(spriteRect.width),
                Mathf.RoundToInt(spriteRect.height));
            targetTexture.SetPixels(pixels);
            targetTexture.Apply();
            DestroyUnityObject(readableTexture);
            return targetTexture;
        }

        public static Texture2D ToTexture2D(this string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                return null;
            }

            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);

            byte[] data;
            try
            {
                data = Convert.FromBase64String(base64String);
            }
            catch (FormatException)
            {
                DestroyUnityObject(texture);
                return null;
            }

            if (!texture.LoadImage(data))
            {
                DestroyUnityObject(texture);
                return null;
            }

            return texture;
        }

        public static string ToBase64(this Texture2D texture)
        {
            if (texture == null)
            {
                return string.Empty;
            }

            byte[] bytes = texture.EncodeToPNG();
            return Convert.ToBase64String(bytes);
        }

        public static Texture2D ToTexture2D(this Texture texture)
        {
            if (texture == null)
            {
                return null;
            }

            int width = Mathf.Max(1, texture.width);
            int height = Mathf.Max(1, texture.height);
            Texture2D texture2D = new(width, height, TextureFormat.RGBA32, false);
            RenderTexture previousRenderTexture = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);

            try
            {
                Graphics.Blit(texture, renderTexture);
                RenderTexture.active = renderTexture;
                texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture2D.Apply();
            }
            finally
            {
                RenderTexture.active = previousRenderTexture;
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            return texture2D;
        }

        public static Texture2D DecodeBase64Image(string base64String)
        {
            return base64String.ToTexture2D();
        }

        private static void DestroyUnityObject(UnityEngine.Object unityObject)
        {
            if (unityObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(unityObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(unityObject);
            }
        }
    }
}
