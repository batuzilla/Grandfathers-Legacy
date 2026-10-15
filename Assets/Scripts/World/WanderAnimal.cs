using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WanderAnimal : MonoBehaviour
{
    public Texture2D sheet;
    public int cellWidth = 16;
    public int cellHeight = 16;
    public int frames = 2;
    public float fps = 3f;
    public float speed = 0.6f;
    public bool flipWhenLeft = true;

    public Vector2 areaCenter;
    public Vector2 areaHalfSize = new Vector2(4f, 3f);
    public Vector2 pauseRange = new Vector2(1f, 3.5f);
    public float bodyRadius = 0.3f;

    SpriteRenderer sr;
    Sprite[] sprites;
    Vector2 target;
    float pauseUntil;
    bool moving;
    float t;
    ContactFilter2D filter;
    readonly Collider2D[] buffer = new Collider2D[4];

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sheet == null) return;
        sprites = SheetUtil.Slice(sheet, cellWidth, cellHeight);
        frames = Mathf.Min(frames, sprites.Length);
        sr.sprite = sprites[0];
        filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = false;
        pauseUntil = Time.time + Random.Range(0f, pauseRange.y);
    }

    void Update()
    {
        if (sprites == null) return;

        if (!moving)
        {
            sr.sprite = sprites[0];
            if (Time.time < pauseUntil) return;
            PickTarget();
            return;
        }

        Vector2 pos = transform.position;
        Vector2 toTarget = target - pos;
        if (toTarget.magnitude < 0.05f) { StopMoving(); return; }

        Vector2 step = toTarget.normalized * speed * Time.deltaTime;
        if (Blocked(pos + step)) { StopMoving(); return; }

        transform.position = pos + step;
        if (flipWhenLeft && Mathf.Abs(step.x) > 0.0001f) sr.flipX = step.x < 0f;

        t += Time.deltaTime * fps;
        sr.sprite = sprites[Mathf.FloorToInt(t) % frames];
    }

    void PickTarget()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 p = areaCenter + new Vector2(
                Random.Range(-areaHalfSize.x, areaHalfSize.x),
                Random.Range(-areaHalfSize.y, areaHalfSize.y));
            if (Blocked(p)) continue;
            target = p;
            moving = true;
            return;
        }
        pauseUntil = Time.time + 1f;
    }

    void StopMoving()
    {
        moving = false;
        pauseUntil = Time.time + Random.Range(pauseRange.x, pauseRange.y);
    }

    bool Blocked(Vector2 p)
    {
        return Physics2D.OverlapCircle(p, bodyRadius, filter, buffer) > 0;
    }
}
