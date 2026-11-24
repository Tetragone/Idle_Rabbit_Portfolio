using UnityEngine;

namespace Game.Common
{
    public static class MySpriteUtility
    {
        public static Sprite CropTransparentArea(Sprite source, byte alphaThreshold = 1)
        {
            if (source == null || source.texture == null)
            {
                return null;
            }

            Texture2D texture = source.texture;

            // Sprite가 텍스처에서 차지하는 영역
            Rect rect = source.rect;

            int startX = Mathf.RoundToInt(rect.x);
            int startY = Mathf.RoundToInt(rect.y);
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);

            Color32[] pixels = texture.GetPixels32();

            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int textureX = startX + x;
                    int textureY = startY + y;

                    Color32 pixel = pixels[textureY * texture.width + textureX];

                    if (pixel.a < alphaThreshold)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            // 완전히 투명한 Sprite인 경우
            if (maxX < minX || maxY < minY)
            {
                return null;
            }

            int croppedWidth = maxX - minX + 1;
            int croppedHeight = maxY - minY + 1;

            Rect croppedRect = new Rect(
                startX + minX,
                startY + minY,
                croppedWidth,
                croppedHeight
            );

            // 원래 Pivot 위치를 최대한 유지
            Vector2 newPivot = new Vector2(
                (source.pivot.x - minX) / croppedWidth,
                (source.pivot.y - minY) / croppedHeight
            );

            return Sprite.Create(
                texture,
                croppedRect,
                newPivot,
                source.pixelsPerUnit
            );
        }
    }
}
