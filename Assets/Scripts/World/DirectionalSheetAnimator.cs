using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DirectionalSheetAnimator : MonoBehaviour
{
    public Texture2D sheet;
    public int cell = 16;
    public float fps = 8f;
    public float moveThreshold = 0.05f;

    public int downColumn = 0;
    public int leftColumn = 1;
    public int upColumn = 2;
    public int rightColumn = 3;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Sprite[] sprites;
    int cols;
    int rows;
    int dirColumn;
    Vector2 lastPos;
    float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (sheet == null) return;
        sprites = SheetUtil.Slice(sheet, cell, cell);
        cols = sheet.width / cell;
        rows = sheet.height / cell;
        dirColumn = downColumn;
        lastPos = transform.position;
        sr.sprite = sprites[dirColumn];
    }

    void LateUpdate()
    {
        if (sprites == null) return;

        Vector2 pos = transform.position;
        Vector2 v = rb != null ? rb.linearVelocity : (pos - lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPos = pos;

        bool moving = v.sqrMagnitude > moveThreshold * moveThreshold;
        if (moving)
        {
            if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) dirColumn = v.x > 0f ? rightColumn : leftColumn;
            else dirColumn = v.y > 0f ? upColumn : downColumn;
            t += Time.deltaTime * fps;
        }
        else t = 0f;

        int frame = moving ? Mathf.FloorToInt(t) % rows : 0;
        sr.flipX = false;
        sr.sprite = sprites[frame * cols + dirColumn];
    }
}
