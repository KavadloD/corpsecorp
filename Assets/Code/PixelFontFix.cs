using TMPro;
using UnityEngine;

public class PixelFontFix : MonoBehaviour
{
    public TMP_FontAsset font;

    void Awake()
    {
        if (!font) return;

        foreach (var t in font.atlasTextures)
            if (t) t.filterMode = FilterMode.Point;
    }
}