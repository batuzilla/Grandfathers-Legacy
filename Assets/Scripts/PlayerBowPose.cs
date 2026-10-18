using UnityEngine;

public class PlayerBowPose : MonoBehaviour
{
    public Texture2D sheet;
    public int cell = 16;
    public int drawRow = 4;
    public int releaseRow = 5;
    public int downColumn = 0;
    public int upColumn = 1;
    public int leftColumn = 2;
    public int rightColumn = 3;

    BowShooter shooter;
    SpriteRenderer sr;
    Sprite[,] pose;
    int cols;
    int rows;

    void Start()
    {
        shooter = GetComponent<BowShooter>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sheet == null || sr == null || sr.sprite == null) { enabled = false; return; }

        sheet.filterMode = FilterMode.Point;
        cols = sheet.width / cell;
        rows = sheet.height / cell;
        pose = new Sprite[rows, cols];

        float ppu = sr.sprite.pixelsPerUnit;
        Vector2 pivot = new Vector2(sr.sprite.pivot.x / sr.sprite.rect.width, sr.sprite.pivot.y / sr.sprite.rect.height);

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                var rect = new Rect(c * cell, sheet.height - (r + 1) * cell, cell, cell);
                pose[r, c] = Sprite.Create(sheet, rect, pivot, ppu);
            }
    }

    void LateUpdate()
    {
        if (shooter == null || pose == null) return;

        int row;
        if (shooter.Drawing) row = drawRow;
        else if (shooter.Releasing) row = releaseRow;
        else return;

        row = Mathf.Clamp(row, 0, rows - 1);
        Vector2 d = shooter.AimDir;
        int col;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) col = d.x > 0f ? rightColumn : leftColumn;
        else col = d.y > 0f ? upColumn : downColumn;

        sr.flipX = false;
        sr.sprite = pose[row, col];
    }
}
