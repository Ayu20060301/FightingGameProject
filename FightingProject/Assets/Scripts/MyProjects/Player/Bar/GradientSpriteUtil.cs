using UnityEngine;

public static class GradientSpriteUtil
{
    public static Sprite Create(Gradient gradient, bool isOriginRight,out Texture2D texture)
    {
        const int width = 128;

        texture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };


        for(int i = 0; i < width; i++)
        {
            float t = i / (float)(width - 1);
            if(isOriginRight)
            {
                t = 1.0f - t;
            }

            texture.SetPixel(i, 0, gradient.Evaluate(t));
        }

        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, width, 1), new Vector2(0.5f, 0.5f));
    }

}
