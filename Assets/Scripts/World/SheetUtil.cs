using UnityEngine;

public static class SheetUtil
{
    public static Sprite[] Slice(Texture2D tex, int cellW, int cellH, float ppu = 16f)
    {
        tex.filterMode = FilterMode.Point;
        int cols = tex.width / cellW;
        int rows = tex.height / cellH;
        var result = new Sprite[cols * rows];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                var rect = new Rect(c * cellW, tex.height - (r + 1) * cellH, cellW, cellH);
                result[r * cols + c] = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), ppu);
            }
        return result;
    }
}
