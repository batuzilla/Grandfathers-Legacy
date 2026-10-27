using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BossAnimator : MonoBehaviour
{
    public Texture2D walkSheet;
    public Texture2D idleSheet;
    public Texture2D hitSheet;
    public int cell = 50;
    public float fps = 8f;
    public float moveThreshold = 0.05f;
    public float punchScale = 1.25f;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Health health;
    Sprite[] walk;
    Sprite[] idle;
    Sprite[] hit;
    float hitUntil;
    float punchUntil;
    float baseScale;
    Vector2 lastPos;
    float moving;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        baseScale = transform.localScale.x;
        if (walkSheet != null) walk = SheetUtil.Slice(walkSheet, cell, cell, 16f);
        if (idleSheet != null) idle = SheetUtil.Slice(idleSheet, cell, cell, 16f);
        if (hitSheet != null) hit = SheetUtil.Slice(hitSheet, cell, cell, 16f);
        lastPos = transform.position;
    }

    void OnEnable()
    {
        if (health != null) health.OnChanged += OnChanged;
    }

    void OnDisable()
    {
        if (health != null) health.OnChanged -= OnChanged;
    }

    void OnChanged(float current, float max)
    {
        hitUntil = Time.time + 0.25f;
    }

    public void Punch()
    {
        punchUntil = Time.time + 0.2f;
    }

    void LateUpdate()
    {
        Vector2 pos = transform.position;
        float speed = rb != null ? rb.linearVelocity.magnitude : (pos - lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPos = pos;
        moving = Mathf.Lerp(moving, speed, 10f * Time.deltaTime);

        Sprite[] set = moving > moveThreshold ? walk : idle;
        if (Time.time < hitUntil && hit != null && hit.Length > 0) set = hit;
        if (set == null || set.Length == 0) set = walk;
        if (set == null || set.Length == 0) return;

        int frame = Mathf.FloorToInt(Time.time * fps) % set.Length;
        sr.sprite = set[frame];
        sr.flipX = false;

        float s = baseScale * (Time.time < punchUntil ? punchScale : 1f);
        transform.localScale = new Vector3(s, s, 1f);
    }
}
