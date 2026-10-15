using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SheetAnimator : MonoBehaviour
{
    public Texture2D sheet;
    public int cellWidth = 16;
    public int cellHeight = 16;
    public int frames = 4;
    public int row = 0;
    public float fps = 6f;
    public bool loop = true;
    public bool destroyAtEnd = false;

    SpriteRenderer sr;
    Sprite[] sprites;
    int cols;
    float t;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sheet == null) return;
        sprites = SheetUtil.Slice(sheet, cellWidth, cellHeight);
        cols = sheet.width / cellWidth;
        frames = Mathf.Min(frames, cols);
        t = loop ? Random.value * frames : 0f;
        Show(0);
    }

    void Update()
    {
        if (sprites == null) return;
        t += Time.deltaTime * fps;
        int i = Mathf.FloorToInt(t);
        if (loop) i %= frames;
        else if (i >= frames)
        {
            if (destroyAtEnd) Destroy(gameObject);
            return;
        }
        Show(i);
    }

    void Show(int frame)
    {
        sr.sprite = sprites[row * cols + frame];
    }
}
